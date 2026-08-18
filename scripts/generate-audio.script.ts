#!/usr/bin/env bun
/**
 * scripts/generate-audio.script.ts
 *
 * The PC Dynasty audio review pipeline.
 *
 *   1. STAGE    — generate a fresh variant in `.audio-staging/` as a large .wav.
 *   2. LIST     — list pending + approved variants for a slot so the human
 *                 can listen to every candidate before deciding.
 *   3. APPROVE  — pick one variant, encode it to the slot's final format
 *                 (.ogg / .opus / etc.), move it to game/godot/Audio/...
 *                 and archive the previously approved variant under
 *                 .deprecated/assets/sounds/ so it can be restored later.
 *   4. DEPRECATE— move a staging variant or an approved asset into
 *                 .deprecated/assets/sounds/ without approving anything else.
 *
 *   bun scripts/generate-audio.script.ts stage     sfx.ui.ui_click
 *   bun scripts/generate-audio.script.ts stage     sfx.ui.ui_click --variant darker
 *   bun scripts/generate-audio.script.ts list      sfx.ui.ui_click
 *   bun scripts/generate-audio.script.ts list      --all
 *   bun scripts/generate-audio.script.ts approve   sfx.ui.ui_click --variant v_1692345678
 *   bun scripts/generate-audio.script.ts deprecate sfx.ui.ui_click --variant v_1692345678
 *   bun scripts/generate-audio.script.ts status
 *   bun scripts/generate-audio.script.ts --validate
 *
 * The staging directory (`.audio-staging/`) and the deprecation directory
 * (`.deprecated/assets/sounds/`) are listed in `.gitignore`. The repo only
 * ever carries the small, compressed approved variants.
 */

import { spawn, spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, readFileSync, renameSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { dirname, isAbsolute, join, relative, resolve } from 'node:path';

import { workspaceRoot } from './mcp-vertex-root.script.ts';

const SPEC_RELATIVE = join('game', 'godot', 'Audio', 'audio-spec.json');
const SPEC_PATH = resolve(workspaceRoot, SPEC_RELATIVE);
const SPEC_DIR = dirname(SPEC_PATH);
const STAGING_RELATIVE = '.audio-staging';
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
	wavPath: string | null;
	finalPath: string | null;
	finalSizeBytes: number | null;
	wavSizeBytes: number | null;
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
	const bpm = Number(group.coherence.bpm);
	if (Number.isFinite(bpm) && bpm !== anchor.bpm) {
		throw STALE_ERROR(
			`group ${group.id} declares bpm=${bpm} but anchor bpm=${anchor.bpm}`,
		);
	}
};

const buildCommand = (
	group: IGroup,
	entry: IEntryDef,
	stagingWavPath: string,
	toolCommand: string,
	nonInteractive: boolean,
	quiet: boolean,
	anchor: IGlobalMusicAnchor | undefined,
): string[] => {
	const argv: string[] = [toolCommand];
	if (group.kind === 'music') {
		argv.push('music', 'generate', '--instrumental');
	} else {
		argv.push('speech', 'synthesize');
	}
	if (nonInteractive) argv.push('--non-interactive');
	if (quiet) argv.push('--quiet');
	argv.push('--model', group.model);
	argv.push('--format', 'wav');
	argv.push('--out', stagingWavPath);
	if (group.kind === 'music') {
		argv.push('--bpm', String(anchor?.bpm ?? group.coherence.bpm ?? 96));
		if (anchor?.key) argv.push('--key', anchor.key);
		if (group.coherence.instruments) argv.push('--instruments', String(group.coherence.instruments));
		if (group.coherence.mood) argv.push('--mood', String(group.coherence.mood));
		if (group.coherence.tempo) argv.push('--tempo', String(group.coherence.tempo));
	} else {
		if (group.coherence.speed !== undefined) argv.push('--speed', String(group.coherence.speed));
		if (group.coherence.volume !== undefined) argv.push('--volume', String(group.coherence.volume));
		if (typeof group.coherence['sound-effect'] === 'string') {
			argv.push('--sound-effect', String(group.coherence['sound-effect']));
		}
	}
	argv.push('--prompt', entry.prompt);
	return argv;
};

const runCommand = (argv: string[]): { ok: boolean; stderr: string } => {
	const result = spawnSync(argv[0] ?? '', argv.slice(1), {
		stdio: ['ignore', 'inherit', 'pipe'],
		env: process.env,
	});
	return { ok: result.status === 0, stderr: result.stderr ? result.stderr.toString() : '' };
};

const variantIdFor = (timestamp: number): string => `v_${timestamp}`;

