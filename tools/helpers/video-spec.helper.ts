#!/usr/bin/env bun
/**
 * tools/helpers/video-spec.helper.ts
 *
 * Shared loader + writer + slot-finder for the video manifest.
 *
 * The sibling of `audio-spec.helper.ts`, with the same public shape, so a
 * caller that already drives audio needs no new vocabulary. Keeping the
 * JSON shape and its zod validation in one place is what stops the runner
 * and the plugin from disagreeing about what a valid spec is.
 */

import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, isAbsolute, join, relative, resolve } from 'node:path';

import {
	VideoSpecSchema,
	type IVideoEntryDef,
	type IVideoGroup,
	type IVideoSlotStatus,
	type IVideoSpec,
	type IResolvedVideoManifest,
	type IResolvedVideoSlot,
} from '../contracts/interfaces/video-spec.interface.ts';

export const SPEC_RELATIVE = join('game', 'content', 'video', 'video-spec.json');
export const DEFAULT_STAGING_RELATIVE = '.generate/videos/staging';
export const DEFAULT_DEPRECATED_RELATIVE = '.deprecated/assets/videos';

/** Godot 4 plays Ogg Theora natively and nothing else without an addon. */
export const SHIPPING_FORMAT = 'ogv';

export const SPEC_ERROR = (message: string): Error => {
	const error = new Error(message);
	error.name = 'VideoSpecError';
	return error;
};

/** Split a "group.entry" key on its LAST dot, so ids may contain dots. */
export const splitLastDot = (value: string): readonly [string, string] => {
	const index = value.lastIndexOf('.');
	if (index === -1) return [value, ''] as const;
	return [value.slice(0, index), value.slice(index + 1)] as const;
};

const ensureDir = (path: string): void => {
	mkdirSync(path, { recursive: true });
};

/** Absolute path to the manifest given the workspace root. */
export const specPathFor = (workspaceRoot: string): string => resolve(workspaceRoot, SPEC_RELATIVE);

/** Read + validate the manifest, applying the runtime defaults. */
export const readVideoSpec = (workspaceRoot: string): IVideoSpec => {
	const specPath = specPathFor(workspaceRoot);
	if (!existsSync(specPath)) {
		throw SPEC_ERROR(`video spec missing at ${specPath}`);
	}
	const raw = JSON.parse(readFileSync(specPath, 'utf8')) as unknown;
	const parsed = VideoSpecSchema.safeParse(raw);
	if (!parsed.success) {
		throw SPEC_ERROR(
			`video spec failed validation: ${parsed.error.issues
				.map((issue) => `${issue.path.join('.')}: ${issue.message}`)
				.join('; ')}`,
		);
	}
	const spec = parsed.data as IVideoSpec & { status: Record<string, IVideoSlotStatus> };
	if (!spec.status) (spec as { status: Record<string, IVideoSlotStatus> }).status = {};
	if (!spec.stagingDir) spec.stagingDir = DEFAULT_STAGING_RELATIVE;
	if (!spec.deprecatedDir) spec.deprecatedDir = DEFAULT_DEPRECATED_RELATIVE;
	return spec;
};

/** Write the manifest back to disk with a trailing newline. */
export const writeVideoSpec = (workspaceRoot: string, spec: IVideoSpec): void => {
	const specPath = specPathFor(workspaceRoot);
	ensureDir(dirname(specPath));
	writeFileSync(specPath, `${JSON.stringify(spec, null, 2)}\n`);
};

export interface IFindVideoSlotResult {
	readonly group: IVideoGroup;
	readonly entry: IVideoEntryDef;
	readonly status: IVideoSlotStatus;
}

