#!/usr/bin/env bun
/**
 * Build every plugin in the workspace against the local mcp-vertex
 * checkout. Equivalent to running `bun run build` from inside the
 * mcp-vertex repo, but only for the plugins we own locally
 * (`plugins/player-club/` today; new plugins pick up automatically).
 *
 * Usage:
 *   bun scripts/build.script.ts
 */

import { readdirSync, existsSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

const workspaceRoot = resolve(import.meta.dirname, '..');
const pluginsDir = join(workspaceRoot, 'plugins');

if (!existsSync(pluginsDir)) {
	process.stdout.write('No `plugins/` directory; nothing to build.\n');
	process.exit(0);
}

const plugins = readdirSync(pluginsDir, { withFileTypes: true })
	.filter((entry) => entry.isDirectory())
	.map((entry) => entry.name);

if (plugins.length === 0) {
	process.stdout.write('No plugins found under `plugins/`; nothing to build.\n');
	process.exit(0);
}

let failed = 0;
for (const name of plugins) {
	process.stdout.write(`\n--- building ${name} ---\n`);
	const result = spawnSync('bun', ['scripts/build-plugin.script.ts', name], {
		stdio: 'inherit',
		cwd: workspaceRoot,
	});
	if ((result.status ?? 0) !== 0) failed += 1;
}

if (failed > 0) {
	process.stderr.write(`\n${failed} plugin(s) failed to build.\n`);
	process.exit(1);
}
process.stdout.write(`\nAll ${plugins.length} plugin(s) built successfully.\n`);