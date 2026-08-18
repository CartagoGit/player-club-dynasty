#!/usr/bin/env bun
/**
 * Symlink the local mcp-vertex checkout into `node_modules/@mcp-vertex`.
 *
 * The workspace consumes `@mcp-vertex/core` from a sibling checkout rather
 * than from a registry (repo rule 1). Expressing that as a manifest
 * dependency forces a choice between two broken options: `link:<absolute
 * path>` hard-codes one machine, and `file:<relative path>` makes bun copy
 * the package, so edits in the checkout stop reaching this workspace.
 *
 * This script keeps the live symlink and derives its target at install time
 * from the layered discovery in `mcp-vertex-root.script.ts`, so a fresh clone
 * on any machine links itself as long as the checkout can be found.
 *
 * Runs from `postinstall`; safe to run at any time.
 */

import { existsSync, lstatSync, mkdirSync, realpathSync, rmSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';

import { resolveMcpVertexRoot, workspaceRoot } from './mcp-vertex-root.script.ts';

const SCOPE_DIRECTORY = join(workspaceRoot, 'node_modules', '@mcp-vertex');
const LINKED_PACKAGES = ['core'] as const;

let root: string;
try {
	root = resolveMcpVertexRoot().path;
} catch (error) {
	// A missing checkout must not fail `bun install` for someone who only
	// wants to build the game; the mcp-vertex scripts report it themselves.
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
