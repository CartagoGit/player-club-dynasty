#!/usr/bin/env bun
/**
 * tools/scripts/mcp-vertex/link.script.ts
 *
 * Symlink the local mcp-vertex checkout into `node_modules/@mcp-vertex`.
 * Runs from `postinstall`; safe to invoke any time.
 */

import { existsSync, lstatSync, mkdirSync, realpathSync, rmSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';

import { resolveMcpVertexRoot, workspaceRoot } from './root.script.ts';

const SCOPE_DIRECTORY = join(workspaceRoot, 'node_modules', '@mcp-vertex');
const LINKED_PACKAGES = ['core'] as const;

let root: string;
try {
	root = resolveMcpVertexRoot().path;
} catch (error) {
	process.stderr.write(`link-mcp-vertex: ${(error as Error).message}\n`);
	process.exit(0);
}

mkdirSync(SCOPE_DIRECTORY, { recursive: true });

for (const packageName of LINKED_PACKAGES) {
	const target = join(root, 'packages', packageName);
	const linkPath = join(SCOPE_DIRECTORY, packageName);

	if (!existsSync(target)) {
		process.stderr.write(`link-mcp-vertex: ${target} does not exist; skipped.\n`);
		continue;
	}

	if (existsSync(linkPath) || lstatSync(linkPath, { throwIfNoEntry: false }) !== undefined) {
		const alreadyCorrect =
			lstatSync(linkPath).isSymbolicLink() && realpathSync(linkPath) === realpathSync(target);
		if (alreadyCorrect) continue;
		rmSync(linkPath, { recursive: true, force: true });
	}

	symlinkSync(target, linkPath, 'dir');
	process.stdout.write(`link-mcp-vertex: @mcp-vertex/${packageName} -> ${target}\n`);
}