/** Look up group + entry + status for a "group.entry" key. */
export const findSlotInSpec = (
	spec: IVideoSpec & { status: Record<string, IVideoSlotStatus> },
	fullKey: string,
): IFindVideoSlotResult => {
	const [groupId, entryId] = splitLastDot(fullKey);
	const group = spec.groups.find((candidate) => candidate.id === groupId);
	if (!group) throw SPEC_ERROR(`group ${groupId} not declared in spec`);
	const entry = group.entries.find((candidate) => candidate.id === entryId);
	if (!entry) throw SPEC_ERROR(`entry ${entryId} not declared in group ${groupId}`);
	const status = spec.status[fullKey] ?? { approvedVariantId: null, variants: {} };
	return { group, entry, status };
};

/** Lazily create and return the status entry for a slot. */
export const ensureSlotStatus = (
	spec: IVideoSpec & { status: Record<string, IVideoSlotStatus> },
	fullKey: string,
): IVideoSlotStatus => {
	if (!spec.status[fullKey]) {
		spec.status[fullKey] = { approvedVariantId: null, variants: {} };
	}
	return spec.status[fullKey];
};

/** Project the spec into a fully-resolved view consumers can iterate. */
export const resolveVideoManifest = (workspaceRoot: string): IResolvedVideoManifest => {
	const spec = readVideoSpec(workspaceRoot);
	const videoRoot = resolve(workspaceRoot, spec.root);
	const stagingDir = resolve(workspaceRoot, spec.stagingDir ?? DEFAULT_STAGING_RELATIVE);
	const deprecatedDir = resolve(workspaceRoot, spec.deprecatedDir ?? DEFAULT_DEPRECATED_RELATIVE);
	const slots: IResolvedVideoSlot[] = spec.groups.flatMap((group) =>
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
				moment: group.moment,
				group,
				entry,
				status,
				approvedVariant: approved,
				requestedVariants: all.filter((variant) => variant.state === 'requested'),
				stagedVariants: all.filter((variant) => variant.state === 'staged'),
				deprecatedVariants: all.filter((variant) => variant.state === 'deprecated'),
			};
		}),
	);
	return {
		root: videoRoot,
		stagingDir,
		deprecatedDir,
		version: spec.version,
		slots,
		anchorRatio: spec.globalLookAnchor?.ratio ?? null,
	};
};

export const findResolvedSlot = (
	manifest: IResolvedVideoManifest,
	fullKey: string,
): IResolvedVideoSlot => {
	const slot = manifest.slots.find((candidate) => candidate.fullKey === fullKey);
	if (!slot) {
		const [groupId, entryId] = splitLastDot(fullKey);
		throw SPEC_ERROR(`slot ${fullKey} not found (group=${groupId} entry=${entryId})`);
	}
	return slot;
};

/**
 * Every group must frame its clips the way the anchor says, or the opening
 * sequence and the trophy lift letterbox differently and the game looks
 * assembled from parts.
 */
export const assertLookAnchor = (manifest: IResolvedVideoManifest): void => {
	if (manifest.anchorRatio === null) return;
	for (const slot of manifest.slots) {
		if (slot.group.ratio !== manifest.anchorRatio) {
			throw SPEC_ERROR(
				`group ${slot.groupId} declares ratio=${slot.group.ratio} but the look anchor is ${manifest.anchorRatio}`,
			);
		}
	}
};

/**
 * A clip nobody can skip is a clip that will be hated. Every group has to
 * belong to a moment the settings board can switch off.
 */
export const assertEveryClipIsSkippable = (manifest: IResolvedVideoManifest): void => {
	for (const slot of manifest.slots) {
		if (!slot.moment) {
			throw SPEC_ERROR(`group ${slot.groupId} declares no moment, so its clip could never be switched off`);
		}
	}
};

/** Resolve a stored relative path against the workspace root. */
export const absoluteVideoPath = (workspaceRoot: string, storedPath: string | null): string | null => {
	if (storedPath === null) return null;
	return isAbsolute(storedPath) ? storedPath : resolve(workspaceRoot, storedPath);
};

/** Convert an absolute path back into a workspace-relative string. */
export const relativeVideoPath = (workspaceRoot: string, absolutePath: string): string =>
	relative(workspaceRoot, absolutePath).split('\\').join('/');
