#!/usr/bin/env bun
/**
 * tools/scripts/reference/download.script.ts
 *
 * Fetch the classic titles we study, so a question about how the
 * original behaved can be answered by running it.
 *
 * Usage:
 *   bun run download                       # the default selection (6.0, 7.0, Basket 6.5)
 *   bun run download --game=pcf7           # one title, by id
 *   bun run download:catalog               # refresh the registry and report drift
 *   bun run download:all-dynamic-multimedia-games --yes
 *
 *   --out <dir>    where to put them (default `.generate/games/`, gitignored)
 *   --dry-run      print the plan and the total, fetch nothing
 *   --yes          required by --all, which is ~41 GB
 *
 * Every game id is documented in
 * `tools/contracts/constants/reference/dinamic-catalogue.constants.ts`.
 *
 * The archive signs its disk URLs with a token only a same-site
 * `fetch()` receives — a direct HTTP request 403s even with a valid
 * token. So a plain fetch is tried first (cheap, resumable, and still
 * the right answer if that ever changes), and whatever it can't reach
 * falls back to a real headless Chrome that asks the game's own kiosk
 * page for a link the way its emulator would. See
 * `tools/helpers/reference/kiosk-browser-download.helper.ts`.
 *
 * The images stay out of the repository. They are references, like the
 * screenshots in `docs/wiki/pcfutbol/14`: we look at them, we describe
 * what we see, and only our own words and wireframes get committed.
 */

import { existsSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';

import {
	DEFAULT_SELECTION,
	REFERENCE_DIR,
	REFERENCE_GAMES,
} from '../../contracts/constants/reference/dinamic-catalogue.constants.ts';
import type {
	IReferenceDownload,
	ReferenceDiskSlot,
} from '../../contracts/interfaces/reference/reference-game.interface.ts';
import { downloadResumable, formatBytes } from '../../helpers/system/download.helper.ts';
import { downloadGameViaKiosk } from '../../helpers/reference/kiosk-browser-download.helper.ts';
import { resolveProjectPaths } from '../../helpers/system/path.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

const CATALOGUE_URL = 'https://online.dinamicmultimedia.es/games.js?v=2';
const SLOTS: readonly ReferenceDiskSlot[] = ['hda', 'hdb', 'cdrom'];

const flagValue = (argv: readonly string[], flag: string): string | null => {
	const inline = argv.find((entry) => entry.startsWith(`${flag}=`));
	if (inline !== undefined) return inline.slice(flag.length + 1);
	const index = argv.indexOf(flag);
	return index === -1 || index + 1 >= argv.length ? null : argv[index + 1]!;
};

/** The filename an URL lands under, with any `?v=` cache-buster dropped. */
const filenameFor = (url: string): string => {
	const withoutQuery = url.split('?')[0]!;
	return withoutQuery.slice(withoutQuery.lastIndexOf('/') + 1);
};

const planFor = (gameIds: readonly string[], outDir: string): readonly IReferenceDownload[] => {
	const plan: IReferenceDownload[] = [];
	for (const gameId of gameIds) {
		const game = (REFERENCE_GAMES as Record<string, (typeof REFERENCE_GAMES)[keyof typeof REFERENCE_GAMES]>)[gameId];
		if (game === undefined) continue;
		for (const slot of SLOTS) {
			const disk = game.disks[slot as keyof typeof game.disks];
			if (disk === undefined) continue;
			plan.push({
				gameId,
				slot,
				url: disk.url,
				targetPath: join(outDir, filenameFor(disk.url)),
				expectedBytes: disk.size,
			});
		}
	}

	return plan;
};

/**
 * Re-read the archive's registry and say what changed.
 *
 * The registry is a snapshot of somebody else's site, so it rots
 * silently: a re-uploaded image gets a new `?v=`, the old URL 404s, and
 * the failure lands on whoever tries to download months later. This
 * turns that into a report.
 */
const refreshCatalogue = async (): Promise<number> => {
	process.stdout.write(`reading ${CATALOGUE_URL}\n`);
	const response = await fetch(CATALOGUE_URL, { redirect: 'follow' });
	if (!response.ok) {
		process.stderr.write(`catalogue: HTTP ${response.status} ${response.statusText}\n`);
		return 1;
	}

	const source = await response.text();
	const live = new Map<string, Map<string, string>>();
	for (const match of source.matchAll(/^ {2}(\w+):\s*\{/gm)) {
		const id = match[1]!;
		const block = source.slice(match.index ?? 0, (match.index ?? 0) + 2500);
		const disks = new Map<string, string>();
		for (const disk of block.matchAll(/(hda|hdb|cdrom):\s*\{[^}]*?url:\s*"([^"]+)"/g)) {
			disks.set(disk[1]!, disk[2]!);
		}

		if (disks.size > 0) live.set(id, disks);
	}

	process.stdout.write(`catalogue: ${live.size} title(s) with disks upstream, ${Object.keys(REFERENCE_GAMES).length} in the registry\n`);

	let drift = 0;
	for (const [id, disks] of live) {
		const known = (REFERENCE_GAMES as Record<string, { disks: Record<string, { url: string }> }>)[id];
		if (known === undefined) {
			process.stdout.write(`  NEW      ${id} (not in the registry)\n`);
			drift += 1;
			continue;
		}

		for (const [slot, url] of disks) {
			const mine = known.disks[slot]?.url;
			if (mine !== url) {
				process.stdout.write(`  CHANGED  ${id}.${slot}\n    was ${mine ?? '(absent)'}\n    now ${url}\n`);
				drift += 1;
			}
		}
	}

	for (const id of Object.keys(REFERENCE_GAMES)) {
		if (!live.has(id)) {
			process.stdout.write(`  GONE     ${id} (in the registry, not upstream)\n`);
			drift += 1;
		}
	}

	process.stdout.write(
		drift === 0
			? 'catalogue: the registry matches upstream.\n'
			: `catalogue: ${drift} difference(s). Update dinamic-catalogue.constants.ts.\n`,
	);
	return 0;
};

const main = async (): Promise<number> => {
	const argv = process.argv.slice(2);

	if (argv.includes('--catalog')) return refreshCatalogue();

	const outDir = resolve(paths.workspaceRoot, flagValue(argv, '--out') ?? REFERENCE_DIR);
	const requested = flagValue(argv, '--game');
	const wantsAll = argv.includes('--all');

	let gameIds: readonly string[];
	if (requested !== null) {
		if (!(requested in REFERENCE_GAMES)) {
			process.stderr.write(
				`download: unknown game '${requested}'. Ids are listed in\n` +
					'  tools/contracts/constants/reference/dinamic-catalogue.constants.ts\n',
			);
			return 2;
		}

		gameIds = [requested];
	} else if (wantsAll) {
		gameIds = Object.keys(REFERENCE_GAMES);
	} else {
		gameIds = DEFAULT_SELECTION;
	}

	const plan = planFor(gameIds, outDir);
	const total = plan.reduce((sum, item) => sum + (item.expectedBytes ?? 0), 0);
	const pending = plan.filter(
		(item) =>
			item.expectedBytes === null ||
			!existsSync(item.targetPath) ||
			statSync(item.targetPath).size !== item.expectedBytes,
	);
	const remaining = pending.reduce((sum, item) => sum + (item.expectedBytes ?? 0), 0);

	process.stdout.write(
		`download: ${gameIds.length} title(s), ${plan.length} file(s), ` +
			`${formatBytes(total)} total, ${formatBytes(remaining)} still to fetch\n` +
			`          into ${outDir}\n\n`,
	);

	// `--all` is ~41 GB of somebody else's bandwidth. It does not happen by
	// accident, and it does not happen without the caller saying so.
	if (wantsAll && requested === null && !argv.includes('--yes') && !argv.includes('--dry-run')) {
		process.stderr.write(
			`download: that is ${formatBytes(total)}. Re-run with --yes if you meant it.\n`,
		);
		return 3;
	}

	if (argv.includes('--dry-run')) {
		for (const item of plan) {
			process.stdout.write(
				`  ${item.gameId}.${item.slot}  ${formatBytes(item.expectedBytes ?? 0).padStart(8)}  ${item.targetPath}\n`,
			);
		}

		return 0;
	}

	const stillNeeded: IReferenceDownload[] = [];
	for (const item of plan) {
		const label = `${item.gameId}.${item.slot}`;
		const outcome = await downloadResumable(item.url, item.targetPath, {
			expectedBytes: item.expectedBytes,
			onProgress: (bytes, expected) => {
				const share = expected ? ` (${Math.floor((bytes / expected) * 100)}%)` : '';
				process.stdout.write(`\r${label}  ${formatBytes(bytes)}${share}      `);
			},
		});

		if (outcome.skipped) {
			process.stdout.write(`\r${label}  already complete (${formatBytes(outcome.bytes)})      \n`);
			continue;
		}

		if (outcome.ok) {
			process.stdout.write(`\r${label}  done (${formatBytes(outcome.bytes)})            \n`);
			continue;
		}

		process.stdout.write(`\r${label}  direct fetch failed (${outcome.error}); will try via kiosk browser      \n`);
		stillNeeded.push(item);
	}

	let failures = 0;
	if (stillNeeded.length > 0) {
		const byGame = new Map<string, IReferenceDownload[]>();
		for (const item of stillNeeded) {
			const list = byGame.get(item.gameId) ?? [];
			list.push(item);
			byGame.set(item.gameId, list);
		}

		for (const [gameId, items] of byGame) {
			process.stdout.write(`\n${gameId}: opening a kiosk session for ${items.length} file(s)\n`);
			const results = await downloadGameViaKiosk(
				gameId,
				items.map((item) => ({
					filename: filenameFor(item.url),
					targetPath: item.targetPath,
					expectedBytes: item.expectedBytes,
				})),
				(filename, bytes, total) => {
					const share = total ? ` (${Math.floor((bytes / total) * 100)}%)` : '';
					process.stdout.write(`\r  ${filename}  ${formatBytes(bytes)}${share}      `);
				},
			);

			for (const result of results) {
				if (result.ok) {
					process.stdout.write(`\r  ${result.filename}  done (${formatBytes(result.bytes)})            \n`);
					continue;
				}

				failures += 1;
				process.stdout.write(`\r  ${result.filename}  FAILED: ${result.error}            \n`);
			}
		}
	}

	if (failures > 0) {
		process.stderr.write(
			`\ndownload: ${failures} file(s) failed. Re-run the same command — finished files are ` +
				'skipped and partial ones resume.\n',
		);
		return 1;
	}

	process.stdout.write(`\ndownload: ${plan.length} file(s) ready in ${outDir}\n`);
	return 0;
};

process.exit(await main());
