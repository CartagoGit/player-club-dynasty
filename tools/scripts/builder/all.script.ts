#!/usr/bin/env bun
/**
 * tools/scripts/builder/all.script.ts
 *
 * Build every plugin in the workspace against the local delendai
 * checkout. Iterates `plugins/` and delegates to `plugin.script.ts`.
 *
 * Usage:
 *   bun tools/scripts/builder/all.script.ts
 */

import { readdirSync, existsSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

const workspaceRoot = resolve(import.meta.dirname, '..', '..', '..');
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
	const result = spawnSync('bun', ['tools/scripts/builder/plugin.script.ts', name], {
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
