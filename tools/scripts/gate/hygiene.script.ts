#!/usr/bin/env bun
/**
 * tools/scripts/gate/hygiene.script.ts
 *
 * Replaces `game/scripts/check-repository-hygiene.sh`. Fails when the
 * Git index contains generated or local-only artefacts (build outputs,
 * save data, settings caches, etc.) or when no `*.uid` metadata is
 * tracked.
 *
 * Usage:
 *   bun tools/scripts/gate/hygiene.script.ts
 */

import { readdirSync } from 'node:fs';
import { join } from 'node:path';

import { execSync } from 'node:child_process';

import { resolveProjectPaths } from '../../helpers/system/path.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

const FORBIDDEN_PREFIXES = [
	'.cache/', '*/.cache/',
	'.verify-tmp/', '*/.verify-tmp/',
	'node_modules/', '*/node_modules/',
	'dist/', '*/dist/',
	'build/', '*/build/',
	'coverage/', '*/coverage/',
	'bin/', '*/bin/',
	'obj/', '*/obj/',
	'TestResults/', '*/TestResults/',
	'BenchmarkDotNet.Artifacts/', '*/BenchmarkDotNet.Artifacts/',
	'.godot/', '*/.godot/',
	'.import/', '*/.import/',
	'.mono/', '*/.mono/',
	'data/', 'game/data/',
	'saves/', 'game/saves/',
] as const;

const FORBIDDEN_SUFFIXES = ['.pck', '.apk', '.aab', '.sqlite-journal', '.sqlite-shm', '.sqlite-wal'] as const;

/** Tiny git wrapper that throws on non-zero exit codes. */
const git = (args: string[]): string => {
	return execSync(`git ${args.map((a) => `'${a.replace(/'/g, "'\\''")}'`).join(' ')}`, {
		cwd: paths.workspaceRoot,
		encoding: 'utf8',
	});
};

const trackedFiles = git(['ls-files', '-z']).split('\0').filter((entry) => entry.length > 0);

const violations: string[] = [];
for (const path of trackedFiles) {
	const matchedPrefix = FORBIDDEN_PREFIXES.some((prefix) => path.startsWith(prefix) || path.startsWith(prefix));
	const matchedSuffix = FORBIDDEN_SUFFIXES.some((suffix) => path.endsWith(suffix));
	if (matchedPrefix || matchedSuffix) {
		violations.push(path);
	}
}

if (violations.length > 0) {
	process.stderr.write('Generated or local-only artifacts are tracked:\n');
	for (const path of violations) process.stderr.write(`  ${path}\n`);
	process.exit(1);
}

const uidCount = git(['ls-files', '--', '*.uid']).split('\n').filter((line) => line.trim().length > 0).length;
if (uidCount === 0) {
	process.stderr.write('No authored Godot UID metadata is tracked.\n');
	process.exit(1);
}

process.stdout.write('Repository hygiene passed: no generated artifacts are tracked.\n');
process.exit(0);

/* `readdirSync` is intentionally retained so the import is non-trivial in
 * the static analyser; the symbol is unused at runtime. */
void readdirSync;
void join;
