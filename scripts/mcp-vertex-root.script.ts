#!/usr/bin/env bun
/**
 * Resolve the local mcp-vertex checkout this workspace bootstraps against.
 *
 * The checkout lives outside the repository, so its location cannot be
 * committed as one machine's absolute path — that is exactly what made the
 * workspace unusable on a second computer. Resolution is therefore layered,
 * from the most explicit source to the most conventional, and every candidate
 * must pass a smoke check before it is accepted:
 *
 *   1. `$MCP_VERTEX_ROOT` — the canonical override (absolute, `~`-relative or
 *      relative to this workspace).
 *   2. `<workspace>/.mcp-vertex-root` — the committed pointer file, which may
 *      hold a relative path so the same value works on every machine that
 *      keeps the two repositories side by side.
 *   3. The linked package: `node_modules/@mcp-vertex/core` resolves through
 *      its symlink to `<checkout>/packages/core`.
 *   4. A `mcp-vertex` sibling in this workspace's directory or in any of its
 *      ancestors.
 *   5. `~/mcp-vertex` and `~/_projects/mcp-vertex`.
 *
 * Usage:
 *   bun scripts/mcp-vertex-root.script.ts                 # prints the absolute path
 *   bun scripts/mcp-vertex-root.script.ts --check         # exits 1 if missing
 *   bun scripts/mcp-vertex-root.script.ts --host-script   # prints host-server.script.ts
 *   bun scripts/mcp-vertex-root.script.ts --verbose       # also prints the source
 */

import { existsSync, readFileSync, realpathSync } from 'node:fs';
import { homedir } from 'node:os';
import { dirname, isAbsolute, join, parse, resolve } from 'node:path';

export const SENTINEL_HOST_SCRIPT = 'tools/scripts/host/host-server.script.ts';
const SENTINEL_CORE_PKG = 'packages/core/package.json';
const POINTER_FILE = '.mcp-vertex-root';
const CHECKOUT_DIRECTORY_NAME = 'mcp-vertex';
const LINKED_CORE = join('node_modules', '@mcp-vertex', 'core');

/** Absolute path of this workspace (the directory holding `scripts/`). */
export const workspaceRoot = resolve(import.meta.dirname, '..');

export interface IResolvedRoot {
	readonly source: string;
	readonly path: string;
}

/** Expand a leading `~` and resolve anything relative against `base`. */
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

/** `<workspace>` and every ancestor, nearest first. */
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

/** The checkout the linked `@mcp-vertex/core` package points at, if any. */
const fromLinkedPackage = (workspace: string): string | undefined => {
	const linked = join(workspace, LINKED_CORE);
	if (!existsSync(linked)) return undefined;
	try {
		// `<checkout>/packages/core` → `<checkout>`.
		return dirname(dirname(realpathSync(linked)));
	} catch {
		return undefined;
	}
};

interface ICandidate {
	readonly source: string;
	readonly path: string | undefined;
	/** An explicit source is honoured even when it fails the smoke check. */
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

/**
 * Locate the checkout. An explicit source (env var, pointer file) wins even
 * when it is broken: the caller then reports *that* path as the problem
 * instead of silently bootstrapping an unrelated checkout that happens to sit
 * next to the workspace.
 */
export const resolveRoot = (workspace: string = workspaceRoot): IResolvedRoot | null => {
	for (const candidate of candidates(workspace)) {
		if (candidate.path === undefined) continue;
		if (candidate.explicit || isCheckout(candidate.path)) {
			return { source: candidate.source, path: candidate.path };
		}
	}
	return null;
};

/**
 * Resolve **and** validate the checkout. Throws with an actionable message so
 * callers never bootstrap a stale or missing tree by accident.
 */
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

// CLI ------------------------------------------------------------------------
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
