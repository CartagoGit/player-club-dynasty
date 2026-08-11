#!/usr/bin/env bun
/**
 * Build a single plugin's `dist/` using `bun build` directly.
 *
 * The mcp-vertex upstream build pipeline (`bun tools/scripts/compile/
 * build.script.ts plugins/<name>`) only knows how to build plugins that
 * live **inside** its own `plugins/` directory; here our plugins live
 * alongside the game (workspace-local), so we wrap `bun build` with
 * the same shape (entry + externals-resolve + target=node).
 *
 * Usage:
 *   bun scripts/build-plugin.script.ts mcp-vertex_player-club-dynasty
 *   bun scripts/build-plugin.script.ts mcp-vertex_player-club-dynasty --watch
 */

import { spawnSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join, resolve } from 'node:path';

const args = process.argv.slice(2);
const pluginName = args[0];
if (!pluginName) {
	process.stderr.write(
		'Usage: bun scripts/build-plugin.script.ts <plugin-name> [--watch]\n',
	);
	process.exit(2);
}

const workspaceRoot = resolve(import.meta.dirname, '..');
const pluginRoot = join(workspaceRoot, 'plugins', pluginName);

if (!existsSync(join(pluginRoot, 'package.json'))) {
	process.stderr.write(`Plugin not found at plugins/${pluginName}.\n`);
	process.exit(2);
}

const buildArgs = [
	'bun',
	'build',
	'./src/index.ts',
	'--outdir=./dist',
	'--target=node',
	'--format=esm',
	'--sourcemap=external',
	'--external=@mcp-vertex/*',
	...args.slice(1),
];

process.stderr.write(`▸ building plugins/${pluginName}\n`);

const result = spawnSync(buildArgs[0]!, buildArgs.slice(1), {
	stdio: 'inherit',
	cwd: pluginRoot,
});
process.exit(result.status ?? 0);