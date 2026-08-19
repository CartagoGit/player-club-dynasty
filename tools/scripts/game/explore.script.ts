#!/usr/bin/env bun
/**
 * tools/scripts/game/explore.script.ts
 *
 * Walk the running game through a route and photograph every step.
 *
 * The screenshots are the point: a change of chrome is reviewed by looking
 * at it, not by describing it. The route is data rather than code, so
 * exploring somewhere new costs a command line instead of an edit to a
 * GDScript file.
 *
 * Usage:
 *   bun run game:explore '[{"shot":"menu"},{"press":"NewCareer"},{"shot":"setup"}]'
 *   bun run game:explore --plan routes/market.json
 *   bun run game:explore --tour                 # the default tour of every route
 *   bun run game:explore --out /tmp/pcd-shots '[{"dump":"*"}]'
 *
 * Steps are documented in `game/godot/Tests/Explore.gd`. The two most
 * useful when you do not know the tree yet:
 *
 *   {"dump": "*"}          print the whole node tree with types
 *   {"find": "NodeName"}   say whether a node exists and where it lives
 *
 * A build runs first unless `--no-build` is passed, because a stale
 * assembly photographs the previous version of the change.
 */

import { existsSync, mkdirSync, readFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

import { locateGodot } from '../../helpers/engine/godot.helper.ts';
import { resolveProjectPaths } from '../../helpers/system/path.helper.ts';
import { runInherit } from '../../helpers/system/run.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

/** Every route the shell can reach today, in the order a player meets them. */
const DEFAULT_TOUR: readonly Record<string, unknown>[] = [
	{ shot: '01-main-menu' },
	{ press: 'Options' },
	{ shot: '02-settings' },
	{ press: 'Back' },
	{ press: 'NewCareer' },
	{ shot: '03-career-setup' },
];

/** The only flags that consume the argument after them. */
const VALUED_FLAGS: ReadonlySet<string> = new Set(['--plan', '--out']);

const flagValue = (argv: readonly string[], flag: string): string | null => {
	const index = argv.indexOf(flag);
	return index === -1 || index + 1 >= argv.length ? null : argv[index + 1]!;
};

const readPlan = (argv: readonly string[]): string => {
	const planFile = flagValue(argv, '--plan');
	if (planFile !== null) {
		const path = resolve(paths.workspaceRoot, planFile);
		if (!existsSync(path)) {
			process.stderr.write(`explore: no plan at ${path}\n`);
			process.exit(2);
		}

		return readFileSync(path, 'utf8');
	}

	if (argv.includes('--tour')) {
		return JSON.stringify(DEFAULT_TOUR);
	}

	// The first argument that is neither a flag nor the value of one.
	// `--no-build` is a boolean, so what follows it is still the plan; only
	// the flags in VALUED_FLAGS swallow their next argument.
	const inline = argv.find((entry, index) => {
		if (entry.startsWith('--')) return false;
		const previous = argv[index - 1];
		return previous === undefined || !VALUED_FLAGS.has(previous);
	});
	if (inline === undefined) {
		process.stderr.write(
			'explore: pass a JSON plan, --plan <file>, or --tour. See the header of this script.\n',
		);
		process.exit(2);
	}

	return inline;
};

const main = async (): Promise<number> => {
	const argv = process.argv.slice(2);
	const plan = readPlan(argv);
	try {
		const parsed: unknown = JSON.parse(plan);
		if (!Array.isArray(parsed)) throw new Error('the plan must be a JSON array of steps');
	} catch (error) {
		process.stderr.write(`explore: ${(error as Error).message}\n`);
		return 2;
	}

	const outDir = resolve(
		paths.workspaceRoot,
		flagValue(argv, '--out') ?? join('.generate', 'shots'),
	);
	mkdirSync(outDir, { recursive: true });

	if (!argv.includes('--no-build')) {
		const built = await runInherit('dotnet', ['build', 'PCDynasty.sln', '-v', 'q', '--nologo'], {
			cwd: paths.gameRoot,
		});
		if (built !== 0) {
			process.stderr.write('explore: the build failed; nothing was photographed.\n');
			return built;
		}
	}

	const godot = locateGodot({ workspaceRoot: paths.workspaceRoot, toolsDir: paths.toolsDir });
	if (!godot) {
		process.stderr.write('explore: no Godot binary. Run `bun run game:resolve` first.\n');
		return 3;
	}

	process.stdout.write(`explore: ${outDir}\n`);
	return runInherit(
		godot.path,
		['--path', paths.projectDir, '--script', 'res://Tests/Explore.gd'],
		{
			cwd: paths.workspaceRoot,
			env: { PCDYNASTY_EXPLORE: plan, PCDYNASTY_SHOTS_DIR: outDir },
		},
	);
};

process.exit(await main());
