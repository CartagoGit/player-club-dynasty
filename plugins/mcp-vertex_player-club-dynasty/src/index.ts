import { definePlugin } from '@mcp-vertex/core/public';
import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, isAbsolute, join } from 'node:path';
import z from 'zod';

import {
	assertMusicAnchor,
	findSlot,
	loadAudioManifest,
} from './lib/audio-manifest.helper';
import { AudioVariantStateSchema } from './lib/audio-spec.types';
import type { IDataFile, IPlayer } from './lib/game-data.types';

const OptionsSchema = z
	.object({
		dataDir: z.string().default('data'),
		savesDir: z.string().default('saves'),
	})
	.strict();

const DEFAULT_OPTIONS = {
	dataDir: 'data',
	savesDir: 'saves',
} as const;

const PlayerPositionSchema = z.enum(['GK', 'DEF', 'MID', 'FWD']);
const SquadEntrySchema = z.object({
	id: z.string(),
	name: z.string(),
	number: z.number().int(),
	position: PlayerPositionSchema,
	rating: z.number(),
	fitness: z.number(),
});
const ClubEntrySchema = z.object({
	id: z.string(),
	name: z.string(),
	stadium: z.string(),
	founded: z.number().int(),
	squadSize: z.number().int().nonnegative(),
	averageRating: z.number(),
});
const ListClubsOutputSchema = z.object({ clubs: z.array(ClubEntrySchema) });
const GetSquadInputSchema = z.object({ clubId: z.string().min(1) }).strict();
const GetSquadOutputSchema = z.object({ players: z.array(SquadEntrySchema) });
const CurrentSeasonOutputSchema = z.object({
	season: z.number().int(),
	clubCount: z.number().int().nonnegative(),
});

type SquadEntry = z.infer<typeof SquadEntrySchema>;
type ClubEntry = z.infer<typeof ClubEntrySchema>;

const ensureFile = (path: string, defaultValue: IDataFile): void => {
	if (existsSync(path)) return;
	mkdirSync(dirname(path), { recursive: true });
	writeFileSync(path, JSON.stringify(defaultValue, null, 2));
};

const readData = (path: string): IDataFile => {
	const raw = readFileSync(path, 'utf8');
	return JSON.parse(raw) as IDataFile;
};

const computeAverageRating = (squad: IPlayer[]): number => {
	if (squad.length === 0) return 0;
	const total = squad.reduce((sum, p) => sum + p.rating, 0);
	return Math.round((total / squad.length) * 10) / 10;
};

