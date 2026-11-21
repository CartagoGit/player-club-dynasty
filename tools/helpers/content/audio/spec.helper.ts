#!/usr/bin/env bun
/**
 * tools/helpers/content/audio/spec.helper.ts
 *
 * Shared loader + writer + slot-finder for the audio manifest.
 *
 * Both the runner (tools/scripts/content/generate-audio.script.ts) and
 * the delendai plugin (plugins/.../lib/audio-manifest.helper.ts) use
 * these helpers so the JSON shape and zod validation are checked in one
 * place — no risk of one side accepting a spec the other rejects.
 */

import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, isAbsolute, join, relative, resolve } from 'node:path';

import {
	AudioSpecSchema,
	type IAudioGroup,
	type IAudioEntryDef,
	type IAudioSlotStatus,
	type IMutableAudioSlotStatus,
	type MutableAudioSpec,
	type IAudioSpec,
	type IResolvedAudioManifest,
	type IResolvedAudioSlot,
} from '../../../contracts/interfaces/content/audio-spec.interface.ts';

export const SPEC_RELATIVE = join('game', 'content', 'audio', 'audio-spec.json');
export const DEFAULT_STAGING_RELATIVE = '.generate/audios/staging';
export const DEFAULT_DEPRECATED_RELATIVE = '.deprecated/assets/sounds';

export const STALE_ERROR = (message: string): Error => {
	const error = new Error(message);
	error.name = 'AudioSpecError';
	return error;
};

/**
 * Split a "group.entry" key on its LAST dot. Slot ids can themselves
 * contain dots (none yet, but spec-wise it's allowed), so lastIndexOf is
 * the safe primitive.
 */
export const splitLastDot = (value: string): readonly [string, string] => {
	const index = value.lastIndexOf('.');
	if (index === -1) return [value, ''] as const;
	return [value.slice(0, index), value.slice(index + 1)] as const;
};

const ensureDir = (path: string): void => {
	mkdirSync(path, { recursive: true });
};

/** Absolute path to the manifest given the workspace root. */
export const specPathFor = (workspaceRoot: string): string =>
	resolve(workspaceRoot, SPEC_RELATIVE);

/**
 * Legacy audio-spec snapshots may omit nullable bookkeeping fields on old
 * variants. Normalize them before zod validation so maintenance commands such
 * as `rebuild-all --clean` can recover a partially-migrated manifest instead of
 * failing before they get a chance to clear `status`.
 */
const normalizeLegacyAudioSpec = (raw: unknown): unknown => {
	if (typeof raw !== 'object' || raw === null) return raw;
	const spec = raw as Record<string, unknown>;
	const status = spec.status;
	if (typeof status !== 'object' || status === null) return raw;
	for (const slot of Object.values(status as Record<string, unknown>)) {
		if (typeof slot !== 'object' || slot === null) continue;
		const mutableSlot = slot as Record<string, unknown>;
		const variants = mutableSlot.variants;
		if (typeof variants !== 'object' || variants === null) continue;
		for (const variant of Object.values(variants as Record<string, unknown>)) {
			if (typeof variant !== 'object' || variant === null) continue;
			const v = variant as Record<string, unknown>;
			if (v.stagedAt === undefined) v.stagedAt = null;
			if (v.approvedAt === undefined) v.approvedAt = null;
			if (v.deprecatedAt === undefined) v.deprecatedAt = null;
			if (v.finalPath === undefined) v.finalPath = null;
			if (v.finalSizeBytes === undefined) v.finalSizeBytes = null;
			if (v.audioPath === undefined && v.wavPath === undefined) v.audioPath = null;
			if (v.audioSizeBytes === undefined && v.wavSizeBytes === undefined) v.audioSizeBytes = null;
			if (v.lastPrompt === undefined) v.lastPrompt = '';
		}
	}
	return raw;
};

/**
 * Read the manifest from disk, validate it with zod, and apply the
 * runtime defaults that the runner relies on (empty status, default
 * staging/deprecated dirs). Throws AudioSpecError on missing/invalid.
 */
export const readAudioSpec = (workspaceRoot: string): MutableAudioSpec => {
	const specPath = specPathFor(workspaceRoot);
	if (!existsSync(specPath)) {
		throw STALE_ERROR(`audio spec missing at ${specPath}`);
	}
	const raw = normalizeLegacyAudioSpec(JSON.parse(readFileSync(specPath, 'utf8')) as unknown);
	const parsed = AudioSpecSchema.safeParse(raw);
	if (!parsed.success) {
		throw STALE_ERROR(
			`audio spec failed validation: ${parsed.error.issues
				.map((issue) => `${issue.path.join('.')}: ${issue.message}`)
				.join('; ')}`,
		);
	}
	const spec = parsed.data as MutableAudioSpec;
	if (!spec.status) spec.status = {};
	if (!spec.stagingDir) spec.stagingDir = DEFAULT_STAGING_RELATIVE;
	if (!spec.deprecatedDir) spec.deprecatedDir = DEFAULT_DEPRECATED_RELATIVE;
	return spec;
};

