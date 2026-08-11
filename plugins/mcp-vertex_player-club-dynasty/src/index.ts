import { definePlugin } from '@mcp-vertex/core/public';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';
import z from 'zod';

import type { IDataFile, IPlayer } from './lib/types';

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

interface ISquadEntry {
	id: string;
	name: string;
	number: number;
	position: IPlayer['position'];
	rating: number;
	fitness: number;
}

interface IClubEntry {
	id: string;
	name: string;
	stadium: string;
	founded: number;
	squadSize: number;
	averageRating: number;
}

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
		const dataFileAbs = ctx.workspace.resolve(opts.dataDir, 'clubs.json');
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
					},
					async () => {
						const data = readData(dataFileAbs);
						const clubs: IClubEntry[] = data.clubs.map((c) => ({
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
						inputSchema: z.object({ clubId: z.string().min(1) }).strict(),
					},
					async ({ clubId }: { clubId: string }) => {
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
						const players: ISquadEntry[] = [...club.squad]
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

		return {
			tools: [
				listClubsRegistration,
				getSquadRegistration,
				currentSeasonRegistration,
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
