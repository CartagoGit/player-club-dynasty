/**
 * Read the audio manifest and surface every slot plus its variants.
 *
 * Pure read-only helper. The mcp-vertex plugin uses it to drive the
 * `audio_list_variants`, `audio_status` and other tools. Mutations
 * (stage / approve / deprecate) live in
 * `scripts/generate-audio.script.ts` — these tools only delegate to it.
 */

import { existsSync, readFileSync } from 'node:fs';
import { dirname, isAbsolute, join, relative, resolve } from 'node:path';

import {
	AudioSpecSchema,
	type AudioEntryDef,
	type AudioGroup,
	type AudioSlotStatus,
	type AudioSpec,
	type AudioVariantRecord,
} from './audio-spec.types';

export interface IResolvedSlot {
	fullKey: string;
	groupId: string;
	entryId: string;
	groupKind: 'music' | 'sfx';
	group: AudioGroup;
	entry: AudioEntryDef;
	status: AudioSlotStatus;
	approvedVariant: AudioVariantRecord | null;
	stagedVariants: AudioVariantRecord[];
	deprecatedVariants: AudioVariantRecord[];
}

export interface IResolvedAudioManifest {
	root: string;
	stagingDir: string;
	deprecatedDir: string;
	version: number;
	slots: IResolvedSlot[];
	anchorBpm: number | null;
	anchorKey: string | null;
}

const STALE_ERROR = (message: string): Error => {
	const error = new Error(message);
	error.name = 'AudioManifestError';
	return error;
};

const splitLastDot = (value: string): [string, string] => {
	const index = value.lastIndexOf('.');
	if (index === -1) return [value, ''];
	return [value.slice(0, index), value.slice(index + 1)];
};

export const loadAudioManifest = (workspaceRoot: string): IResolvedAudioManifest => {
	const specPath = join(workspaceRoot, 'game', 'content', 'audio', 'audio-spec.json');
	if (!existsSync(specPath)) {
		throw STALE_ERROR(`audio spec missing at ${specPath}`);
	}
	const raw = JSON.parse(readFileSync(specPath, 'utf8')) as unknown;
	const parsed = AudioSpecSchema.safeParse(raw);
	if (!parsed.success) {
		throw STALE_ERROR(
			`audio spec failed validation: ${parsed.error.issues
				.map((issue) => `${issue.path.join('.')}: ${issue.message}`)
				.join('; ')}`,
		);
	}
	const spec: AudioSpec = parsed.data;
	const audioRoot = resolve(workspaceRoot, spec.root);
	const slots: IResolvedSlot[] = [];
	for (const group of spec.groups) {
		for (const entry of group.entries) {
			const fullKey = `${group.id}.${entry.id}`;
			const status = spec.status[fullKey] ?? { approvedVariantId: null, variants: {} };
			const allVariants = Object.values(status.variants);
			const approvedVariant = status.approvedVariantId
				? (status.variants[status.approvedVariantId] ?? null)
				: null;
			slots.push({
				fullKey,
				groupId: group.id,
				entryId: entry.id,
				groupKind: group.kind,
				group,
				entry,
				status,
				approvedVariant,
				stagedVariants: allVariants.filter((variant) => variant.state === 'staged'),
				deprecatedVariants: allVariants.filter((variant) => variant.state === 'deprecated'),
			});
		}
	}
	return {
		root: audioRoot,
		stagingDir: resolve(workspaceRoot, spec.stagingDir ?? '.audio-staging'),
		deprecatedDir: resolve(workspaceRoot, spec.deprecatedDir ?? '.deprecated/assets/sounds'),
		version: spec.version,
		slots,
		anchorBpm: spec.globalMusicAnchor?.bpm ?? null,
		anchorKey: spec.globalMusicAnchor?.key ?? null,
	};
};

export const findSlot = (manifest: IResolvedAudioManifest, fullKey: string): IResolvedSlot => {
	const slot = manifest.slots.find((candidate) => candidate.fullKey === fullKey);
	if (!slot) {
		const [groupId, entryId] = splitLastDot(fullKey);
		throw STALE_ERROR(`slot ${fullKey} not found (group=${groupId} entry=${entryId})`);
	}
	return slot;
};

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

void dirname;
void isAbsolute;
void relative;