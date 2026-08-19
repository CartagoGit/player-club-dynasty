#!/usr/bin/env bun
/**
 * open-audio — Opens an audio file (or any file) in the OS-native player.
 *
 * On WSL, the WSL gVisor kernel has no /dev/snd and no pulseaudio, so the
 * in-shell `ffplay`/`cvlc`/`aplay` produce no sound. This wrapper detects
 * WSL by reading /proc/version and, when on WSL, translates the path to a
 * Windows UNC path (via `wslpath -w`) and hands it to `cmd.exe /c start`.
 * That invokes the Windows shell, which respects the file association for
 * the extension (VLC for .ogg, Firefox for .md, etc.) — those DO have audio.
 *
 * On native Linux, it falls back to xdg-open.
 *
 * Usage:
 *   bun tools/scripts/content/open-audio.script.ts <file>
 *   bun tools/scripts/content/open-audio.script.ts --help
 */

import { existsSync, readFileSync } from "node:fs";
import { spawn } from "node:child_process";
import { resolve } from "node:path";

function detectWSL(): boolean {
	try {
		const v = readFileSync("/proc/version", "utf8").toLowerCase();
		return v.includes("microsoft") || v.includes("wsl");
	} catch {
		return false;
	}
}

function openInWindows(winPath: string, target: string): void {
	// WSL UNC paths (\\wsl.localhost\Ubuntu\...) are tricky: cmd's `start`
	// treats them as directories and opens the Explorer instead of the
	// file. Workaround: pass a file:// URL to a known browser. We try the
	// browsers in order of preference; the first installed one wins.
	const fileUrl = "file:///" + winPath.replace(/\\/g, "/").replace(/^\/+/, "");
	const candidates = ["msedge.exe", "chrome.exe", "brave.exe", "firefox.exe"];
	for (const browser of candidates) {
		const proc = spawn(browser, [fileUrl], { detached: true, stdio: "ignore" });
		proc.on("error", () => {}); // skip browsers not installed
		proc.unref();
	}
	// Also try the Windows file association as a fallback (works for files
	// on the Windows filesystem, not for WSL UNC paths).
	const fallback = spawn("cmd.exe", ["/c", "start", "", "/d", winPath], {
		detached: true,
		stdio: "ignore",
	});
	fallback.unref();
}

function openInLinux(target: string): void {
	const proc = spawn("xdg-open", [target], { detached: true, stdio: "ignore" });
	proc.unref();
}

const args = process.argv.slice(2);
if (args.length === 0 || args[0] === "--help" || args[0] === "-h") {
	process.stdout.write(`Usage: bun ${import.meta.file} <file>

Opens <file> in the OS-native player.
  - WSL:   cmd.exe /c start (Windows file association — VLC for .ogg, Firefox for .md)
  - Linux: xdg-open (DE file association)

This works for any file type, not just audio. The canonical uses are:
  - .ogg / .mp3 / .wav  →  default audio player (VLC, Groove, etc.)
  - .md / .html         →  default browser (Firefox, Chrome, Edge)
  - .txt                →  default text editor
`);
	process.exit(args.length === 0 ? 1 : 0);
}

const target = resolve(args[0]);
if (!existsSync(target)) {
	process.stderr.write(`open-audio: file not found: ${target}\n`);
	process.exit(1);
}

if (detectWSL()) {
	// wslpath -w changes /home/cartago/... → \\wsl$\Ubuntu\home\cartago\...
	// (or C:\Users\cartago\... if the file is on the Windows filesystem).
	const winPath = Bun.spawnSync(["wslpath", "-w", target], {
		stdout: "pipe",
		stderr: "pipe",
	});
	const out = new TextDecoder().decode(winPath.stdout).trim();
	if (!out) {
		process.stderr.write(`open-audio: wslpath failed: ${new TextDecoder().decode(winPath.stderr)}\n`);
		process.exit(1);
	}
	openInWindows(out, target);
	const ext = target.slice(target.lastIndexOf(".") + 1);
	const where = { ogg: "audio player (VLC)", mp3: "audio player", wav: "audio player", md: "browser", html: "browser" }[ext] ?? "Windows app";
	process.stdout.write(`✓ opened ${where}: ${out}\n`);
} else {
	openInLinux(target);
	process.stdout.write(`✓ opened in Linux: ${target}\n`);
}

