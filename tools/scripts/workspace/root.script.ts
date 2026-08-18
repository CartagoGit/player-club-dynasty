#!/usr/bin/env bun
/**
 * tools/scripts/workspace/root.script.ts
 *
 * Resolve the local mcp-vertex checkout this workspace bootstraps against.
 * Moved from `scripts/mcp-vertex-root.script.ts` so the workspace follows
 * the new `tools/scripts/<area>/<verb>.script.ts` convention.
 *
 * Resolution order:
 *   1. `$MCP_VERTEX_ROOT`
 *   2. `<workspace>/.mcp-vertex-root`
 *   3. `<workspace>/node_modules/@mcp-vertex/core` (through the symlink)
 *   4. A `mcp-vertex` sibling of the workspace and each ancestor
 *   5. `$HOME/mcp-vertex` and `$HOME/_projects/mcp-vertex`
 *
 * Usage:
 *   bun tools/scripts/workspace/root.script.ts                  # absolute path
 *   bun tools/scripts/workspace/root.script.ts --check          # exit 1 if missing
 *   bun tools/scripts/workspace/root.script.ts --host-script    # path of host-server.script.ts
 *   bun tools/scripts/workspace/root.script.ts --verbose
 */

import { existsSync, readFileSync, realpathSync } from 'node:fs';
import { homedir } from 'node:os';
import { dirname, isAbsolute, join, parse, resolve } from 'node:path';

export const SENTINEL_HOST_SCRIPT = 'tools/scripts/host/host-server.script.ts';
const SENTINEL_CORE_PKG = 'packages/core/package.json';
const POINTER_FILE = '.mcp-vertex-root';
const CHECKOUT_DIRECTORY_NAME = 'mcp-vertex';
const LINKED_CORE = join('node_modules', '@mcp-vertex', 'core');

export const workspaceRoot = resolve(import.meta.dirname, '..', '..', '..');

export interface IResolvedRoot {
	readonly source: string;
	readonly path: string;
}

export const expandPath = (value: string, base: string): string => {
	const trimmed = value.trim();
	if (trimmed.startsWith('~')) return join(homedir(), trimmed.slice(1));
	return isAbsolute(trimmed) ? trimmed : resolve(base, trimmed);
};

export const smokeCheck = (path: string): { ok: boolean; missing: string[] } => {
	const missing: string[] = [];
	if (!existsSync(join(path, SENTINEL_HOST_SCRIPT))) missing.push(SENTINEL_HOST_SCRIPT);
	if (!existsSync(join(path, SENTINEL_CORE_PKG))) missing.push(SENTINEL_CORE_PKG);
	return { ok: missing.length === 0, missing };
};

const isCheckout = (path: string): boolean => smokeCheck(path).ok;

const readPointer = (workspace: string): string | undefined => {
	const pointerPath = join(workspace, POINTER_FILE);
	if (!existsSync(pointerPath)) return undefined;
	const trimmed = readFileSync(pointerPath, 'utf8').trim();
	return trimmed.length > 0 ? trimmed : undefined;
};

const ancestors = (workspace: string): readonly string[] => {
	const out: string[] = [];
	let current = workspace;
	const { root } = parse(current);
	while (true) {
		out.push(current);
		if (current === root) break;
		current = dirname(current);
	}
	return out;
};

const fromLinkedPackage = (workspace: string): string | undefined => {
	const linked = join(workspace, LINKED_CORE);
	if (!existsSync(linked)) return undefined;
	try {
		return dirname(dirname(realpathSync(linked)));
	} catch {
		return undefined;
	}
};

interface ICandidate {
	readonly source: string;
	readonly path: string | undefined;
	readonly explicit: boolean;
}

const candidates = (workspace: string): readonly ICandidate[] => {
	const fromEnv = process.env.MCP_VERTEX_ROOT?.trim();
	const pointer = readPointer(workspace);
	const siblings = ancestors(workspace).map((ancestor) => ({
		source: `sibling checkout in ${ancestor}`,
		path: join(ancestor, CHECKOUT_DIRECTORY_NAME),
		explicit: false,
	}));

	return [
		{
			source: 'MCP_VERTEX_ROOT env var',
			path: fromEnv ? expandPath(fromEnv, workspace) : undefined,
			explicit: true,
		},
		{
			source: `${POINTER_FILE} pointer file`,
			path: pointer ? expandPath(pointer, workspace) : undefined,
			explicit: true,
		},
		{
			source: 'linked @mcp-vertex/core package',
			path: fromLinkedPackage(workspace),
			explicit: false,
		},
		...siblings,
		{
			source: 'home directory',
			path: join(homedir(), CHECKOUT_DIRECTORY_NAME),
			explicit: false,
		},
		{
			source: 'home projects directory',
			path: join(homedir(), '_projects', CHECKOUT_DIRECTORY_NAME),
			explicit: false,
		},
	];
};

export const resolveRoot = (workspace: string = workspaceRoot): IResolvedRoot | null => {
	for (const candidate of candidates(workspace)) {
		if (candidate.path === undefined) continue;
		if (candidate.explicit || isCheckout(candidate.path)) {
			return { source: candidate.source, path: candidate.path };
		}
	}
	return null;
};

export const resolveMcpVertexRoot = (workspace: string = workspaceRoot): IResolvedRoot => {
	const resolved = resolveRoot(workspace);
	if (resolved === null) {
		throw new Error(
			`Could not locate the local mcp-vertex checkout. Clone it next to this workspace, set MCP_VERTEX_ROOT, or write its path into ${POINTER_FILE} (relative paths are resolved against the workspace).`,
		);
	}
	const check = smokeCheck(resolved.path);
	if (!check.ok) {
		throw new Error(
			`mcp-vertex checkout at ${resolved.path} (from ${resolved.source}) is missing: ${check.missing.join(', ')}`,
		);
	}
	return resolved;
};

if (import.meta.main) {
	const args = new Set(process.argv.slice(2));
	try {
		const resolved = resolveMcpVertexRoot();
		const value = args.has('--host-script')
			? join(resolved.path, SENTINEL_HOST_SCRIPT)
			: resolved.path;
		process.stdout.write(
			args.has('--verbose') ? `${value} (${resolved.source})\n` : `${value}\n`,
		);
	} catch (error) {
		process.stderr.write(`${(error as Error).message}\n`);
		process.exit(args.has('--check') ? 1 : 2);
	}
}
