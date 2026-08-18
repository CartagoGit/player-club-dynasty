#!/usr/bin/env bun
/**
 * tools/scripts/mcp-vertex/dev.script.ts
 *
 * Dev entrypoint — runs the local mcp-vertex host in stdio mode with the
 * workspace's plugin set. Pass `--doctor` to validate without serving.
 *
 * Usage:
 *   bun tools/scripts/mcp-vertex/dev.script.ts
 *   bun tools/scripts/mcp-vertex/dev.script.ts --doctor
 */

import { spawnSync } from 'node:child_process';
import { resolve } from 'node:path';

const args = process.argv.slice(2);
const isDoctor = args.includes('--doctor');
const passthroughArgs = args.filter((arg) => arg !== '--doctor');

const workspaceRoot = resolve(import.meta.dirname, '..', '..', '..');
const rootScript = resolve(import.meta.dirname, 'root.script.ts');
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
