#!/usr/bin/env bun
/**
 * Resolve the local mcp-vertex checkout that the workspace bootstraps against.
 *
 * Order of resolution:
 *   1. `$MCP_VERTEX_ROOT` env var (canonical override).
 *   2. `<this-workspace>/.mcp-vertex-root` pointer file (committed for
 *      per-developer portability — can be regenerated).
 *   3. The default `/home/cartago/_projects/mcp-vertex` sibling.
 *
 * Each step is followed by a smoke check: the resolved path must contain
 * `tools/scripts/host/host-server.script.ts` and `packages/core/package.json`.
 * If either check fails, the script throws — better to fail loud than to
 * silently bootstrap a stale checkout.
 *
 * Usage:
 *   bun scripts/mcp-vertex-root.script.ts                 # prints the absolute path
 *   bun scripts/mcp-vertex-root.script.ts --check         # exits 1 if missing
 *   bun scripts/mcp-vertex-root.script.ts --host-script   # prints host-server.script.ts
 */

import { existsSync, readFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

const SENTINEL_HOST_SCRIPT = 'tools/scripts/host/host-server.script.ts';
const SENTINEL_CORE_PKG = 'packages/core/package.json';
const POINTER_FILE = '.mcp-vertex-root';

const args = new Set(process.argv.slice(2));
const wantCheck = args.has('--check');
const wantHostScript = args.has('--host-script');

const workspaceRoot = resolve(import.meta.dirname, '..');

function readPointer(): string | undefined {
	const pointerPath = join(workspaceRoot, POINTER_FILE);
	if (!existsSync(pointerPath)) return undefined;
	const trimmed = readFileSync(pointerPath, 'utf8').trim();
	return trimmed.length > 0 ? trimmed : undefined;
}

function resolveRoot(): { source: string; path: string } | null {
	const fromEnv = process.env.MCP_VERTEX_ROOT?.trim();
	if (fromEnv) return { source: 'MCP_VERTEX_ROOT env var', path: fromEnv };

	const fromPointer = readPointer();
	if (fromPointer)
		return { source: `${POINTER_FILE} pointer file`, path: fromPointer };

	const fromDefault = '/home/cartago/_projects/mcp-vertex';
	if (existsSync(join(fromDefault, SENTINEL_HOST_SCRIPT))) {
		return { source: 'default sibling', path: fromDefault };
	}
	return null;
}

function smokeCheck(path: string): { ok: boolean; missing: string[] } {
	const missing: string[] = [];
	if (!existsSync(join(path, SENTINEL_HOST_SCRIPT)))
		missing.push(SENTINEL_HOST_SCRIPT);
	if (!existsSync(join(path, SENTINEL_CORE_PKG))) missing.push(SENTINEL_CORE_PKG);
	return { ok: missing.length === 0, missing };
}

const resolved = resolveRoot();
if (!resolved) {
	const message = `Could not locate the local mcp-vertex checkout. Set MCP_VERTEX_ROOT or create ${POINTER_FILE}.`;
	process.stderr.write(`${message}\n`);
	process.exit(wantCheck ? 1 : 2);
}

const check = smokeCheck(resolved.path);
if (!check.ok) {
	const message = `mcp-vertex checkout at ${resolved.path} is missing: ${check.missing.join(', ')}`;
	process.stderr.write(`${message}\n`);
	process.exit(wantCheck ? 1 : 2);
}

if (wantHostScript) {
	process.stdout.write(`${join(resolved.path, SENTINEL_HOST_SCRIPT)}\n`);
} else {
	process.stdout.write(`${resolved.path}\n`);
}