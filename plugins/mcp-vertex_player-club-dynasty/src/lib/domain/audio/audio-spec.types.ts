/**
 * Shared zod schemas for the audio manifest.
 *
 * The same contract is enforced by:
 *   - game/godot/Audio/audio-spec.json (declarative, source of truth)
 *   - scripts/generate-audio.script.ts (the runner that talks to mmx)
 *   - the mcp-vertex plugin tools that agents use to stage / list / approve
 */

import z from 'zod';

export const AudioVariantStateSchema = z.enum(['staged', 'approved', 'deprecated']);

export const AudioVariantRecordSchema = z.object({
	variantId: z.string().min(1),
	state: AudioVariantStateSchema,
	stagedAt: z.string().nullable(),
	approvedAt: z.string().nullable(),
	deprecatedAt: z.string().nullable(),
	audioPath: z.string().nullable().optional(),
	wavPath: z.string().nullable().optional(),
	finalPath: z.string().nullable(),
	finalSizeBytes: z.number().int().nullable(),
	audioSizeBytes: z.number().int().nullable().optional(),
	wavSizeBytes: z.number().int().nullable().optional(),
	lastPrompt: z.string(),
});

export const AudioSlotStatusSchema = z.object({
	approvedVariantId: z.string().nullable(),
	variants: z.record(z.string(), AudioVariantRecordSchema),
});

export const AudioCoherenceSchema = z.record(z.string(), z.unknown());

export const AudioEntryDefSchema = z.object({
	id: z.string().min(1),
	filename: z.string().min(1),
	prompt: z.string().min(1),
});

export const AudioGroupSchema = z.object({
	id: z.string().min(1),
	kind: z.enum(['music', 'sfx']),
	outputDir: z.string().min(1),
	format: z.string().min(1),
	model: z.string().min(1),
	styleAnchor: z.string().min(1),
	coherence: AudioCoherenceSchema,
	entries: z.array(AudioEntryDefSchema).min(1),
});

export const AudioGlobalMusicAnchorSchema = z.object({
	bpm: z.number().int().positive(),
	key: z.string().min(1),
	scaleNotes: z.array(z.string()).min(1),
	instruments: z.string().min(1),
	palette: z.string().optional(),
	loopSeconds: z.number().int().positive(),
	transitionRules: z.array(z.string().min(1)).min(1),
});

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

export type AudioSpec = z.infer<typeof AudioSpecSchema>;
export type AudioGroup = z.infer<typeof AudioGroupSchema>;
export type AudioEntryDef = z.infer<typeof AudioEntryDefSchema>;
export type AudioSlotStatus = z.infer<typeof AudioSlotStatusSchema>;
export type AudioVariantRecord = z.infer<typeof AudioVariantRecordSchema>;
export type AudioVariantState = z.infer<typeof AudioVariantStateSchema>;
export type AudioGlobalMusicAnchor = z.infer<typeof AudioGlobalMusicAnchorSchema>;