export default definePlugin({
	name: 'player-club-dynasty',
	version: '0.1.0',
	describe:
		'Player Club Dynasty domain tools: roster, club cards, season clock, and match log exposed to MCP agents.',
	cacheNamespace: 'results',
	optionsSchema: OptionsSchema,
	register(ctx) {
		const parsed = OptionsSchema.safeParse(ctx.options);
		const opts = parsed.success ? parsed.data : DEFAULT_OPTIONS;
		const prefix = ctx.namespacePrefix;
		const dataFileAbs = ctx.workspace.resolve(join(opts.dataDir, 'clubs.json'));
		ensureFile(dataFileAbs, { clubs: [], currentSeason: 1 });

		const listClubsRegistration = {
			id: `${prefix}_list_clubs`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_list_clubs`,
					{
						description:
							'Return every club in the saved dataset with stadium, founded year, squad size and average rating.',
						inputSchema: z.object({}).strict(),
						outputSchema: ListClubsOutputSchema,
					},
					async () => {
						const data = readData(dataFileAbs);
						const clubs: ClubEntry[] = data.clubs.map((c) => ({
							id: c.id,
							name: c.name,
							stadium: c.stadium,
							founded: c.founded,
							squadSize: c.squad.length,
							averageRating: computeAverageRating(c.squad),
						}));
						return {
							content: [
								{ type: 'text', text: JSON.stringify({ clubs }) },
							],
							structuredContent: { clubs },
						};
					},
				);
			},
		};

		const getSquadRegistration = {
			id: `${prefix}_get_squad`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_get_squad`,
					{
						description:
							'Return every player in a given club, ordered by shirt number.',
						inputSchema: GetSquadInputSchema,
						outputSchema: GetSquadOutputSchema,
					},
					async ({ clubId }: z.infer<typeof GetSquadInputSchema>) => {
						const data = readData(dataFileAbs);
						const club = data.clubs.find((c) => c.id === clubId);
						if (!club) {
							return {
								content: [
									{
										type: 'text',
										text: JSON.stringify({ error: `club ${clubId} not found` }),
									},
								],
								isError: true,
							};
						}
						const players: SquadEntry[] = [...club.squad]
							.sort((a, b) => a.number - b.number)
							.map((p) => ({
								id: p.id,
								name: p.name,
								number: p.number,
								position: p.position,
								rating: p.rating,
								fitness: p.fitness,
							}));
						return {
							content: [
								{ type: 'text', text: JSON.stringify({ players }) },
							],
							structuredContent: { players },
						};
					},
				);
			},
		};

		const currentSeasonRegistration = {
			id: `${prefix}_current_season`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_current_season`,
					{
						description:
							'Return the in-progress season number and the count of clubs known to the dataset.',
						inputSchema: z.object({}).strict(),
						outputSchema: CurrentSeasonOutputSchema,
					},
					async () => {
						const data = readData(dataFileAbs);
						const payload = {
							season: data.currentSeason,
							clubCount: data.clubs.length,
						};
						return {
							content: [
								{ type: 'text', text: JSON.stringify(payload) },
							],
							structuredContent: payload,
						};
					},
				);
			},
		};

		// --- audio review pipeline tools ------------------------------------------
		const runAudioScript = (
			args: readonly string[],
		): { ok: boolean; stdout: string; stderr: string } => {
			const result = spawnSync(
				'bun',
				[join(ctx.workspace.root, 'tools', 'scripts', 'content', 'generate-audio.script.ts'), ...args],
				{ encoding: 'utf8', cwd: ctx.workspace.root },
			);
			return {
				ok: result.status === 0,
				stdout: result.stdout ?? '',
				stderr: result.stderr ?? '',
			};
		};

		const AudioVariantViewSchema = z.object({
			variantId: z.string(),
			state: AudioVariantStateSchema,
			wavPath: z.string().nullable(),
			finalPath: z.string().nullable(),
			wavSizeBytes: z.number().int().nullable(),
			finalSizeBytes: z.number().int().nullable(),
			stagedAt: z.string().nullable(),
			approvedAt: z.string().nullable(),
			deprecatedAt: z.string().nullable(),
		});
		type AudioVariantView = z.infer<typeof AudioVariantViewSchema>;

		const AudioSlotViewSchema = z.object({
			fullKey: z.string(),
			groupId: z.string(),
			entryId: z.string(),
			groupKind: z.enum(['music', 'sfx']),
			filename: z.string(),
			approvedVariant: AudioVariantViewSchema.nullable(),
			stagedVariants: z.array(AudioVariantViewSchema),
			deprecatedVariants: z.array(AudioVariantViewSchema),
		});

		const AudioStageInputSchema = z
			.object({ fullKey: z.string().min(1), variantId: z.string().optional() })
			.strict();
		const AudioStageOutputSchema = z.object({
			fullKey: z.string(),
			variantId: z.string(),
			wavPath: z.string(),
			wavSizeBytes: z.number().int().nullable(),
			listenCommand: z.string(),
		});
		const audioStageRegistration = {
			id: `${prefix}_audio_stage`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_audio_stage`,
					{
						description:
							'Generate a new audio variant for a slot (e.g. "sfx.ui.ui_click") into .audio-staging/. Returns the wav path and a "listen" command the human can use to preview the variant.',
						inputSchema: AudioStageInputSchema,
						outputSchema: AudioStageOutputSchema,
					},
					async ({ fullKey, variantId }: z.infer<typeof AudioStageInputSchema>) => {
						const args: string[] = ['stage', fullKey];
						if (variantId) args.push('--variant', variantId);
						const result = runAudioScript(args);
						if (!result.ok) {
							return { content: [{ type: 'text', text: result.stderr || result.stdout }], isError: true };
						}
						const variantIdMatch = result.stdout.match(/variant (v_\d+)/);
						const wavMatch = result.stdout.match(/stage \S+ → (\S+)/);
						const listenCommand = `bun tools/scripts/content/generate-audio.script.ts listen ${fullKey} --variant ${variantIdMatch?.[1] ?? '?'}`;
						const payload = {
							fullKey,
							variantId: variantIdMatch?.[1] ?? '?',
							wavPath: wavMatch?.[1] ?? '',
							wavSizeBytes: null,
							listenCommand,
						};
						return { content: [{ type: 'text', text: JSON.stringify(payload) }], structuredContent: payload };
					},
				);
			},
		};

		const AudioListVariantsInputSchema = z
			.object({
				fullKey: z.string().optional(),
				groupId: z.string().optional(),
				state: AudioVariantStateSchema.optional(),
			})
			.strict();
		const AudioListVariantsOutputSchema = z.object({
			anchorBpm: z.number().nullable(),
			anchorKey: z.string().nullable(),
			slots: z.array(AudioSlotViewSchema),
		});
		const audioListVariantsRegistration = {
			id: `${prefix}_audio_list_variants`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_audio_list_variants`,
					{
						description:
							'List every slot and its variants. Optionally filter by slot, groupId or variant state.',
						inputSchema: AudioListVariantsInputSchema,
						outputSchema: AudioListVariantsOutputSchema,
					},
					async ({ fullKey, groupId, state }: z.infer<typeof AudioListVariantsInputSchema>) => {
						const manifest = loadAudioManifest(ctx.workspace.root);
						assertMusicAnchor(manifest);
						const slots = manifest.slots
							.filter((slot) => {
								if (fullKey !== undefined && slot.fullKey !== fullKey) return false;
								if (groupId !== undefined && slot.groupId !== groupId) return false;
								return true;
							})
							.map((slot) => {
								const toView = (variant: typeof slot.approvedVariant): AudioVariantView | null =>
									variant
										? {
												variantId: variant.variantId,
												state: variant.state,
												wavPath: variant.wavPath,
												finalPath: variant.finalPath,
												wavSizeBytes: variant.wavSizeBytes,
												finalSizeBytes: variant.finalSizeBytes,
												stagedAt: variant.stagedAt,
												approvedAt: variant.approvedAt,
												deprecatedAt: variant.deprecatedAt,
											}
										: null;
								const allStaged = state === undefined || state === 'staged' ? slot.stagedVariants : [];
								const allDeprecated =
									state === undefined || state === 'deprecated' ? slot.deprecatedVariants : [];
								const approved = state === undefined || state === 'approved' ? slot.approvedVariant : null;
								return {
									fullKey: slot.fullKey,
									groupId: slot.groupId,
									entryId: slot.entryId,
									groupKind: slot.groupKind,
									filename: slot.entry.filename,
									approvedVariant: toView(approved),
									stagedVariants: allStaged
										.map((v) => toView(v))
										.filter((v): v is AudioVariantView => v !== null),
									deprecatedVariants: allDeprecated
										.map((v) => toView(v))
										.filter((v): v is AudioVariantView => v !== null),
								};
							});
						const payload = { anchorBpm: manifest.anchorBpm, anchorKey: manifest.anchorKey, slots };
						return { content: [{ type: 'text', text: JSON.stringify(payload) }], structuredContent: payload };
					},
				);
			},
		};

		const AudioListenInputSchema = z
			.object({ fullKey: z.string().min(1), variantId: z.string().min(1) })
			.strict();
		const AudioListenOutputSchema = z.object({
			fullKey: z.string(),
			variantId: z.string(),
			wavPath: z.string(),
			playbackHint: z.string(),
		});
		const audioListenRegistration = {
			id: `${prefix}_audio_listen`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_audio_listen`,
					{
						description:
							'Return the absolute path of a staged wav variant. The human opens that path in their audio player of choice.',
						inputSchema: AudioListenInputSchema,
						outputSchema: AudioListenOutputSchema,
					},
					async ({ fullKey, variantId }: z.infer<typeof AudioListenInputSchema>) => {
						const manifest = loadAudioManifest(ctx.workspace.root);
						const slot = findSlot(manifest, fullKey);
						const variant = slot.status.variants[variantId];
						if (!variant) {
							return { content: [{ type: 'text', text: `variant ${variantId} not found` }], isError: true };
						}
						if (!variant.wavPath) {
							return {
								content: [{ type: 'text', text: `variant ${variantId} has no wav path on disk` }],
								isError: true,
							};
						}
						const wavPath = isAbsolute(variant.wavPath)
							? variant.wavPath
							: join(ctx.workspace.root, variant.wavPath);
						const payload = {
							fullKey,
							variantId,
							wavPath,
							playbackHint:
								'open the file with your audio player, e.g. `xdg-open <path>` or `mpv <path>`',
						};
						return { content: [{ type: 'text', text: JSON.stringify(payload) }], structuredContent: payload };
					},
				);
			},
		};

		const AudioApproveInputSchema = z
			.object({ fullKey: z.string().min(1), variantId: z.string().min(1) })
			.strict();
		const AudioApproveOutputSchema = z.object({
			fullKey: z.string(),
			variantId: z.string(),
			finalPath: z.string(),
			wavSizeBytes: z.number().int().nullable(),
			finalSizeBytes: z.number().int().nullable(),
			archivedPreviousVariantId: z.string().nullable(),
		});
		const audioApproveRegistration = {
			id: `${prefix}_audio_approve`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_audio_approve`,
					{
						description:
							'Encode a staged variant to the slot format (.ogg / .opus / etc.) via ffmpeg, move it to game/godot/Audio/<Music|SFX>/<filename>, and archive the previously approved variant under .deprecated/assets/sounds/.',
						inputSchema: AudioApproveInputSchema,
						outputSchema: AudioApproveOutputSchema,
					},
					async ({ fullKey, variantId }: z.infer<typeof AudioApproveInputSchema>) => {
						const result = runAudioScript(['approve', fullKey, '--variant', variantId]);
						if (!result.ok) {
							return { content: [{ type: 'text', text: result.stderr || result.stdout }], isError: true };
						}
						const finalMatch = result.stdout.match(/final:\s+(\S+)/);
						const wavSizeMatch = result.stdout.match(/wav:\s+\S+\s+\((\d+)\s+KB\)/);
						const finalSizeMatch = result.stdout.match(/final:\s+\S+\s+\(([\d.]+)\s+KB/);
						const manifest = loadAudioManifest(ctx.workspace.root);
						const slot = findSlot(manifest, fullKey);
						const previousId =
							slot.approvedVariant && slot.approvedVariant.variantId !== variantId
								? slot.approvedVariant.variantId
								: null;
						const payload = {
							fullKey,
							variantId,
							finalPath: finalMatch?.[1] ?? '',
							wavSizeBytes: wavSizeMatch?.[1] ? Number(wavSizeMatch[1]) * 1024 : null,
							finalSizeBytes: finalSizeMatch?.[1] ? Number(finalSizeMatch[1]) * 1024 : null,
							archivedPreviousVariantId: previousId,
						};
						return { content: [{ type: 'text', text: JSON.stringify(payload) }], structuredContent: payload };
					},
				);
			},
		};

		const AudioDeprecateInputSchema = z
			.object({ fullKey: z.string().min(1), variantId: z.string().min(1) })
			.strict();
		const AudioDeprecateOutputSchema = z.object({
			fullKey: z.string(),
			variantId: z.string(),
			archivedPath: z.string().nullable(),
			slotNowEmpty: z.boolean(),
		});
		const audioDeprecateRegistration = {
			id: `${prefix}_audio_deprecate`,
			register: async (server: any) => {
				server.registerTool(
					`${prefix}_audio_deprecate`,
					{
						description:
							'Move any variant (staged or approved) into .deprecated/assets/sounds/. Frees the slot for a fresh stage run.',
						inputSchema: AudioDeprecateInputSchema,
						outputSchema: AudioDeprecateOutputSchema,
					},
					async ({ fullKey, variantId }: z.infer<typeof AudioDeprecateInputSchema>) => {
						const result = runAudioScript(['deprecate', fullKey, '--variant', variantId]);
						if (!result.ok) {
							return { content: [{ type: 'text', text: result.stderr || result.stdout }], isError: true };
						}
						const manifest = loadAudioManifest(ctx.workspace.root);
						const slot = findSlot(manifest, fullKey);
						const variant = slot.status.variants[variantId];
						const payload = {
							fullKey,
							variantId,
							archivedPath: variant?.finalPath ?? variant?.wavPath ?? null,
							slotNowEmpty: slot.approvedVariant === null,
						};
						return { content: [{ type: 'text', text: JSON.stringify(payload) }], structuredContent: payload };
					},
				);
			},
		};

		return {
			tools: [
				listClubsRegistration,
				getSquadRegistration,
				currentSeasonRegistration,
				audioStageRegistration,
				audioListVariantsRegistration,
				audioListenRegistration,
				audioApproveRegistration,
				audioDeprecateRegistration,
			],
			knowledge: [
				{
					id: `${prefix}-overview`,
					title: 'Player Club Dynasty domain plugin',
					body: 'Exposes the football-manager dataset (clubs, squads, season clock) over MCP. Use `player-club-dynasty_list_clubs` first to enumerate clubs, then `player-club-dynasty_get_squad` for any club id, and `player-club-dynasty_current_season` to read the in-progress season. The dataset lives in `<dataDir>/clubs.json` (default `data/clubs.json`) and is auto-created with an empty fixture on first read.',
				},
			],
			skills: [],
		};
	},
});
