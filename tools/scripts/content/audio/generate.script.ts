#!/usr/bin/env bun
/**
 * tools/scripts/content/audio/generate.script.ts
 *
 * The PC Dynasty audio review pipeline.
 *
 *   1. STAGE    — generate a fresh variant in `.generate/audios/staging/` as an .mp3.
 *   2. LIST     — list pending + approved variants for a slot so the human
 *                 can listen to every candidate before deciding.
 *   3. APPROVE  — pick one variant, encode it to the slot's final format
 *                 (.ogg / .opus / etc.), move it to game/godot/Audio/...
 *                 and archive the previously approved variant under
 *                 .deprecated/assets/sounds/ so it can be restored later.
 *   4. DEPRECATE— move a staging variant or an approved asset into
 *                 .deprecated/assets/sounds/ without approving anything else.
 *
 *   bun tools/scripts/content/audio/generate.script.ts stage     sfx.ui.ui_click
 *   bun tools/scripts/content/audio/generate.script.ts stage     sfx.ui.ui_click --variant darker
 *   bun tools/scripts/content/audio/generate.script.ts list      sfx.ui.ui_click
 *   bun tools/scripts/content/audio/generate.script.ts list      --all
 *   bun tools/scripts/content/audio/generate.script.ts approve   sfx.ui.ui_click --variant v_1692345678
 *   bun tools/scripts/content/audio/generate.script.ts deprecate sfx.ui.ui_click --variant v_1692345678
 *   bun tools/scripts/content/audio/generate.script.ts status
 *   bun tools/scripts/content/audio/generate.script.ts --validate
 *
 * The staging directory (`.generate/audios/staging/`) and the deprecation directory
 * (`.deprecated/assets/sounds/`) are listed in `.gitignore`. The repo only
 * ever carries the small, compressed approved variants.
 */

