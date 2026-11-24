#!/usr/bin/env bun
/**
 * tools/scripts/workspace/run.script.ts
 *
 * Run a script that lives inside the local delendai checkout, without
 * any workspace file naming a machine-specific absolute path.
 *
 * Usage:
 *   bun tools/scripts/workspace/run.script.ts tools/scripts/lint/proposals.script.ts [args...]
 */

import { spawnSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';

import { ensureRuntimeLink, resolveDelendaiRoot, workspaceRoot } from './root.script.ts';

const [relativeScript, ...forwarded] = process.argv.slice(2);

if (relativeScript === undefined || relativeScript.length === 0) {
	process.stderr.write(
		'delendai-run: expected a checkout-relative script path as the first argument.\n',
	);
	process.exit(2);
}

let root: string;
try {
	root = resolveDelendaiRoot().path;
} catch (error) {
	process.stderr.write(`${(error as Error).message}\n`);
	process.exit(2);
}

try {
	ensureRuntimeLink();
} catch (error) {
	process.stderr.write(`delendai-run: ${(error as Error).message}\n`);
}

const target = join(root, relativeScript);
if (!existsSync(target)) {
	process.stderr.write(`delendai-run: ${relativeScript} does not exist in ${root}.\n`);
	process.exit(2);
}

const result = spawnSync(process.execPath, [target, ...forwarded], {
	cwd: workspaceRoot,
	stdio: 'inherit',
});

process.exit(result.status ?? 1);
