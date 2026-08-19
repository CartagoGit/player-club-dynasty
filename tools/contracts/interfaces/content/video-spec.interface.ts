/**
 * tools/contracts/interfaces/content/video-spec.interface.ts
 *
 * The single source of truth for the video manifest shape.
 *
 * Deliberately the sibling of `audio-spec.interface.ts`: same variant
 * lifecycle, same slot addressing (`group.entry`), same status map. An
 * agent or a human who has driven the audio pipeline already knows this
 * one.
 *
 * Three things differ, and they are differences of the medium, not of
 * taste:
 *
 *   1. **Generation is asynchronous.** `mmx video generate` returns a task
 *      id and the file arrives later, so a staged variant records its
 *      `taskId` and can be in flight with no file on disk yet.
 *   2. **The shipping format is Ogg Theora.** Godot 4 plays `.ogv`
 *      natively through `VideoStreamPlayer` and nothing else without a
 *      third-party addon, so approve always transcodes to `ogv` no matter
 *      what the generator returned.
 *   3. **A slot declares the moment it belongs to.** The settings board
 *      gates playback per moment, so the manifest has to say which switch
 *      governs each clip.
 */

import z from 'zod';

// ─── Variant lifecycle ────────────────────────────────────────────────────

export type VideoVariantState = 'requested' | 'staged' | 'approved' | 'deprecated';

export const VideoVariantStateSchema = z.enum(['requested', 'staged', 'approved', 'deprecated']);

export interface IVideoVariantRecord {
	readonly variantId: string;
	readonly state: VideoVariantState;
	/** mmx task id while the clip is still being generated; null once downloaded. */
	readonly taskId: string | null;
	readonly requestedAt: string | null;
	readonly stagedAt: string | null;
	readonly approvedAt: string | null;
	readonly deprecatedAt: string | null;
	/** The generator's own output (mp4), inside the staging tree. */
	readonly sourcePath: string | null;
	readonly sourceSizeBytes: number | null;
	/** The shipped .ogv, inside `root`. */
	readonly finalPath: string | null;
	readonly finalSizeBytes: number | null;
	readonly lastPrompt: string;
}

export const VideoVariantRecordSchema = z.object({
	variantId: z.string().min(1),
	state: VideoVariantStateSchema,
	taskId: z.string().nullable(),
	requestedAt: z.string().nullable(),
	stagedAt: z.string().nullable(),
	approvedAt: z.string().nullable(),
	deprecatedAt: z.string().nullable(),
	sourcePath: z.string().nullable(),
	sourceSizeBytes: z.number().int().nullable(),
	finalPath: z.string().nullable(),
	finalSizeBytes: z.number().int().nullable(),
	lastPrompt: z.string(),
});

// ─── Slot = groupId.entryId ───────────────────────────────────────────────

export interface IVideoSlotStatus {
	readonly approvedVariantId: string | null;
	readonly variants: Readonly<Record<string, IVideoVariantRecord>>;
}

export const VideoSlotStatusSchema = z.object({
	approvedVariantId: z.string().nullable(),
	variants: z.record(z.string(), VideoVariantRecordSchema),
});

// ─── Moments the settings board can switch off ────────────────────────────

/**
 * Mirrors `PCDynasty.Presentation.Core.VideoMoment`. A clip whose moment is
 * switched off in settings is never loaded, so a slot that declares no
 * moment would be unskippable — the validator rejects that.
 */
export type VideoMoment = 'intro' | 'match' | 'celebration';

export const VideoMomentSchema = z.enum(['intro', 'match', 'celebration']);

// ─── Spec content ─────────────────────────────────────────────────────────

export interface IVideoEntryDef {
	readonly id: string;
	readonly filename: string;
	readonly prompt: string;
	/** 4–15 seconds; the generator refuses anything outside that. */
	readonly durationSeconds: number;
}

export const VideoEntryDefSchema = z.object({
	id: z.string().min(1),
	filename: z.string().min(1),
	prompt: z.string().min(1),
	durationSeconds: z.number().int().min(4).max(15),
});

export interface IVideoGroup {
	readonly id: string;
	readonly moment: VideoMoment;
	readonly outputDir: string;
	readonly format: string;
	readonly model: string;
	readonly ratio: string;
	readonly styleAnchor: string;
	readonly coherence: Readonly<Record<string, unknown>>;
	readonly entries: readonly IVideoEntryDef[];
}