/** Write the manifest back to disk atomically with a trailing newline. */
export const writeAudioSpec = (workspaceRoot: string, spec: IAudioSpec): void => {
	const specPath = specPathFor(workspaceRoot);
	ensureDir(dirname(specPath));
	writeFileSync(specPath, `${JSON.stringify(spec, null, 2)}\n`);
};

export interface IFindSlotResult {
	readonly group: IAudioGroup;
	readonly entry: IAudioEntryDef;
	readonly status: IAudioSlotStatus;
}

/** Look up group + entry + status for a "group.entry" key. */
export const findSlotInSpec = (
	spec: IAudioSpec & { status: Record<string, IAudioSlotStatus> },
	fullKey: string,
): IFindSlotResult => {
	const [groupId, entryId] = splitLastDot(fullKey);
	const group = spec.groups.find((candidate) => candidate.id === groupId);
	if (!group) throw STALE_ERROR(`group ${groupId} not declared in spec`);
	const entry = group.entries.find((candidate) => candidate.id === entryId);
	if (!entry) throw STALE_ERROR(`entry ${entryId} not declared in group ${groupId}`);
	const status = spec.status[fullKey] ?? { approvedVariantId: null, variants: {} };
	return { group, entry, status };
};

/** Lazily create and return the status entry for a slot. */
export const ensureSlotStatus = (
	spec: MutableAudioSpec,
	fullKey: string,
): IMutableAudioSlotStatus => {
	if (!spec.status[fullKey]) {
		spec.status[fullKey] = { approvedVariantId: null, variants: {} };
	}
	return spec.status[fullKey]!;
};

/**
 * Project the spec into a fully-resolved view that consumers can iterate
 * without re-deriving relationships. Used by the delendai plugin tools.
 */
export const resolveAudioManifest = (workspaceRoot: string): IResolvedAudioManifest => {
	const spec = readAudioSpec(workspaceRoot);
	const audioRoot = resolve(workspaceRoot, spec.root);
	const stagingDir = resolve(workspaceRoot, spec.stagingDir ?? DEFAULT_STAGING_RELATIVE);
	const deprecatedDir = resolve(workspaceRoot, spec.deprecatedDir ?? DEFAULT_DEPRECATED_RELATIVE);
	const slots: IResolvedAudioSlot[] = spec.groups.flatMap((group) =>
		group.entries.map((entry) => {
			const fullKey = `${group.id}.${entry.id}`;
			const status = spec.status[fullKey] ?? { approvedVariantId: null, variants: {} };
			const all = Object.values(status.variants);
			const approved = status.approvedVariantId
				? (status.variants[status.approvedVariantId] ?? null)
				: null;
			return {
				fullKey,
				groupId: group.id,
				entryId: entry.id,
				groupKind: group.kind,
				group,
				entry,
				status,
				approvedVariant: approved,
				stagedVariants: all.filter((variant) => variant.state === 'staged'),
				deprecatedVariants: all.filter((variant) => variant.state === 'deprecated'),
			};
		}),
	);
	return {
		root: audioRoot,
		stagingDir,
		deprecatedDir,
		version: spec.version,
		slots,
		anchorBpm: spec.globalMusicAnchor?.bpm ?? null,
		anchorKey: spec.globalMusicAnchor?.key ?? null,
	};
};

export const findResolvedSlot = (
	manifest: IResolvedAudioManifest,
	fullKey: string,
): IResolvedAudioSlot => {
	const slot = manifest.slots.find((candidate) => candidate.fullKey === fullKey);
	if (!slot) {
		const [groupId, entryId] = splitLastDot(fullKey);
		throw STALE_ERROR(`slot ${fullKey} not found (group=${groupId} entry=${entryId})`);
	}
	return slot;
};

/** Validate that every music group's coherence matches the global anchor. */
export const assertMusicAnchor = (manifest: IResolvedAudioManifest): void => {
	if (manifest.anchorBpm === null) return;
	for (const slot of manifest.slots) {
		if (slot.groupKind !== 'music') continue;
		const declaredBpm = Number(slot.group.coherence.bpm);
		if (Number.isFinite(declaredBpm) && declaredBpm !== manifest.anchorBpm) {
			throw STALE_ERROR(
				`group ${slot.groupId} declares bpm=${declaredBpm} but global anchor bpm=${manifest.anchorBpm}`,
			);
		}
	}
};

/** Resolve a stored relative path against the workspace root. */
export const absoluteAudioPath = (workspaceRoot: string, storedPath: string | null): string | null => {
	if (storedPath === null) return null;
	return isAbsolute(storedPath) ? storedPath : resolve(workspaceRoot, storedPath);
};

/** Convert an absolute path back into a workspace-relative string. */
export const relativeAudioPath = (workspaceRoot: string, absolutePath: string): string =>
	relative(workspaceRoot, absolutePath);