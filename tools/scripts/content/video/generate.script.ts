#!/usr/bin/env bun
/**
 * tools/scripts/content/video/generate.script.ts
 *
 * The PC Dynasty video review pipeline — the sibling of
 * `generate-audio.script.ts`, deliberately the same verbs in the same
 * order so anyone who has staged a sound already knows how to stage a
 * clip.
 *
 *   stage      — ask the generator for a fresh variant into
 *                `.generate/videos/staging/<group>/<entry>/v_<ts>.mp4`
 *   fetch      — download a variant that was requested with --async
 *   list       — every variant of a slot, so a human can watch them all
 *                before deciding
 *   approve    — transcode one variant to Ogg Theora, move it into
 *                `game/godot/Video/...` and archive the previous one
 *   deprecate  — archive a variant without approving anything else
 *   status     — the whole manifest at a glance
 *   --validate — check the spec without touching anything
 *
 *   bun tools/scripts/content/video/generate.script.ts stage video.brand.logo_sting
 *   bun tools/scripts/content/video/generate.script.ts stage video.brand.logo_sting --async
 *   bun tools/scripts/content/video/generate.script.ts fetch video.brand.logo_sting --variant v_1692345678
 *   bun tools/scripts/content/video/generate.script.ts list --all
 *   bun tools/scripts/content/video/generate.script.ts approve video.brand.logo_sting --variant v_1692345678
 *   bun tools/scripts/content/video/generate.script.ts --validate
 *
 * Unlike the audio runner, this one imports the shared contract and
 * helper rather than re-declaring the manifest shape locally: a second
 * copy of the types is exactly how a pipeline starts accepting specs the
 * other half rejects.
 *
 * `.generate/videos/staging/` and `.deprecated/assets/videos/` are
 * gitignored. The repository only ever carries approved, transcoded clips.
 */

