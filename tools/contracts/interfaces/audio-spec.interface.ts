/**
 * tools/contracts/interfaces/audio-spec.interface.ts
 *
 * The single source of truth for the audio manifest shape.
 *
 * The same TypeScript types are consumed by:
 *   - tools/scripts/content/generate-audio.script.ts (the runner that
 *     talks to mmx + ffmpeg),
 *   - plugins/mcp-vertex_player-club-dynasty/src/lib/audio-manifest.helper.ts
 *     (the mcp-vertex plugin that exposes stage/listen/approve/deprecate
 *     to agents),
 *   - scripts/generate-audio.script.spec.ts (the runner's test suite).
 *
 * Defining them here means a schema change ripples through the whole
 * pipeline automatically — no copy-pasted interfaces drifting apart.
 *
 * The zod validators live next to these types to keep runtime + compile
 * time in sync; consumers can pick whichever side they need.
 */

import z from 'zod';

// ─── Variant lifecycle ────────────────────────────────────────────────────

export type AudioVariantState = 'staged' | 'approved' | 'deprecated';

export const AudioVariantStateSchema = z.enum(['staged', 'approved', 'deprecated']);

export interface IAudioVariantRecord {
	readonly variantId: string;
	readonly state: AudioVariantState;
	readonly stagedAt: string | null;
	readonly approvedAt: string | null;
	readonly deprecatedAt: string | null;
	readonly wavPath: string | null;
	readonly finalPath: string | null;
	readonly finalSizeBytes: number | null;
	readonly wavSizeBytes: number | null;
	readonly lastPrompt: string;
}

export const AudioVariantRecordSchema = z.object({
	variantId: z.string().min(1),
	state: AudioVariantStateSchema,
	stagedAt: z.string().nullable(),
	approvedAt: z.string().nullable(),
	deprecatedAt: z.string().nullable(),
	wavPath: z.string().nullable(),
	finalPath: z.string().nullable(),
	finalSizeBytes: z.number().int().nullable(),
	wavSizeBytes: z.number().int().nullable(),
	lastPrompt: z.string(),
});

// ─── Slot = groupId.entryId ────────────────────────────────────────────────

export interface IAudioSlotStatus {
	readonly approvedVariantId: string | null;
	readonly variants: Readonly<Record<string, IAudioVariantRecord>>;
}

export const AudioSlotStatusSchema = z.object({
	approvedVariantId: z.string().nullable(),
	variants: z.record(z.string(), AudioVariantRecordSchema),
});

// ─── Coherence / spec content ─────────────────────────────────────────────

export type AudioKind = 'music' | 'sfx';

export interface IAudioEntryDef {
	readonly id: string;
	readonly filename: string;
	readonly prompt: string;
}

export const AudioEntryDefSchema = z.object({
	id: z.string().min(1),
	filename: z.string().min(1),
	prompt: z.string().min(1),
});

export interface IAudioGroup {
	readonly id: string;
	readonly kind: AudioKind;
	readonly outputDir: string;
	readonly format: string;
	readonly model: string;
	readonly styleAnchor: string;
	readonly coherence: Readonly<Record<string, unknown>>;
	readonly entries: readonly IAudioEntryDef[];
}

export const AudioGroupSchema = z.object({
	id: z.string().min(1),
	kind: z.enum(['music', 'sfx']),
	outputDir: z.string().min(1),
	format: z.string().min(1),
	model: z.string().min(1),
	styleAnchor: z.string().min(1),
	coherence: z.record(z.string(), z.unknown()),
	entries: z.array(AudioEntryDefSchema).min(1),
});

export interface IAudioGlobalMusicAnchor {
	readonly bpm: number;
	readonly key: string;
	readonly scaleNotes: readonly string[];
	readonly instruments: string;
	readonly palette?: string;
	readonly loopSeconds: number;
	readonly transitionRules: readonly string[];
}

export const AudioGlobalMusicAnchorSchema = z.object({
	bpm: z.number().int().positive(),
	key: z.string().min(1),
	scaleNotes: z.array(z.string()).min(1),
	instruments: z.string().min(1),
	palette: z.string().optional(),
	loopSeconds: z.number().int().positive(),
	transitionRules: z.array(z.string().min(1)).min(1),
});

// ─── Top-level spec ───────────────────────────────────────────────────────

export interface IAudioToolConfig {
	readonly command: string;
	readonly nonInteractive: boolean;
	readonly quiet: boolean;
}

export interface IAudioSpec {
	readonly version: number;
	readonly root: string;
	readonly stagingDir?: string;
	readonly deprecatedDir?: string;
	readonly tool: IAudioToolConfig;
	readonly globalMusicAnchor?: IAudioGlobalMusicAnchor;
	readonly groups: readonly IAudioGroup[];
	readonly status: Readonly<Record<string, IAudioSlotStatus>>;
}

export const AudioSpecSchema = z.object({
	version: z.number().int().positive(),
	root: z.string().min(1),
	stagingDir: z.string().optional(),
	deprecatedDir: z.string().optional(),
	tool: z.object({
		command: z.string().min(1),
		nonInteractive: z.boolean(),
		quiet: z.boolean(),
	}),
	globalMusicAnchor: AudioGlobalMusicAnchorSchema.optional(),
	groups: z.array(AudioGroupSchema).min(1),
	status: z.record(z.string(), AudioSlotStatusSchema),
});

// ─── Resolved slot (helper output) ────────────────────────────────────────

export interface IResolvedAudioSlot {
	readonly fullKey: string;
	readonly groupId: string;
	readonly entryId: string;
	readonly groupKind: AudioKind;
	readonly group: IAudioGroup;
	readonly entry: IAudioEntryDef;
	readonly status: IAudioSlotStatus;
	readonly approvedVariant: IAudioVariantRecord | null;
	readonly stagedVariants: readonly IAudioVariantRecord[];
	readonly deprecatedVariants: readonly IAudioVariantRecord[];
}

export interface IResolvedAudioManifest {
	readonly root: string;
	readonly stagingDir: string;
	readonly deprecatedDir: string;
	readonly version: number;
	readonly slots: readonly IResolvedAudioSlot[];
	readonly anchorBpm: number | null;
	readonly anchorKey: string | null;
}

// ─── Encoding result (used by approve) ────────────────────────────────────

export interface IAudioEncodeResult {
	readonly ok: boolean;
	readonly stderr: string;
	readonly bytes: number | null;
	readonly codec: string | null;
}