export const VideoGroupSchema = z.object({
	id: z.string().min(1),
	moment: VideoMomentSchema,
	outputDir: z.string().min(1),
	format: z.string().min(1),
	model: z.string().min(1),
	ratio: z.string().min(1),
	styleAnchor: z.string().min(1),
	coherence: z.record(z.string(), z.unknown()),
	entries: z.array(VideoEntryDefSchema).min(1),
});

/**
 * The look every clip shares, so six sequences generated months apart still
 * belong to the same game. The audio spec calls its equivalent
 * `globalMusicAnchor`; this is the same idea for the eye.
 */
export interface IVideoGlobalLookAnchor {
	readonly palette: string;
	readonly camera: string;
	readonly grade: string;
	readonly avoid: string;
	readonly ratio: string;
}

export const VideoGlobalLookAnchorSchema = z.object({
	palette: z.string().min(1),
	camera: z.string().min(1),
	grade: z.string().min(1),
	avoid: z.string().min(1),
	ratio: z.string().min(1),
});

export interface IVideoToolConfig {
	readonly command: string;
	readonly nonInteractive: boolean;
	readonly quiet: boolean;
}

export interface IVideoSpec {
	readonly version: number;
	readonly root: string;
	readonly stagingDir?: string;
	readonly deprecatedDir?: string;
	readonly tool: IVideoToolConfig;
	readonly globalLookAnchor?: IVideoGlobalLookAnchor;
	readonly groups: readonly IVideoGroup[];
	readonly status: Readonly<Record<string, IVideoSlotStatus>>;
}

export const VideoSpecSchema = z.object({
	version: z.number().int().positive(),
	root: z.string().min(1),
	stagingDir: z.string().optional(),
	deprecatedDir: z.string().optional(),
	tool: z.object({
		command: z.string().min(1),
		nonInteractive: z.boolean(),
		quiet: z.boolean(),
	}),
	globalLookAnchor: VideoGlobalLookAnchorSchema.optional(),
	groups: z.array(VideoGroupSchema).min(1),
	status: z.record(z.string(), VideoSlotStatusSchema),
});

// ─── Resolved slot (helper output) ────────────────────────────────────────

export interface IResolvedVideoSlot {
	readonly fullKey: string;
	readonly groupId: string;
	readonly entryId: string;
	readonly moment: VideoMoment;
	readonly group: IVideoGroup;
	readonly entry: IVideoEntryDef;
	readonly status: IVideoSlotStatus;
	readonly approvedVariant: IVideoVariantRecord | null;
	readonly requestedVariants: readonly IVideoVariantRecord[];
	readonly stagedVariants: readonly IVideoVariantRecord[];
	readonly deprecatedVariants: readonly IVideoVariantRecord[];
}

export interface IResolvedVideoManifest {
	readonly root: string;
	readonly stagingDir: string;
	readonly deprecatedDir: string;
	readonly version: number;
	readonly slots: readonly IResolvedVideoSlot[];
	readonly anchorRatio: string | null;
}

// ─── Encoding result (used by approve) ────────────────────────────────────

export interface IVideoEncodeResult {
	readonly ok: boolean;
	readonly stderr: string;
	readonly bytes: number | null;
	readonly codec: string | null;
}

// ─── The editing view ─────────────────────────────────────────────────────

/**
 * The read model above is `readonly` end to end so a consumer cannot
 * mutate the manifest by accident. The runner, however, exists to
 * *record* decisions: it stages a variant, marks one approved, moves
 * another to the archive. It needs one writable view, and it is
 * declared here once instead of being cast back at every call site —
 * a cast the type-checker cannot verify and a reader cannot audit.
 *
 * `readXxxSpec()` hands back a draft because it has just parsed a fresh
 * object off disk that nobody else holds a reference to. `writeXxxSpec()`
 * still accepts the readonly view, so a caller who only reads cannot be
 * tricked into writing.
 */

export interface IMutableVideoSlotStatus {
	approvedVariantId: string | null;
	variants: Record<string, IVideoVariantRecord>;
}

export type MutableVideoSpec = Omit<IVideoSpec, 'status' | 'stagingDir' | 'deprecatedDir'> & {
	stagingDir: string;
	deprecatedDir: string;
	status: Record<string, IMutableVideoSlotStatus>;
};