const stage = (fullKey: string, suffix: string | null): number => {
	const spec = readSpec();
	const { group, entry } = findSlot(spec, fullKey);
	if (spec.globalMusicAnchor) assertMusicAnchor(group, spec.globalMusicAnchor);
	const slotDir = join(STAGING_ROOT, group.id, entry.id);
	ensureDir(slotDir);
	const variantId = suffix ?? variantIdFor(Date.now());
	const wavName = `${variantId}.wav`;
	const wavPath = join(slotDir, wavName);
	const argv = buildCommand(
		group,
		entry,
		wavPath,
		spec.tool.command,
		spec.tool.nonInteractive,
		spec.tool.quiet,
		spec.globalMusicAnchor,
	);
	process.stdout.write(`stage ${fullKey} → ${wavPath}\n  $ ${argv.join(' ')}\n`);
	const { ok, stderr } = runCommand(argv);
	if (!ok) {
		process.stderr.write(`mmx failed: ${stderr.trim()}\n`);
		rmSync(wavPath, { force: true });
		return 1;
	}
	const stats = statSync(wavPath);
	const slot = ensureSlotStatus(spec, fullKey);
	slot.variants[variantId] = {
		variantId,
		state: 'staged',
		stagedAt: new Date().toISOString(),
		approvedAt: null,
		deprecatedAt: null,
		wavPath: relative(workspaceRoot, wavPath),
		finalPath: null,
		finalSizeBytes: null,
		wavSizeBytes: stats.size,
		lastPrompt: entry.prompt,
	};
	writeSpec(spec);
	process.stdout.write(
		`\n✓ staged ${fullKey} variant ${variantId} (${(stats.size / 1024).toFixed(1)} KB wav)\n` +
			`  listen with: bun scripts/generate-audio.script.ts listen ${fullKey} --variant ${variantId}\n`,
	);
	return 0;
};

const listen = (fullKey: string, variantId: string): number => {
	const spec = readSpec();
	const slot = spec.status[fullKey];
	if (!slot) throw STALE_ERROR(`slot ${fullKey} has no staged variants`);
	const variant = slot.variants[variantId];
	if (!variant) throw STALE_ERROR(`variant ${variantId} not found in slot ${fullKey}`);
	const wavAbs = isAbsolute(variant.wavPath ?? '')
		? (variant.wavPath as string)
		: resolve(workspaceRoot, variant.wavPath ?? '');
	if (!existsSync(wavAbs)) {
		throw STALE_ERROR(`staged wav missing on disk: ${wavAbs}`);
	}
	process.stdout.write(`${wavAbs}\n`);
	return 0;
};

const list = (fullKey: string | null, all: boolean): number => {
	const spec = readSpec();
	const slots = all ? Object.entries(spec.status) : [[fullKey, spec.status[fullKey ?? '']]] as Array<
		[string, ISlotStatus | undefined]
	>;
	const rows: string[] = ['| slot | variant | state | wav | final |'];
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
				`| ${slotKey}${approved} | ${variant.variantId} | ${variant.state} | ${variant.wavPath ?? '—'} | ${variant.finalPath ?? '—'} |`,
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
	if (!variant.wavPath) throw STALE_ERROR(`variant ${variantId} has no staged wav`);
	const wavAbs = isAbsolute(variant.wavPath)
		? variant.wavPath
		: resolve(workspaceRoot, variant.wavPath);
	if (!existsSync(wavAbs)) {
		throw STALE_ERROR(`staged wav missing on disk: ${wavAbs}`);
	}

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

	const encoding = encodeToTargetFormat(wavAbs, finalAbs, group.format);
	if (!encoding.ok) {
		process.stderr.write(`encoding failed: ${encoding.stderr}\n`);
		return 1;
	}

	variant.state = 'approved';
	variant.approvedAt = new Date().toISOString();
	variant.finalPath = relative(workspaceRoot, finalAbs);
	variant.finalSizeBytes = encoding.bytes;
	slot.approvedVariantId = variantId;
	writeSpec(spec);

	process.stdout.write(
		`\n✓ approved ${fullKey} variant ${variantId}\n` +
			`  wav:   ${variant.wavPath} (${(variant.wavSizeBytes ?? 0) / 1024} KB)\n` +
			`  final: ${variant.finalPath} (${((encoding.bytes ?? 0) / 1024).toFixed(1)} KB ${group.format})\n` +
			`  ratio: ${(((encoding.bytes ?? 0) / (variant.wavSizeBytes ?? 1)) * 100).toFixed(1)}% of the wav\n`,
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
	if (variant.wavPath) {
		const wavAbs = isAbsolute(variant.wavPath)
			? variant.wavPath
			: resolve(workspaceRoot, variant.wavPath);
		if (existsSync(wavAbs)) {
			const archivePath = join(archiveDir, `${variantId}__${stamp}__${variant.wavPath.split('/').pop()}`);
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
			'  stage     <slot> [--variant <id>]   Generate a new variant in .audio-staging/.',
			'  list      [<slot>] [--all]          List staged / approved variants.',
			'  listen    <slot> --variant <id>     Print the path to the staged wav (open it in any player).',
			'  approve   <slot> --variant <id>     Encode the staged wav to the slot format and move it to game/godot/Audio/...; the previously approved variant is archived in .deprecated/.',
			'  deprecate <slot> --variant <id>     Move a staged or approved variant to .deprecated/assets/sounds/.',
			'  status                              Print a summary of every slot.',
			'  --validate                          Validate the spec (anchor, coherence, status consistency).',
			'',
			'A slot is written as "<group>.<entry>", e.g. "sfx.ui.ui_click" or "music.menu.menu_main".',
			'Staged files live in .audio-staging/<group>/<entry>/<variantId>.wav and are NOT committed.',
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