import { copyFileSync, existsSync, mkdirSync, renameSync, statSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

import { workspaceRoot } from '../../workspace/root.script.ts';
import { runSync } from '../../../helpers/system/process.helper.ts';
import { encodeVideoTo } from '../../../helpers/content/video/encoder.helper.ts';
import {
	SHIPPING_FORMAT,
	assertEveryClipIsSkippable,
	assertLookAnchor,
	ensureSlotStatus,
	findSlotInSpec,
	readVideoSpec,
	relativeVideoPath,
	resolveVideoManifest,
	writeVideoSpec,
} from '../../../helpers/content/video/spec.helper.ts';
import type {
	IVideoSlotStatus,
	IVideoSpec,
	IVideoVariantRecord,
} from '../../../contracts/interfaces/content/video-spec.interface.ts';


const nowIso = (): string => new Date().toISOString();

const ensureDir = (path: string): void => {
	mkdirSync(path, { recursive: true });
};

const fail = (message: string): never => {
	process.stderr.write(`${message}\n`);
	process.exit(1);
};

const flagValue = (argv: readonly string[], flag: string): string | null => {
	const index = argv.indexOf(flag);
	if (index === -1 || index + 1 >= argv.length) return null;
	return argv[index + 1] ?? null;
};

const hasFlag = (argv: readonly string[], flag: string): boolean => argv.includes(flag);

/** Staging path for a variant. The generator always hands us an mp4. */
const stagingPathFor = (
	spec: IVideoSpec,
	groupId: string,
	entryId: string,
	variantId: string,
): string =>
	resolve(
		workspaceRoot,
		spec.stagingDir ?? '.generate/videos/staging',
		groupId,
		entryId,
		`${variantId}.mp4`,
	);

/**
 * Compose the prompt actually sent to the generator: the entry's own text,
 * then the shared look anchor. The anchor goes last so it reads as the
 * house style constraining the shot rather than the subject of it.
 */
const promptFor = (spec: IVideoSpec, entryPrompt: string, styleAnchor: string): string => {
	const anchor = spec.globalLookAnchor;
	if (!anchor) return `${entryPrompt} ${styleAnchor}`;
	return [
		entryPrompt,
		styleAnchor,
		`Palette: ${anchor.palette}.`,
		`Camera: ${anchor.camera}.`,
		`Grade: ${anchor.grade}.`,
		`Do not include: ${anchor.avoid}.`,
	].join(' ');
};

const toolArgs = (spec: IVideoSpec): string[] => {
	const args: string[] = [];
	if (spec.tool.nonInteractive) args.push('--non-interactive');
	if (spec.tool.quiet) args.push('--quiet');
	return args;
};

// ─── stage ────────────────────────────────────────────────────────────────

const stage = (argv: readonly string[]): void => {
	const slotKey = argv[1];
	if (!slotKey) fail('usage: stage <group.entry> [--variant <name>] [--async]');

	const spec = readVideoSpec(workspaceRoot);
	const { group, entry } = findSlotInSpec(spec, slotKey!);
	const status = ensureSlotStatus(spec, slotKey!);

	const variantId = flagValue(argv, '--variant') ?? `v_${Math.floor(Date.now() / 1000)}`;
	if (status.variants[variantId]) {
		fail(`variant ${variantId} already exists for ${slotKey}; pick another --variant name`);
	}

	const target = stagingPathFor(spec, group.id, entry.id, variantId);
	ensureDir(dirname(target));

	const prompt = promptFor(spec, entry.prompt, group.styleAnchor);
	const asynchronous = hasFlag(argv, '--async');
	const args = [
		'video',
		'generate',
		...toolArgs(spec),
		'--model',
		group.model,
		'--prompt',
		prompt,
		'--duration',
		String(entry.durationSeconds),
		'--ratio',
		group.ratio,
	];
	if (asynchronous) {
		args.push('--async');
	} else {
		args.push('--download', target);
	}

	process.stdout.write(`staging ${slotKey} as ${variantId} (${entry.durationSeconds}s, ${group.ratio})\n`);
	const result = runSync(spec.tool.command, args);
	if (!result.succeeded) {
		fail(`generator failed for ${slotKey}: ${result.stderr || result.stdout}`);
	}

	const record: IVideoVariantRecord = asynchronous
		? {
				variantId,
				state: 'requested',
				taskId: (result.stdout ?? '').trim() || null,
				requestedAt: nowIso(),
				stagedAt: null,
				approvedAt: null,
				deprecatedAt: null,
				sourcePath: null,
				sourceSizeBytes: null,
				finalPath: null,
				finalSizeBytes: null,
				lastPrompt: prompt,
			}
		: {
				variantId,
				state: 'staged',
				taskId: null,
				requestedAt: nowIso(),
				stagedAt: nowIso(),
				approvedAt: null,
				deprecatedAt: null,
				sourcePath: relativeVideoPath(workspaceRoot, target),
				sourceSizeBytes: existsSync(target) ? statSync(target).size : null,
				finalPath: null,
				finalSizeBytes: null,
				lastPrompt: prompt,
			};

	status.variants[variantId] = record;
	writeVideoSpec(workspaceRoot, spec);
	process.stdout.write(
		asynchronous
			? `requested ${slotKey} ${variantId}; run: fetch ${slotKey} --variant ${variantId}\n`
			: `staged ${slotKey} ${variantId} → ${relativeVideoPath(workspaceRoot, target)}\n`,
	);
};

// ─── fetch ────────────────────────────────────────────────────────────────

const fetch = (argv: readonly string[]): void => {
	const slotKey = argv[1];
	const variantId = flagValue(argv, '--variant');
	if (!slotKey || !variantId) fail('usage: fetch <group.entry> --variant <id>');

	const spec = readVideoSpec(workspaceRoot);
	const { group, entry } = findSlotInSpec(spec, slotKey!);
	const status = ensureSlotStatus(spec, slotKey!);
	const record = status.variants[variantId!];
	if (!record) fail(`variant ${variantId} not found for ${slotKey}`);
	if (record!.state !== 'requested') {
		fail(`variant ${variantId} is ${record!.state}, not requested; nothing to fetch`);
	}
	if (!record!.taskId) fail(`variant ${variantId} has no task id recorded`);

	const target = stagingPathFor(spec, group.id, entry.id, variantId!);
	ensureDir(dirname(target));
	const result = runSync(spec.tool.command, [
		'video',
		'download',
		...toolArgs(spec),
		record!.taskId!,
		'--output',
		target,
	]);
	if (!result.succeeded) {
		fail(`download failed for ${slotKey} ${variantId}: ${result.stderr || result.stdout}`);
	}

	status.variants[variantId!] = {
		...record!,
		state: 'staged',
		taskId: null,
		stagedAt: nowIso(),
		sourcePath: relativeVideoPath(workspaceRoot, target),
		sourceSizeBytes: existsSync(target) ? statSync(target).size : null,
	};
	writeVideoSpec(workspaceRoot, spec);
	process.stdout.write(`staged ${slotKey} ${variantId} → ${relativeVideoPath(workspaceRoot, target)}\n`);
};

// ─── list ─────────────────────────────────────────────────────────────────

const list = (argv: readonly string[]): void => {
	const manifest = resolveVideoManifest(workspaceRoot);
	const all = hasFlag(argv, '--all');
	const slotKey = argv[1];
	const slots = all
		? manifest.slots
		: manifest.slots.filter((slot) => slot.fullKey === slotKey);
	if (!all && slots.length === 0) fail(`slot ${slotKey} not found`);

	process.stdout.write('| slot | moment | variant | state | source | final |\n');
	process.stdout.write('| --- | --- | --- | --- | --- | --- |\n');
	for (const slot of slots) {
		const variants = Object.values(slot.status.variants);
		if (variants.length === 0) {
			process.stdout.write(`| ${slot.fullKey} | ${slot.moment} | — | none | — | — |\n`);
			continue;
		}
		for (const variant of variants) {
			process.stdout.write(
				`| ${slot.fullKey} | ${slot.moment} | ${variant.variantId} | ${variant.state} | ${variant.sourcePath ?? '—'} | ${variant.finalPath ?? '—'} |\n`,
			);
		}
	}
};

// ─── approve ──────────────────────────────────────────────────────────────

const approve = (argv: readonly string[]): void => {
	const slotKey = argv[1];
	const variantId = flagValue(argv, '--variant');
	if (!slotKey || !variantId) fail('usage: approve <group.entry> --variant <id>');

	const spec = readVideoSpec(workspaceRoot);
	const { group, entry } = findSlotInSpec(spec, slotKey!);
	const status = ensureSlotStatus(spec, slotKey!);
	const record = status.variants[variantId!];
	if (!record) fail(`variant ${variantId} not found for ${slotKey}`);
	if (record!.state !== 'staged') fail(`variant ${variantId} is ${record!.state}; only staged variants can be approved`);
	if (!record!.sourcePath) fail(`variant ${variantId} has no staged file on disk`);

	const source = resolve(workspaceRoot, record!.sourcePath!);
	const finalRelative = join(spec.root, group.outputDir, `${entry.filename}.${SHIPPING_FORMAT}`);
	const finalAbsolute = resolve(workspaceRoot, finalRelative);
	ensureDir(dirname(finalAbsolute));

	// Archive whatever is currently approved before overwriting it.
	const previousId = status.approvedVariantId;
	if (previousId && previousId !== variantId) {
		const previous = status.variants[previousId];
		if (previous?.finalPath) {
			const archived = resolve(
				workspaceRoot,
				spec.deprecatedDir ?? '.deprecated/assets/videos',
				group.id,
				entry.id,
				`${previousId}.${SHIPPING_FORMAT}`,
			);
			ensureDir(dirname(archived));
			const previousAbsolute = resolve(workspaceRoot, previous.finalPath);
			if (existsSync(previousAbsolute)) renameSync(previousAbsolute, archived);
			status.variants[previousId] = { ...previous, state: 'deprecated', deprecatedAt: nowIso() };
		}
	}

	const encoded = encodeVideoTo(source, finalAbsolute, group.format);
	if (!encoded.ok) fail(`encode failed for ${slotKey}: ${encoded.stderr}`);

	status.variants[variantId!] = {
		...record!,
		state: 'approved',
		approvedAt: nowIso(),
		finalPath: finalRelative.split('\\').join('/'),
		finalSizeBytes: encoded.bytes,
	};
	status.approvedVariantId = variantId!;
	writeVideoSpec(workspaceRoot, spec);
	process.stdout.write(
		`approved ${slotKey} ${variantId} → ${finalRelative} (${encoded.codec}, ${encoded.bytes ?? 0} bytes)\n`,
	);
};

// ─── deprecate ────────────────────────────────────────────────────────────

const deprecate = (argv: readonly string[]): void => {
	const slotKey = argv[1];
	const variantId = flagValue(argv, '--variant');
	if (!slotKey || !variantId) fail('usage: deprecate <group.entry> --variant <id>');

	const spec = readVideoSpec(workspaceRoot);
	const { group, entry } = findSlotInSpec(spec, slotKey!);
	const status = spec.status[slotKey!];
	if (!status) fail(`slot ${slotKey} has no status yet; nothing to deprecate`);
	const record = status!.variants[variantId!];
	if (!record) fail(`variant ${variantId} not found for ${slotKey}`);

	const archived = resolve(
		workspaceRoot,
		spec.deprecatedDir ?? '.deprecated/assets/videos',
		group.id,
		entry.id,
		`${variantId}.mp4`,
	);
	ensureDir(dirname(archived));
	const source = record!.sourcePath ? resolve(workspaceRoot, record!.sourcePath) : null;
	if (source && existsSync(source)) copyFileSync(source, archived);

	status!.variants[variantId!] = { ...record!, state: 'deprecated', deprecatedAt: nowIso() };
	if (status!.approvedVariantId === variantId) status!.approvedVariantId = null;
	writeVideoSpec(workspaceRoot, spec);
	process.stdout.write(`deprecated ${slotKey} ${variantId}\n`);
};

// ─── status ───────────────────────────────────────────────────────────────

const printStatus = (): void => {
	const manifest = resolveVideoManifest(workspaceRoot);
	process.stdout.write('| slot | moment | approved | staged | requested | deprecated |\n');
	process.stdout.write('| --- | --- | --- | --- | --- | --- |\n');
	for (const slot of manifest.slots) {
		process.stdout.write(
			`| ${slot.fullKey} | ${slot.moment} | ${slot.approvedVariant?.variantId ?? '—'} | ${slot.stagedVariants.length} | ${slot.requestedVariants.length} | ${slot.deprecatedVariants.length} |\n`,
		);
	}
	const approved = manifest.slots.filter((slot) => slot.approvedVariant !== null).length;
	process.stdout.write(`\n${approved}/${manifest.slots.length} slots have an approved clip.\n`);
};

// ─── validate ─────────────────────────────────────────────────────────────

const validate = (): void => {
	const manifest = resolveVideoManifest(workspaceRoot);
	assertLookAnchor(manifest);
	assertEveryClipIsSkippable(manifest);
	process.stdout.write(
		`video spec ok: ${manifest.slots.length} slots across ${new Set(manifest.slots.map((slot) => slot.groupId)).size} groups.\n`,
	);
};

// ─── entry point ──────────────────────────────────────────────────────────

const main = (): void => {
	const argv = process.argv.slice(2);
	if (hasFlag(argv, '--validate')) return validate();

	switch (argv[0]) {
		case 'stage':
			return stage(argv);
		case 'fetch':
			return fetch(argv);
		case 'list':
			return list(argv);
		case 'approve':
			return approve(argv);
		case 'deprecate':
			return deprecate(argv);
		case 'status':
			return printStatus();
		default:
			fail('usage: generate-video.script.ts <stage|fetch|list|approve|deprecate|status> | --validate');
	}
};

try {
	main();
} catch (error) {
	fail(error instanceof Error ? `${error.name}: ${error.message}` : String(error));
}
