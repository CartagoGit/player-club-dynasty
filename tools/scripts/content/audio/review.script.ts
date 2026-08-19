#!/usr/bin/env bun
/**
 * Interactive terminal review — bypasses VS Code preview entirely.
 *
 * Walks every staged ogg, plays it with the OS-native player, and prompts
 * for a decision: o (OK), b (bad), r (re-stage), s (skip). At the end,
 * prints a list of decisions and (with --commit) executes them.
 *
 * On WSL, the player is `open-audio.script.ts`, which delegates to the
 * Windows file association (VLC, Groove, browser) because WSL itself has
 * no /dev/snd. On Linux it's plain ffplay.
 *
 * Run with: `bun tools/scripts/content/audio/review.script.ts`
 * Optional flags:
 *   --group=music.menu     only review one group
 *   --player=mpv           override audio player
 *   --commit               execute the decisions at the end
 */

import { existsSync, readdirSync, statSync } from "node:fs";
import { resolve } from "node:path";

import { readAudioSpec } from "../../../helpers/content/audio/spec.helper.ts";
import { workspaceRoot } from "../../workspace/root.script.ts";

const ARGS = new Map<string, string>();
for (const arg of Bun.argv.slice(2)) {
	const [k, v] = arg.replace(/^--/, "").split("=");
	ARGS.set(k, v ?? "true");
}

if (ARGS.has("help") || ARGS.has("h")) {
	process.stdout.write(`Usage: bun ${import.meta.file} [options]

  --group=<id>     only review one group (e.g. music.menu)
  --player=<cmd>   override audio player
  --commit         execute the decisions at the end
  --help, -h       show this help

Walks every staged .ogg, plays it in the OS-native player, prompts
o/b/r/s, then (with --commit) runs the corresponding audio_approve /
audio_deprecate / audio_stage commands.
`);
	process.exit(0);
}

const GROUP_FILTER = ARGS.get("group");
const COMMIT = ARGS.has("commit");

// Detect WSL — without pulseaudio, ffplay can't actually produce sound.
const IS_WSL = existsSync("/proc/version") &&
	(await Bun.file("/proc/version").text()).toLowerCase().includes("microsoft");

// Detect WSLg's PulseAudio socket. When present, ffplay can play through
// the WSLg → Windows audio bridge (set PULSE_SERVER). When absent (older
// WSL or no pulseaudio), fall back to opening the file in the OS-native
// player via open-audio.script.ts.
const WSLG_PULSE = "/mnt/wslg/PulseServer";
const HAS_PULSE_BRIDGE = IS_WSL && existsSync(WSLG_PULSE);

const OPEN_AUDIO = resolve(import.meta.dir, "open-audio.script.ts");

const PLAYER_CMD = ARGS.get("player") ?? (HAS_PULSE_BRIDGE
	? "ffplay -hide_banner -autoexit -nodisp -loglevel error"
	: IS_WSL
		? `bun ${OPEN_AUDIO}`
		: "ffplay -hide_banner -autoexit -nodisp -loglevel error");

// Export PULSE_SERVER for the child ffplay process so it finds the WSLg sink.
if (HAS_PULSE_BRIDGE) {
	process.env.PULSE_SERVER = `unix:${WSLG_PULSE}`;
}

const spec = readAudioSpec(workspaceRoot);

interface Decision {
	slot: string;
	variant: string;
	action: "approve" | "deprecate" | "restage" | "skip";
	note?: string;
}

const decisions: Decision[] = [];

function prompt(question: string): Promise<string> {
	process.stdout.write(question);
	return new Promise((res) => {
		// `Bun.stdin` is a BunFile — it has no `once` and no `setRawMode`,
		// so the previous version threw on the first question. The line
		// stream we actually want is the Node-compatible `process.stdin`.
		const stdin = process.stdin;
		stdin.setRawMode?.(false);
		stdin.resume();
		stdin.once("data", (data: Buffer) => {
			stdin.pause();
			res(data.toString().trim());
		});
	});
}