import { spawn, spawnSync } from 'node:child_process';
import { copyFileSync, existsSync, mkdirSync, readFileSync, renameSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { dirname, isAbsolute, join, relative, resolve } from 'node:path';

import { workspaceRoot } from '../../workspace/root.script.ts';

const SPEC_RELATIVE = join('game', 'content', 'audio', 'audio-spec.json');
const SPEC_PATH = resolve(workspaceRoot, SPEC_RELATIVE);
const SPEC_DIR = dirname(SPEC_PATH);
const STAGING_RELATIVE = '.generate/audios/staging';
const DEPRECATED_RELATIVE = '.deprecated/assets/sounds';
const STAGING_ROOT = resolve(workspaceRoot, STAGING_RELATIVE);
const DEPRECATED_ROOT = resolve(workspaceRoot, DEPRECATED_RELATIVE);

const STALE_ERROR = (message: string): Error => {
	const error = new Error(message);
	error.name = 'AudioSpecError';
	return error;
};

type EntryState = 'pending' | 'generated' | 'rejected' | 'skipped';
type VariantState = 'staged' | 'approved' | 'deprecated';

interface IEntryDef {
	id: string;
	filename: string;
	prompt: string;
}

interface IVariantRecord {
	variantId: string;
	state: VariantState;
	stagedAt: string | null;
	approvedAt: string | null;
	deprecatedAt: string | null;
	audioPath?: string | null;
	wavPath?: string | null;
	finalPath: string | null;
	finalSizeBytes: number | null;
	audioSizeBytes?: number | null;
	wavSizeBytes?: number | null;
	lastPrompt: string;
}

interface ISlotStatus {
	approvedVariantId: string | null;
	variants: Record<string, IVariantRecord>;
}

interface ICoherence {
	[key: string]: unknown;
}

interface IGroup {
	id: string;
	kind: 'music' | 'sfx';
	outputDir: string;
	format: string;
	model: string;
	styleAnchor: string;
	coherence: ICoherence;
	entries: IEntryDef[];
}

interface IGlobalMusicAnchor {
	bpm: number;
	key: string;
	scaleNotes: string[];
	instruments: string;
	palette?: string;
	loopSeconds: number;
	transitionRules: string[];
}

interface IAudioSpec {
	version: number;
	root: string;
	stagingDir?: string;
	deprecatedDir?: string;
	tool: { command: string; nonInteractive: boolean; quiet: boolean };
	globalMusicAnchor?: IGlobalMusicAnchor;
	groups: IGroup[];
	status: Record<string, ISlotStatus>;
}

const ensureDir = (path: string): void => {
	mkdirSync(path, { recursive: true });
};

const readSpec = (): IAudioSpec => {
	if (!existsSync(SPEC_PATH)) {
		throw STALE_ERROR(`audio spec missing at ${SPEC_PATH}.`);
	}
	const parsed = JSON.parse(readFileSync(SPEC_PATH, 'utf8')) as IAudioSpec;
	if (!parsed.status) parsed.status = {};
	if (!parsed.stagingDir) parsed.stagingDir = STAGING_RELATIVE;
	if (!parsed.deprecatedDir) parsed.deprecatedDir = DEPRECATED_RELATIVE;
	return parsed;
};

const writeSpec = (spec: IAudioSpec): void => {
	ensureDir(SPEC_DIR);
	writeFileSync(SPEC_PATH, `${JSON.stringify(spec, null, 2)}\n`);
};

const findSlot = (
	spec: IAudioSpec,
	fullKey: string,
): { group: IGroup; entry: IEntryDef; status: ISlotStatus } => {
	const [groupId, entryId] = fullKey.includes('.') ? splitLastDot(fullKey) : [fullKey, ''];
	const group = spec.groups.find((candidate) => candidate.id === groupId);
	if (!group) throw STALE_ERROR(`group ${groupId} not declared in spec`);
	const entry = group.entries.find((candidate) => candidate.id === entryId);
	if (!entry) throw STALE_ERROR(`entry ${entryId} not declared in group ${groupId}`);
	const status = spec.status[fullKey] ?? { approvedVariantId: null, variants: {} };
	return { group, entry, status };
};

const splitLastDot = (value: string): [string, string] => {
	const index = value.lastIndexOf('.');
	if (index === -1) return [value, ''];
	return [value.slice(0, index), value.slice(index + 1)];
};

const ensureSlotStatus = (spec: IAudioSpec, fullKey: string): ISlotStatus => {
	const status = spec.status[fullKey];
	if (status) return status;
	const created: ISlotStatus = { approvedVariantId: null, variants: {} };
	spec.status[fullKey] = created;
	return created;
};

const assertMusicAnchor = (group: IGroup, anchor: IGlobalMusicAnchor): void => {
	if (group.kind !== 'music') return;
	// The v2 prompts embed the authoritative BPM/key in the prompt itself, so
	// different music groups (menu, workspace, editor...) can have different
	// BPMs without breaking crossfade — as long as they share the global anchor's
	// KEY and instrument palette. We only validate the key.
	if (anchor.key && group.coherence.key && typeof group.coherence.key === 'string') {
		const groupKey = String(group.coherence.key).toLowerCase();
		const anchorKey = anchor.key.toLowerCase();
		// Accept if group's key starts with the anchor key (e.g. "C major" matches "C major (Ionian)")
		if (!groupKey.startsWith(anchorKey.split(' ')[0])) {
			throw STALE_ERROR(
				`group ${group.id} declares key=${group.coherence.key} but anchor key=${anchor.key}`,
			);
		}
	}
};

/**
 * Build the final mmx prompt by concatenating the entry-specific prompt with
 * the group-level and shared DNA. The order is:
 *   1. <group>/<entry>.txt  (the actual instruction for this variant)
 *   2. <group>/_variant.txt (the per-group sonic identity)
 *   3. _shared/sonic-identity.txt (music only — the global cross-group DNA)
 *
 * If `.generate/prompts/audios/<group>/<entry>.txt` does not exist, fall back to the prompt
 * stored in the audio-spec.json (legacy v1 behavior). The shared/group
 * identity is still appended so the new sonic DNA is always present.
 */
const buildFinalPrompt = (groupId: string, entryId: string, fallbackPrompt: string): string => {
	const parts: string[] = [];
	const entryPromptPath = resolve(workspaceRoot, `.generate/prompts/audios/${groupId}/${entryId}.txt`);
	const groupVariantPath = resolve(workspaceRoot, `.generate/prompts/audios/${groupId}/_variant.txt`);
	const sharedPath = resolve(workspaceRoot, '.generate/prompts/audios/_shared/sonic-identity.txt');

	if (existsSync(entryPromptPath)) {
		parts.push(readFileSync(entryPromptPath, 'utf8').trim());
	} else {
		parts.push(fallbackPrompt);
	}
	if (existsSync(groupVariantPath)) {
		parts.push('\n\n--- GROUP IDENTITY ---\n\n' + readFileSync(groupVariantPath, 'utf8').trim());
	}
	// Shared sonic identity only applies to music (not SFX).
	if (groupId.startsWith('music.') && existsSync(sharedPath)) {
		parts.push('\n\n--- SHARED SONIC IDENTITY (PC DYNASTY) ---\n\n' + readFileSync(sharedPath, 'utf8').trim());
	}
	return parts.join('');
};

const buildCommand = (
	group: IGroup,
	entry: IEntryDef,
	stagedOutPath: string,
	toolCommand: string,
	nonInteractive: boolean,
	quiet: boolean,
	anchor: IGlobalMusicAnchor | undefined,
): string[] => {
	const argv: string[] = [toolCommand];
	if (group.kind === 'music') {
		argv.push('music', 'generate', '--instrumental');
	} else {
		// SFX groups also use `music generate --instrumental` because
		// `mmx speech synthesize` requires --text, which makes no sense
		// for non-vocal UI / match stings. We pass the SFX coherence as
		// music-style flags (instruments/mood) so the chip aesthetic
		// from the original spec still comes through.
		argv.push('music', 'generate', '--instrumental');
	}
	if (nonInteractive) argv.push('--non-interactive');
	if (quiet) argv.push('--quiet');
	argv.push('--model', group.model);
	// mmx doesn't support ogg output. We use mp3 128k as the staging format
	// (the API doesn't accept lower bitrates). The precompress script then
	// encodes the mp3 to ogg vorbis 22050Hz q=2/q=3 for the final asset.
	argv.push('--format', 'mp3');
	argv.push('--bitrate', '128000');
	argv.push('--out', stagedOutPath);
	if (group.kind === 'music') {
		// mmx 1.0.19+ accepts --bpm/--key as hints; the spec prompt embeds the
		// authoritative BPM/key per slot. We pass the group's coherence.bpm/key
		// as hints so music-3.0 has context, but the prompt wins if there's a
		// conflict (which happens for variants with different BPM — we only pass
		// the group's anchor value, so prompt + flag should agree).
		if (group.coherence.bpm) argv.push('--bpm', String(group.coherence.bpm));
		if (group.coherence.key) argv.push('--key', String(group.coherence.key));
		if (group.coherence.instruments) argv.push('--instruments', String(group.coherence.instruments));
		if (group.coherence.mood) argv.push('--mood', String(group.coherence.mood));
		if (group.coherence.tempo) argv.push('--tempo', String(group.coherence.tempo));
	} else {
		// SFX pass through `speed` / `volume` as `tempo` / `mood` hints so
		// they survive the music-3.0 pipe. The prompt is the load-bearing
		// input — the coherence is supplementary.
		if (group.coherence.speed !== undefined) {
			argv.push('--tempo', `${String(group.coherence.speed)}x speed`);
		}
		if (group.coherence.volume !== undefined) {
			argv.push('--mood', `volume ${String(group.coherence.volume)}`);
		}
		if (typeof group.coherence['sound-effect'] === 'string') {
			argv.push('--instruments', String(group.coherence['sound-effect']));
		}
	}
	const finalPrompt = buildFinalPrompt(group.id, entry.id, entry.prompt);
	argv.push('--prompt', finalPrompt);
	return argv;
};

const runCommand = (argv: string[]): { ok: boolean; stderr: string } => {
	const result = spawnSync(argv[0] ?? '', argv.slice(1), {
		stdio: ['ignore', 'inherit', 'pipe'],
		env: process.env,
	});
	const stderr = result.stderr ? result.stderr.toString() : '';
	if (result.status !== 0 && stderr.includes('Music API is no longer available to new users')) {
		return {
			ok: false,
			stderr:
				'mmx auth is valid, but this account is not entitled to the legacy Music API (HTTP 410). Migrate this pipeline to MiniMax Audio or another backend before attempting stage again.',
		};
	}
	if (result.status !== 0 && stderr.includes('Token Plan usage limit reached')) {
		return {
			ok: false,
			stderr:
				'mmx auth is valid, but the Token Plan for music is exhausted. Wait for reset or top up quota before attempting stage again.',
		};
	}
	return { ok: result.status === 0, stderr };
};

const variantIdFor = (timestamp: number): string => `v_${timestamp}`;

const stage = (fullKey: string, suffix: string | null): number => {
	const spec = readSpec();
	const { group, entry } = findSlot(spec, fullKey);
	if (spec.globalMusicAnchor) assertMusicAnchor(group, spec.globalMusicAnchor);
	const slotDir = join(STAGING_ROOT, group.id, entry.id);
	ensureDir(slotDir);
	const variantId = suffix ?? variantIdFor(Date.now());
	// mmx writes whatever --format says (mp3) but uses the path's extension
	// for the on-disk name. We always use .mp3 because both music and SFX
	// are written as 128kbps mp3 by this script.
	const ext = 'mp3';
	const stagedName = `${variantId}.${ext}`;
	const stagedPath = join(slotDir, stagedName);
	const argv = buildCommand(
		group,
		entry,
		stagedPath,
		spec.tool.command,
		spec.tool.nonInteractive,
		spec.tool.quiet,
		spec.globalMusicAnchor,
	);
	process.stdout.write(`stage ${fullKey} → ${stagedPath}\n  $ ${argv.join(' ')}\n`);
	const { ok, stderr } = runCommand(argv);
	if (!ok) {
		process.stderr.write(`mmx failed: ${stderr.trim()}\n`);
		rmSync(stagedPath, { force: true });
		return 1;
	}
	const stats = statSync(stagedPath);
	const slot = ensureSlotStatus(spec, fullKey);
	slot.variants[variantId] = {
		variantId,
		state: 'staged',
		stagedAt: new Date().toISOString(),
		approvedAt: null,
		deprecatedAt: null,
		audioPath: relative(workspaceRoot, stagedPath),
		wavPath: null,
		finalPath: null,
		finalSizeBytes: null,
		audioSizeBytes: stats.size,
		wavSizeBytes: null,
		lastPrompt: entry.prompt,
	};
	writeSpec(spec);
	process.stdout.write(
		`\n✓ staged ${fullKey} variant ${variantId} (${(stats.size / 1024).toFixed(1)} KB ${ext})\n` +
			`  listen with: bun tools/scripts/content/audio/generate.script.ts listen ${fullKey} --variant ${variantId}\n`,
	);
	return 0;
};

const listen = (fullKey: string, variantId: string): number => {
	const spec = readSpec();
	const slot = spec.status[fullKey];
	if (!slot) throw STALE_ERROR(`slot ${fullKey} has no staged variants`);
	const variant = slot.variants[variantId];
	if (!variant) throw STALE_ERROR(`variant ${variantId} not found in slot ${fullKey}`);
	const stagedPath = variant.audioPath ?? variant.wavPath;
	if (!stagedPath) throw STALE_ERROR(`variant ${variantId} has no staged audio`);
	const stagedAbs = isAbsolute(stagedPath)
		? stagedPath
		: resolve(workspaceRoot, stagedPath);
	if (!existsSync(stagedAbs)) {
		throw STALE_ERROR(`staged audio missing on disk: ${stagedAbs}`);
	}
	process.stdout.write(`${stagedAbs}\n`);
	return 0;
};

const list = (fullKey: string | null, all: boolean): number => {
	const spec = readSpec();
	const slots = all ? Object.entries(spec.status) : [[fullKey, spec.status[fullKey ?? '']]] as Array<
		[string, ISlotStatus | undefined]
	>;
	const rows: string[] = ['| slot | variant | state | staged | final |'];
	rows.push('|---|---|---|---|---|');
	for (const [slotKey, slot] of slots) {
		if (!slot) {
			rows.push(`| ${slotKey} | — | (no variants yet) | — | — |`);
			continue;
		}
		const variants = Object.values(slot.variants).sort((a, b) =>
			(a.variantId < b.variantId ? 1 : -1),
		);
		if (variants.length === 0) {
			rows.push(`| ${slotKey} | — | (no variants yet) | — | — |`);
			continue;
		}
		for (const variant of variants) {
			const approved = slot.approvedVariantId === variant.variantId ? ' ✓' : '';
			rows.push(
				`| ${slotKey}${approved} | ${variant.variantId} | ${variant.state} | ${variant.audioPath ?? variant.wavPath ?? '—'} | ${variant.finalPath ?? '—'} |`,
			);
		}
	}
	process.stdout.write(`${rows.join('\n')}\n`);
	return 0;
};

const encodeToTargetFormat = (
	wavAbs: string,
	targetAbs: string,
	format: string,
): { ok: boolean; stderr: string; bytes: number | null } => {
	// ffmpeg is the universal, dependency-free encoder we expect on PATH.
	// We probe for it once; if it is missing, surface a clear error.
	const probe = spawnSync('ffmpeg', ['-version'], { encoding: 'utf8' });
	if (probe.status !== 0) {
		return {
			ok: false,
			stderr: 'ffmpeg is required to encode the approved variant. Install it or set PCM_OGG_ENCODER.',
			bytes: null,
		};
	}
	const codec = format === 'mp3' ? 'libmp3lame' : 'libvorbis';
	const result = spawnSync(
		'ffmpeg',
		['-y', '-i', wavAbs, '-vn', '-acodec', codec, '-q:a', '5', targetAbs],
		{ stdio: ['ignore', 'inherit', 'pipe'], encoding: 'utf8' },
	);
	if (result.status !== 0) {
		return { ok: false, stderr: result.stderr ?? 'unknown ffmpeg error', bytes: null };
	}
	const bytes = existsSync(targetAbs) ? statSync(targetAbs).size : null;
	return { ok: true, stderr: '', bytes };
};

const approve = (fullKey: string, variantId: string): number => {
	const spec = readSpec();
	const { group, entry } = findSlot(spec, fullKey);
	const slot = ensureSlotStatus(spec, fullKey);
	const variant = slot.variants[variantId];
	if (!variant) throw STALE_ERROR(`variant ${variantId} not found in slot ${fullKey}`);
	if (variant.state === 'deprecated') {
		throw STALE_ERROR(`variant ${variantId} is deprecated; re-stage it first`);
	}
	// The variant's audio file (ogg vorbis) is the only staged asset. Use
	// audioPath (post-v2 spec), falling back to legacy wavPath if audioPath
	// is missing.
	const stagedPath = variant.audioPath ?? variant.wavPath;
	if (!stagedPath) throw STALE_ERROR(`variant ${variantId} has no staged audio`);
	const stagedAbs = isAbsolute(stagedPath)
		? stagedPath
		: resolve(workspaceRoot, stagedPath);
	if (!existsSync(stagedAbs)) {
		throw STALE_ERROR(`staged audio missing on disk: ${stagedAbs}`);
	}
	// The final destination filename has the extension the group declares
	// (e.g. ".ogg", ".mp3", ".opus"). The staged file already matches that
	// extension; if it doesn't, fall back to re-encoding.
	const stagedExt = stagedAbs.slice(stagedAbs.lastIndexOf('.') + 1);
	const finalExt = group.format;

	const finalDir = resolve(join(workspaceRoot, spec.root, group.outputDir));
	ensureDir(finalDir);
	const finalAbs = join(finalDir, entry.filename);

	// archive the previously approved variant, if any, before we overwrite.
	if (slot.approvedVariantId && slot.approvedVariantId !== variantId) {
		const previous = slot.variants[slot.approvedVariantId];
		if (previous?.finalPath) {
			const previousAbs = isAbsolute(previous.finalPath)
				? previous.finalPath
				: resolve(workspaceRoot, previous.finalPath);
			if (existsSync(previousAbs)) {
				const archiveDir = join(DEPRECATED_ROOT, group.id, entry.id);
				ensureDir(archiveDir);
				const stamp = new Date().toISOString().replace(/[:.]/g, '-');
				const archivePath = join(archiveDir, `${slot.approvedVariantId}__${stamp}__${entry.filename}`);
				renameSync(previousAbs, archivePath);
				previous.state = 'deprecated';
				previous.deprecatedAt = new Date().toISOString();
				previous.finalPath = relative(workspaceRoot, archivePath);
			}
		}
	}

	let bytes: number | null = null;
	let usedCopy = false;
	if (stagedExt === finalExt) {
		// Same format — just copy.
		copyFileSync(stagedAbs, finalAbs);
		bytes = statSync(finalAbs).size;
		usedCopy = true;
	} else {
		// Format mismatch — re-encode.
		const encoding = encodeToTargetFormat(stagedAbs, finalAbs, group.format);
		if (!encoding.ok) {
			process.stderr.write(`encoding failed: ${encoding.stderr}\n`);
			return 1;
		}
		bytes = encoding.bytes;
	}

	variant.state = 'approved';
	variant.approvedAt = new Date().toISOString();
	variant.finalPath = relative(workspaceRoot, finalAbs);
	variant.finalSizeBytes = bytes;
	slot.approvedVariantId = variantId;
	writeSpec(spec);

	process.stdout.write(
		`\n✓ approved ${fullKey} variant ${variantId}\n` +
			`  staged: ${variant.audioPath ?? variant.wavPath} (${(variant.audioSizeBytes ?? variant.wavSizeBytes) ? `${(((variant.audioSizeBytes ?? variant.wavSizeBytes) ?? 0) / 1024).toFixed(0)} KB` : "?"})\n` +
			`  final:  ${variant.finalPath} (${((bytes ?? 0) / 1024).toFixed(1)} KB ${group.format})${usedCopy ? " (copied)" : " (re-encoded)"}\n`,
	);
	return 0;
};

const deprecate = (fullKey: string, variantId: string): number => {
	const spec = readSpec();
	const slot = spec.status[fullKey];
	if (!slot) throw STALE_ERROR(`slot ${fullKey} has no status`);
	const variant = slot.variants[variantId];
	if (!variant) throw STALE_ERROR(`variant ${variantId} not found in slot ${fullKey}`);
	const { group, entry } = findSlot(spec, fullKey);

	const archiveDir = join(DEPRECATED_ROOT, group.id, entry.id);
	ensureDir(archiveDir);
	const stamp = new Date().toISOString().replace(/[:.]/g, '-');

	if (variant.finalPath) {
		const finalAbs = isAbsolute(variant.finalPath)
			? variant.finalPath
			: resolve(workspaceRoot, variant.finalPath);
		if (existsSync(finalAbs)) {
			const archivePath = join(archiveDir, `${variantId}__${stamp}__${entry.filename}`);
			renameSync(finalAbs, archivePath);
			variant.finalPath = relative(workspaceRoot, archivePath);
		}
	}
	if (variant.audioPath) {
		const audioAbs = isAbsolute(variant.audioPath)
			? variant.audioPath
			: resolve(workspaceRoot, variant.audioPath);
		if (existsSync(audioAbs)) {
			const baseFile = variant.audioPath.split('/').pop() ?? `${variantId}.mp3`;
			const archivePath = join(archiveDir, `${variantId}__${stamp}__${baseFile}`);
			renameSync(audioAbs, archivePath);
			variant.audioPath = relative(workspaceRoot, archivePath);
		}
	}
	if (variant.wavPath) {
		// Legacy: pre-v2 spec had a wavPath. New spec only has audioPath. Keep
		// this branch for back-compat with old status.json snapshots.
		const wavAbs = isAbsolute(variant.wavPath)
			? variant.wavPath
			: resolve(workspaceRoot, variant.wavPath);
		if (existsSync(wavAbs)) {
			const baseFile = variant.wavPath.split('/').pop() ?? `${variantId}.wav`;
			const archivePath = join(archiveDir, `${variantId}__${stamp}__${baseFile}`);
			renameSync(wavAbs, archivePath);
			variant.wavPath = relative(workspaceRoot, archivePath);
		}
	}
	variant.state = 'deprecated';
	variant.deprecatedAt = new Date().toISOString();
	if (slot.approvedVariantId === variantId) slot.approvedVariantId = null;
	writeSpec(spec);
	process.stdout.write(`✓ deprecated ${fullKey} variant ${variantId}\n`);
	return 0;
};

const statusReport = (): number => {
	const spec = readSpec();
	const slots = Object.entries(spec.status);
	if (slots.length === 0) {
		process.stdout.write('No audio slots have any variants yet. Run `stage <slot>` first.\n');
		return 0;
	}
	const rows: string[] = ['| slot | approved | staged | deprecated |'];
	rows.push('|---|---|---|---|');
	for (const [slotKey, slot] of slots) {
		const counts = { staged: 0, approved: 0, deprecated: 0 };
		for (const variant of Object.values(slot.variants)) counts[variant.state] += 1;
		rows.push(
			`| ${slotKey} | ${slot.approvedVariantId ?? '—'} | ${counts.staged} | ${counts.deprecated} |`,
		);
	}
	process.stdout.write(`${rows.join('\n')}\n`);
	return 0;
};

const validateSpec = (): number => {
	const spec = readSpec();
	if (spec.groups.some((group) => group.kind === 'music') && !spec.globalMusicAnchor) {
		process.stderr.write('audio spec has no globalMusicAnchor — music crossfades would clash.\n');
		return 1;
	}
	for (const group of spec.groups) {
		if (group.kind !== 'music') continue;
		try {
			assertMusicAnchor(group, spec.globalMusicAnchor as IGlobalMusicAnchor);
		} catch (error) {
			process.stderr.write(`${(error as Error).message}\n`);
			return 1;
		}
	}
	const orphan = Object.keys(spec.status).filter((key) => {
		const [groupId, entryId] = splitLastDot(key);
		const group = spec.groups.find((candidate) => candidate.id === groupId);
		return !group || !group.entries.some((entry) => entry.id === entryId);
	});
	if (orphan.length > 0) {
		process.stderr.write(`orphan status slots: ${orphan.join(', ')}\n`);
		return 1;
	}
	process.stdout.write('audio spec: ok\n');
	return 0;
};

const printHelp = (): void => {
	process.stdout.write(
		[
			'PC Dynasty audio review pipeline',
			'',
			'Commands:',
			'  stage     <slot> [--variant <id>]   Generate a new variant in .generate/audios/staging/.',
			'  list      [<slot>] [--all]          List staged / approved variants.',
			'  listen    <slot> --variant <id>     Print the path to the staged wav (open it in any player).',
			'  approve   <slot> --variant <id>     Encode the staged wav to the slot format and move it to game/godot/Audio/...; the previously approved variant is archived in .deprecated/.',
			'  deprecate <slot> --variant <id>     Move a staged or approved variant to .deprecated/assets/sounds/.',
			'  status                              Print a summary of every slot.',
			'  --validate                          Validate the spec (anchor, coherence, status consistency).',
			'',
			'A slot is written as "<group>.<entry>", e.g. "sfx.ui.ui_click" or "music.menu.menu_main".',
			'Staged files live in .generate/audios/staging/<group>/<entry>/<variantId>.mp3 and are NOT committed.',
			'Approved files live in game/godot/Audio/<Music|SFX>/<filename> and ARE committed.',
			'Archived files live in .deprecated/assets/sounds/<group>/<entry>/<variantId>__<ts>__<filename>.',
		].join('\n'),
	);
};

const main = async (): Promise<number> => {
	const argv = process.argv.slice(2);
	if (argv.length === 0 || argv.includes('--help') || argv.includes('-h')) {
		printHelp();
		return 0;
	}
	const command = argv[0];
	const positional = argv[1] ?? null;
	const variantIndex = argv.indexOf('--variant');
	const variantId = variantIndex !== -1 ? (argv[variantIndex + 1] ?? null) : null;
	switch (command) {
		case 'stage':
			if (!positional) {
				process.stderr.write('stage requires a slot, e.g. stage sfx.ui.ui_click\n');
				return 2;
			}
			return stage(positional, variantId);
		case 'list':
			return list(positional, argv.includes('--all'));
		case 'listen':
			if (!positional || !variantId) {
				process.stderr.write('listen requires a slot and --variant <id>\n');
				return 2;
			}
			return listen(positional, variantId);
		case 'approve':
			if (!positional || !variantId) {
				process.stderr.write('approve requires a slot and --variant <id>\n');
				return 2;
			}
			return approve(positional, variantId);
		case 'deprecate':
			if (!positional || !variantId) {
				process.stderr.write('deprecate requires a slot and --variant <id>\n');
				return 2;
			}
			return deprecate(positional, variantId);
		case 'status':
			return statusReport();
		case '--validate':
			return validateSpec();
		default:
			process.stderr.write(`unknown command: ${command}\n`);
			printHelp();
			return 2;
	}
};

process.exit(await main());

void spawn;