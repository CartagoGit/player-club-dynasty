#!/usr/bin/env bun
/**
 * Run a script that lives inside the local mcp-vertex checkout, without any
 * workspace file naming a machine-specific absolute path.
 *
 * `package.json` used to invoke `/home/cartago/_projects/mcp-vertex/...`
 * directly, so every script broke the moment the repository was cloned on a
 * different computer. This thin runner resolves the checkout with the layered
 * discovery in `mcp-vertex-root.script.ts` and forwards the remaining
 * arguments unchanged.
 *
 * Usage:
 *   bun scripts/mcp-vertex-run.script.ts tools/scripts/lint/proposals.script.ts [args...]
 */

import { spawnSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';

import { resolveMcpVertexRoot, workspaceRoot } from './mcp-vertex-root.script.ts';

const [relativeScript, ...forwarded] = process.argv.slice(2);

if (relativeScript === undefined || relativeScript.length === 0) {
	process.stderr.write(
		'mcp-vertex-run: expected a checkout-relative script path as the first argument.\n',
	);
	process.exit(2);
}

let root: string;
try {
	root = resolveMcpVertexRoot().path;
} catch (error) {
	process.stderr.write(`${(error as Error).message}\n`);
	process.exit(2);
}

const target = join(root, relativeScript);
if (!existsSync(target)) {
	process.stderr.write(`mcp-vertex-run: ${relativeScript} does not exist in ${root}.\n`);
	process.exit(2);
}

const result = spawnSync(process.execPath, [target, ...forwarded], {
	cwd: workspaceRoot,
	stdio: 'inherit',
});

process.exit(result.status ?? 1);