async function play(audioPath: string) {
	console.log(`\n  ▶ Playing: ${audioPath}`);
	const playerArgs = PLAYER_CMD.split(" ");
	const proc = Bun.spawn([...playerArgs, audioPath], {
		stdout: "inherit",
		stderr: "pipe",
	});
	const exit = await proc.exited;
	if (exit !== 0) {
		console.error(`  (player exited ${exit})`);
	}
	if (IS_WSL && !HAS_PULSE_BRIDGE) {
		// open-audio returns immediately because the player runs detached
		// on the Windows side. Wait until the user presses Enter so the
		// decision prompt doesn't appear before the audio finishes.
		await prompt("(pulsa Enter cuando termine de sonar) ");
	}
}

for (const g of spec.groups) {
	if (GROUP_FILTER && g.id !== GROUP_FILTER) continue;
	const groupDir = `.generate/audios/staging/${g.id}`;
	if (!existsSync(groupDir)) continue;
	for (const e of g.entries) {
		const slotDir = `${groupDir}/${e.id}`;
		if (!existsSync(slotDir)) continue;
		const oggs = readdirSync(slotDir).filter((f) => f.endsWith(".ogg"));
		for (const o of oggs) {
			const slot = `${g.id}.${e.id}`;
			const variant = o.replace(/\.ogg$/, "");
			const audioPath = `${slotDir}/${variant}.ogg`;
			const oggSize = `${(statSync(audioPath).size / 1024).toFixed(0)} KB`;

			console.log(`\n========================================`);
			console.log(`Slot: ${slot}`);
			console.log(`Variant: ${variant} (${oggSize})`);
			console.log(`========================================`);

			await play(audioPath);

			const answer = await prompt("\nDecision? (o=OK / b=Bad / r=Re-stage / s=Skip): ");
			let action: Decision["action"];
			switch (answer.toLowerCase()) {
				case "o":
				case "ok":
					action = "approve";
					break;
				case "b":
				case "bad":
					action = "deprecate";
					break;
				case "r":
				case "restage":
					action = "restage";
					break;
				default:
					action = "skip";
			}
			let note: string | undefined;
			if (action === "restage") {
				note = await prompt("Note for re-stage prompt (optional): ");
			} else if (action === "deprecate") {
				// "b" for Bad maps to the `deprecate` action; asking for the
				// reason on the string the user typed instead of the action
				// we stored meant this prompt never appeared.
				note = await prompt("Note for why it's bad (optional): ");
			}
			decisions.push({ slot, variant, action, note });
		}
	}
}

console.log(`\n========================================`);
console.log(`Summary (${decisions.length} decisions):`);
console.log(`========================================`);
for (const d of decisions) {
	console.log(`  ${d.action.padEnd(10)} ${d.slot}  --variant ${d.variant}${d.note ? `  (note: ${d.note})` : ""}`);
}

if (!COMMIT) {
	console.log(`\n--- DRY-RUN. Para ejecutar: bun tools/scripts/content/audio/review.script.ts --commit ---`);
	console.log(`\nO dime "ejecuta el interactive review" y yo proceso las decisiones.`);
	process.exit(0);
}

// Execute
for (const d of decisions) {
	if (d.action === "skip") continue;
	const cmd = d.action === "approve"
		? `bun run game:audio:approve ${d.slot} --variant ${d.variant}`
		: d.action === "deprecate"
			? `bun run game:audio:deprecate ${d.slot} --variant ${d.variant}`
			: `bun run game:audio:stage ${d.slot}${d.note ? `  # note: ${d.note}` : ""}`;
	console.log(`\n$ ${cmd}`);
	const proc = Bun.spawn(["bash", "-c", cmd], { stdout: "inherit", stderr: "inherit" });
	const exit = await proc.exited;
	if (exit !== 0) console.error(`  FAIL exit ${exit}`);
}

// Regenerate the markdown checklist with the new state.
await Bun.spawn(["bun", "tools/scripts/content/build-audio-checklist.script.ts"]).exited;
console.log("\nOK checklist regenerated.");