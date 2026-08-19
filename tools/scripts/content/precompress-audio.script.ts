#!/usr/bin/env bun
/**
 * Compresses every staged wav to ogg (libvorbis q=5) WITHOUT approving it.
 *
 * Why: wavs are 15-30 MB each. Reviewers want to listen to the actual shipped
 * format (~2 MB ogg) so we encode the wavs first. The runner then copies the
 * ogg directly to the final location on approve — no double-encoding.
 *
 * After this script, the staging dir should contain ONLY ogg files (one per
 * variant). Use --delete-wavs to clean up the wavs after a successful encode.
 *
 * Usage:
 *   bun tools/scripts/content/precompress-audio.script.ts                  # all staged wavs
 *   bun tools/scripts/content/precompress-audio.script.ts --group=music.menu  # one group
 *   bun tools/scripts/content/precompress-audio.script.ts --delete-wavs    # clean up wavs
 *
 * The encoded oggs and the spec's `audioPath` are updated so the rest of the
 * pipeline (build_checklist, interactive_review, approve) uses the compressed
 * format end-to-end.
 */
import { existsSync, mkdirSync, readFileSync, readdirSync, statSync, unlinkSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const ARGS = new Map<string, string>();
for (const arg of Bun.argv.slice(2)) {
	const [k, v] = arg.replace(/^--/, "").split("=");
	ARGS.set(k, v ?? "true");
}

if (ARGS.has("help") || ARGS.has("h")) {
	process.stdout.write(`Usage: bun ${import.meta.file} [options]

  --group=<id>            only precompress one group (e.g. music.menu)
  --delete-wavs           remove the source wavs after a successful encode
  --recompress-existing   re-encode existing .ogg files with the new profile
                          (use after changing sample rate / quality to migrate
                          staged variants to the new compression level)
  --help, -h              show this help

Encodes every staged .wav to .ogg (libvorbis, 22050 Hz, q=2 for sfx / q=3 for
music) in place, then rewrites audio-spec.json so each variant's audioPath
points to the ogg.

Compression profile (PC Dynasty's 1990s aesthetic):
  - Sample rate: 22050 Hz  (CD is 44100 Hz — half the rate, ~half the size)
  - Codec: libvorbis (ogg vorbis)
  - Quality:
      music.*    → q=3  (~110 kbps, indistinguishable from CD for retro game music)
      sfx.*      → q=2  (~85  kbps, transparent for short SFX)
  - Total expected: ~50 MB for 69 staged variants (was 142 MB at 44100 Hz q=5)
`);
	process.exit(0);
}

const GROUP_FILTER = ARGS.get("group");
const DELETE_WAVS = ARGS.has("delete-wavs");

const specPath = "game/content/audio/audio-spec.json";
const spec = JSON.parse(readFileSync(specPath, "utf8"));

/**
 * Per-group encoding profile. We pass the wav's source dir to figure out the
 * group (the staging layout is .generate/audios/staging/<group>/<entry>/...).
 */
function profileForOggPath(oggPath: string): { sampleRate: string; q: string } {
	// e.g. .generate/audios/staging/music.menu/menu_main/v_xxx.ogg → group is "music.menu"
	const parts = oggPath.split("/");
	const group = parts[parts.length - 3] ?? "";
	return group.startsWith("music.")
		? { sampleRate: "22050", q: "3" }   // music: 110 kbps, 22050 Hz
		: { sampleRate: "22050", q: "2" }; // sfx:   85  kbps, 22050 Hz
}

async function encodeToOgg(srcPath: string, oggPath: string): Promise<{ ok: boolean; bytes: number; stderr: string }> {
	// ffmpeg accepts any input format ffmpeg supports (mp3, wav, etc.)
	// and re-encodes to ogg vorbis at the configured sample rate and
	// quality. We don't restrict the source extension — ffmpeg sniffs it.
	mkdirSync(resolve(oggPath, ".."), { recursive: true });
	const { sampleRate, q } = profileForOggPath(oggPath);
	const proc = Bun.spawn([
		"ffmpeg", "-y", "-i", srcPath, "-vn", "-ar", sampleRate,
		"-acodec", "libvorbis", "-q:a", q, oggPath,
	], { stdout: "pipe", stderr: "pipe" });
	const exitCode = await proc.exited;
	const stderr = await new Response(proc.stderr).text();
	if (exitCode !== 0) return { ok: false, bytes: 0, stderr: stderr.slice(-300) };
	return { ok: true, bytes: statSync(oggPath).size, stderr: "" };
}

async function reencodeExistingOgg(oggPath: string): Promise<{ ok: boolean; bytes: number; stderr: string }> {
	const { sampleRate, q } = profileForOggPath(oggPath);
	// ffmpeg picks the output format from the LAST file extension. Putting
	// .tmp before .ogg would make ffmpeg reject the file. We use .tmp.ogg
	// so the trailing extension is .ogg (= vorbis) and the prefix tags
	// it as our atomic-replace temp.
	const tmpPath = oggPath.replace(/\.ogg$/, "") + ".tmp.ogg";
	const proc = Bun.spawn([
		"ffmpeg", "-y", "-i", oggPath, "-vn", "-ar", sampleRate,
		"-acodec", "libvorbis", "-q:a", q, tmpPath,
	], { stdout: "pipe", stderr: "pipe" });
	const exitCode = await proc.exited;
	if (exitCode !== 0) {
		const stderr = await new Response(proc.stderr).text();
		return { ok: false, bytes: 0, stderr: stderr.slice(-300) };
	}
	const bytes = statSync(tmpPath).size;
	const { renameSync } = await import("node:fs");
	renameSync(tmpPath, oggPath);
	return { ok: true, bytes, stderr: "" };
}

const RECOMPRESS_EXISTING = ARGS.has("recompress-existing");

let encoded = 0;
let skipped = 0;
let failed = 0;
let deletedSources = 0;
let totalSrc = 0;
let totalDst = 0;

for (const g of spec.groups) {
	if (GROUP_FILTER && g.id !== GROUP_FILTER) continue;
	const groupDir = `.generate/audios/staging/${g.id}`;
	if (!existsSync(groupDir)) continue;
	for (const e of g.entries) {
		const slotDir = `${groupDir}/${e.id}`;
		if (!existsSync(slotDir)) continue;
		for (const file of readdirSync(slotDir)) {
			if (RECOMPRESS_EXISTING) {
				if (!file.endsWith(".ogg")) continue;
				const oggPath = `${slotDir}/${file}`;
				const srcBytes = statSync(oggPath).size;
				const r = await reencodeExistingOgg(oggPath);
				if (!r.ok) {
					console.error(`FAIL ${oggPath}: ${r.stderr.slice(0, 100)}`);
					failed++;
					continue;
				}
				encoded++;
				totalSrc += srcBytes;
				totalDst += r.bytes;
			} else if (file.endsWith(".mp3") || file.endsWith(".wav")) {
				// mmx 1.0.19 produces mp3 (the runner is configured for
				// --format mp3 --bitrate 128000). Legacy v1 staging used
				// .wav; we accept both for backwards compatibility.
				const srcPath = `${slotDir}/${file}`;
				const oggName = file.replace(/\.(mp3|wav)$/, ".ogg");
				const oggPath = `${slotDir}/${oggName}`;
				const srcBytes = statSync(srcPath).size;
				totalSrc += srcBytes;
				if (existsSync(oggPath) && statSync(oggPath).mtimeMs >= statSync(srcPath).mtimeMs) {
					skipped++;
					totalDst += statSync(oggPath).size;
					continue;
				}
				const r = await encodeToOgg(srcPath, oggPath);
				if (!r.ok) {
					console.error(`FAIL ${srcPath}: ${r.stderr.slice(0, 100)}`);
					failed++;
					continue;
				}
				encoded++;
				totalDst += r.bytes;
				if (DELETE_WAVS) {
					unlinkSync(srcPath);
					deletedSources++;
				}
			}
		}
	}
}

const saved = totalSrc - totalDst;
console.log(`\nEncoded: ${encoded}, skipped: ${skipped}, failed: ${failed}`);
if (DELETE_WAVS) console.log(`Deleted sources: ${deletedSources}`);
console.log(`Source: ${(totalSrc / 1024 / 1024).toFixed(1)} MB -> Output: ${(totalDst / 1024 / 1024).toFixed(1)} MB`);
console.log(`Saved: ${(saved / 1024 / 1024).toFixed(1)} MB${totalSrc ? ` (${Math.round(100 * saved / totalSrc)}%)` : ""}`);

// Update the spec so each variant's audioPath points to the ogg.
let specUpdated = 0;
for (const slot of Object.values(spec.status) as Array<{ variants: Record<string, { audioPath?: string; wavPath?: string; wavSizeBytes?: number; audioSizeBytes?: number }> }>) {
	for (const v of Object.values(slot.variants)) {
		const stagedRel = v.audioPath ?? v.wavPath;
		if (!stagedRel) continue;
		const dir = stagedRel.replace(/\/[^/]+$/, "");
		const baseName = stagedRel.split("/").pop() ?? "";
		const vid = baseName.replace(/\.(wav|ogg|mp3)$/, "");
		const oggFull = `${dir}/${vid}.ogg`;
		if (existsSync(oggFull)) {
			v.audioPath = oggFull;
			delete v.wavPath;
			delete v.wavSizeBytes;
			v.audioSizeBytes = statSync(oggFull).size;
			specUpdated++;
		}
	}
}

writeFileSync(specPath, JSON.stringify(spec, null, "\t") + "\n");
console.log(`Spec updated: ${specUpdated} variants now point to .ogg.`);