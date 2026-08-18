#!/usr/bin/env bun
/**
 * Dev entrypoint — runs the local mcp-vertex host in stdio mode with
 * the workspace's plugin set. This is the same command that
 * `.vscode/mcp.json` launches under the hood, but reachable from the
 * terminal for debugging (stdio protocol smoke-tests, plugin load
 * diagnostics, manual JSON-RPC pokes).
 *
 * Usage:
 *   bun scripts/dev.script.ts             # stdio MCP server, blocks
 *   bun scripts/dev.script.ts --doctor    # dry-run: validate + print loaded surface
 */

import { spawnSync } from 'node:child_process';
import { resolve } from 'node:path';

const args = process.argv.slice(2);
const isDoctor = args.includes('--doctor');
const passthroughArgs = args.filter((arg) => arg !== '--doctor');

const workspaceRoot = resolve(import.meta.dirname, '..');
const rootScript = resolve(import.meta.dirname, 'mcp-vertex-root.script.ts');
const rootResult = spawnSync('bun', [rootScript], { encoding: 'utf8' });
if (rootResult.status !== 0) {
	process.stderr.write(rootResult.stderr);
	process.exit(rootResult.status ?? 1);
}
const mcpVertexRoot = rootResult.stdout.trim();
const entryScript = isDoctor
	? `${mcpVertexRoot}/packages/cli/src/index.ts`
	: `${mcpVertexRoot}/tools/scripts/host/host-server.script.ts`;

const cmdArgs = [
	entryScript,
	`--workspace=${workspaceRoot}`,
	`--config=${workspaceRoot}/mcp-vertex.config.json`,
	'--preset=swarm',
	'--exclude-plugins=issues,web-fetch,external-mcps',
	...passthroughArgs,
];

if (isDoctor) cmdArgs.push('doctor');

const result = spawnSync('bun', cmdArgs, { stdio: 'inherit', cwd: workspaceRoot });
process.exit(result.status ?? 0);
