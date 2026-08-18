#!/usr/bin/env bun
/**
 * tools/helpers/path.helper.ts
 *
 * Helpers for the layout every script shares: workspace root, game root,
 * project root, tools dir. Centralised so renames never cascade.
 */

import { dirname, isAbsolute, join, resolve } from 'node:path';

import { HOME_ENV_VAR, PATH_ENV_VAR, PATH_LIST_SEPARATOR } from '../contracts/constants/env.constants.ts';
import {
	ARCHITECTURE_TEST_PROJECT,
	BENCHMARK_ARTIFACTS_DIR,
	BENCHMARK_PROJECT,
	GAME_SOLUTION_FILE,
	GODOT_PRESENTATION_CSPROJ,
	GODOT_VERSION_FILE_RELATIVE,
	TOOLS_DIR,
	TOOLS_GODOT_DIR,
} from '../contracts/constants/fs.constants.ts';
import { PCDYNASTY_TOOLS_DIR_ENV_VAR } from '../contracts/constants/godot.constants.ts';

/** Absolute path of the workspace root (the directory that holds `package.json`). */
export const workspaceRoot = (fromScript: string): string => {
	return resolve(fromScript, '..', '..', '..');
};

export interface IProjectPaths {
	readonly workspaceRoot: string;
	readonly gameRoot: string;
	readonly projectDir: string;
	readonly csproj: string;
	readonly solution: string;
	readonly architectureTestProject: string;
	readonly benchmarkProject: string;
	readonly benchmarkArtifactsDir: string;
	readonly godotVersionFile: string;
	readonly toolsDir: string;
	readonly godotToolsDir: string;
}

/** Resolve the canonical project paths from any script location. */
export const resolveProjectPaths = (fromScript: string): IProjectPaths => {
	const root = workspaceRoot(fromScript);
	const gameRoot = join(root, 'game');
	const toolsDir = resolveToolsDir(root);
	return {
		workspaceRoot: root,
		gameRoot,
		projectDir: join(gameRoot, 'godot'),
		csproj: join(gameRoot, 'godot', GODOT_PRESENTATION_CSPROJ),
		solution: join(gameRoot, GAME_SOLUTION_FILE),
		architectureTestProject: join(gameRoot, ARCHITECTURE_TEST_PROJECT),
		benchmarkProject: join(gameRoot, BENCHMARK_PROJECT),
		benchmarkArtifactsDir: join(gameRoot, BENCHMARK_ARTIFACTS_DIR),
		godotVersionFile: join(root, GODOT_VERSION_FILE_RELATIVE),
		toolsDir,
		godotToolsDir: join(toolsDir, TOOLS_GODOT_DIR),
	};
};

/** Resolve `${PCDYNASTY_TOOLS_DIR}` if set, otherwise `<workspace>/.tools`. */
export const resolveToolsDir = (root: string): string => {
	const override = process.env[PCDYNASTY_TOOLS_DIR_ENV_VAR];
	if (override && override.length > 0) return isAbsolute(override) ? override : join(root, override);
	return join(root, TOOLS_DIR);
};

/** First element of `$PATH` that contains an executable named `name`. */
export const locateOnPath = (name: string): string | null => {
	for (const dir of (process.env[PATH_ENV_VAR] ?? '').split(PATH_LIST_SEPARATOR)) {
		if (!dir) continue;
		const candidate = join(dir, name);
		try {
			const stat = Bun.file(candidate);
			if (stat.size > 0) return candidate;
		} catch {
			// continue scanning
		}
	}
	return null;
};

/** Returns `$HOME` or, if missing, the cwd (the latter should never happen). */
export const homeOrCwd = (): string => {
	const home = process.env[HOME_ENV_VAR];
	if (home && home.length > 0) return home;
	return process.cwd();
};

/** Repeat `dirname()` until the entry matches `predicate`. */
export const findAncestor = (start: string, predicate: (path: string) => boolean): string | null => {
	let current = resolve(start);
	const { root } = parse(current);
	while (true) {
		if (predicate(current)) return current;
		if (current === root) return null;
		current = dirname(current);
	}
};
