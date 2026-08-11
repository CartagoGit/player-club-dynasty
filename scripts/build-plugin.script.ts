#!/usr/bin/env bun
/**
 * Build a single plugin's `dist/` using the local mcp-vertex compile
 * pipeline. Equivalent to running `bun tools/scripts/compile/build.script.ts
 * plugins/<name>` from inside the mcp-vertex checkout, but resolved
 * dynamically against whichever checkout this workspace bootstraps against.
 *
 * Usage:
 *   bun scripts/build-plugin.script.ts player-club
 *   bun scripts/build-plugin.script.ts player-club --watch
 */

import { spawnSync } from 'node:child_process';
import { resolve } from 'node:path';

const rootScript = resolve(import.meta.dirname, 'mcp-vertex-root.script.ts');
const rootResult = spawnSync('bun', [rootScript], { encoding: 'utf8' });
if (rootResult.status !== 0) {
	process.stderr.write(rootResult.stderr);
	process.exit(rootResult.status ?? 1);
}
const mcpVertexRoot = rootResult.stdout.trim();

const args = process.argv.slice(2);
const pluginName = args[0];
if (!pluginName) {
	process.stderr.write('Usage: bun scripts/build-plugin.script.ts <plugin-name> [--watch]\n');
	process.exit(2);
}

const buildScript = `${mcpVertexRoot}/tools/scripts/compile/build.script.ts`;
const compileArgs = ['bun', buildScript, `plugins/${pluginName}`, ...args.slice(1)];

const result = spawnSync(compileArgs[0]!, compileArgs.slice(1), {
	stdio: 'inherit',
	cwd: mcpVertexRoot,
});
process.exit(result.status ?? 0);