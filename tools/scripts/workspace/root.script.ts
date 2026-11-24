#!/usr/bin/env bun
/**
 * tools/scripts/workspace/root.script.ts
 *
 * Resolve the local delendai checkout this workspace bootstraps against.
 * Moved from `scripts/delendai-root.script.ts` so the workspace follows
 * the new `tools/scripts/<area>/<verb>.script.ts` convention.
 *
 * Resolution order:
 *   1. `$DELENDAI_ROOT`
 *   2. `$DELENDAI_PATH`
 *   3. `<workspace>/.delendai-root` (optional, untracked, per machine)
 *   4. `<workspace>/.cache/delendai-runtime` (gitignored link, see `ensureRuntimeLink`)
 *   5. `<workspace>/node_modules/@delendai/core` (through the symlink)
 *   6. The stable install `~/.local/share/delendai/stable`
 *   7. A `delendai` sibling of the workspace and each ancestor
 *   8. `$HOME/delendai` and `$HOME/_projects/delendai`
 *
 * No tracked file names a machine path: they only name the link
 * `.cache/delendai-runtime`, which `ensureRuntimeLink` points at
 * `$DELENDAI_PATH` or the stable install on every machine.
 *
 * Usage:
 *   bun tools/scripts/workspace/root.script.ts                  # absolute path
 *   bun tools/scripts/workspace/root.script.ts --check          # exit 1 if missing
 *   bun tools/scripts/workspace/root.script.ts --host-script    # path of host-server.script.ts
 *   bun tools/scripts/workspace/root.script.ts --verbose
 */

import {
	existsSync,
	lstatSync,
	mkdirSync,
	readFileSync,
	readlinkSync,
	realpathSync,
	rmSync,
	symlinkSync,
} from 'node:fs';
import { homedir } from 'node:os';
import { dirname, isAbsolute, join, parse, resolve } from 'node:path';

export const SENTINEL_HOST_SCRIPT = 'tools/scripts/host/host-server.script.ts';
const SENTINEL_CORE_PKG = 'packages/core/package.json';
const POINTER_FILE = '.delendai-root';
const CHECKOUT_DIRECTORY_NAME = 'delendai';
export const RUNTIME_LINK = join('.cache', 'delendai-runtime');
const LINKED_CORE = join('node_modules', '@delendai', 'core');

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

/** The install this machine should link to: `$DELENDAI_PATH`, else the stable channel. */
export const runtimeTarget = (home: string = homedir()): string =>
	process.env.DELENDAI_PATH?.trim() || join(home, '.local', 'share', 'delendai', 'stable');

/**
 * Create or refresh the gitignored link `.cache/delendai-runtime` so the
 * committed files can reference it instead of a machine path. Throws a
 * clear error when the target is not a delendai install.
 */
export const ensureRuntimeLink = (
	workspace: string = workspaceRoot,
	target: string = runtimeTarget(),
): string => {
	if (!isCheckout(target)) {
		throw new Error(
			`delendai not found at ${target}. Set DELENDAI_PATH or install the stable channel at ~/.local/share/delendai/stable.`,
		);
	}
	const link = join(workspace, RUNTIME_LINK);
	const current = lstatSync(link, { throwIfNoEntry: false });
	if (current?.isSymbolicLink() && resolve(dirname(link), readlinkSync(link)) === resolve(target)) {
		return link;
	}
	if (current !== undefined) rmSync(link, { recursive: true, force: true });
	mkdirSync(dirname(link), { recursive: true });
	symlinkSync(target, link, 'dir');
	return link;
};

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

const candidates = (workspace: string, home: string): readonly ICandidate[] => {
	const fromEnv = process.env.DELENDAI_ROOT?.trim();
	const fromPath = process.env.DELENDAI_PATH?.trim();
	const pointer = readPointer(workspace);
	const siblings = ancestors(workspace).map((ancestor) => ({
		source: `sibling checkout in ${ancestor}`,
		path: join(ancestor, CHECKOUT_DIRECTORY_NAME),
		explicit: false,
	}));

	return [
		{
			source: 'DELENDAI_ROOT env var',
			path: fromEnv ? expandPath(fromEnv, workspace) : undefined,
			explicit: true,
		},
		{
			source: 'DELENDAI_PATH env var',
			path: fromPath ? expandPath(fromPath, workspace) : undefined,
			explicit: true,
		},
		{
			source: `${POINTER_FILE} pointer file`,
			path: pointer ? expandPath(pointer, workspace) : undefined,
			explicit: true,
		},
		{
			source: `${RUNTIME_LINK} link`,
			path: join(workspace, RUNTIME_LINK),
			explicit: false,
		},
		{
			source: 'linked @delendai/core package',
			path: fromLinkedPackage(workspace),
			explicit: false,
		},
		{
			source: 'stable install',
			path: runtimeTarget(home),
			explicit: false,
		},
		...siblings,
		{
			source: 'home directory',
			path: join(home, CHECKOUT_DIRECTORY_NAME),
			explicit: false,
		},
		{
			source: 'home projects directory',
			path: join(home, '_projects', CHECKOUT_DIRECTORY_NAME),
			explicit: false,
		},
	];
};

export const resolveRoot = (
	workspace: string = workspaceRoot,
	home: string = homedir(),
): IResolvedRoot | null => {
	for (const candidate of candidates(workspace, home)) {
		if (candidate.path === undefined) continue;
		if (candidate.explicit || isCheckout(candidate.path)) {
			return { source: candidate.source, path: candidate.path };
		}
	}
	return null;
};

export const resolveDelendaiRoot = (workspace: string = workspaceRoot): IResolvedRoot => {
	const resolved = resolveRoot(workspace);
	if (resolved === null) {
		throw new Error(
			`Could not locate the local delendai checkout. Clone it next to this workspace, set DELENDAI_ROOT, or write its path into ${POINTER_FILE} (relative paths are resolved against the workspace).`,
		);
	}
	const check = smokeCheck(resolved.path);
	if (!check.ok) {
		throw new Error(
			`delendai checkout at ${resolved.path} (from ${resolved.source}) is missing: ${check.missing.join(', ')}`,
		);
	}
	return resolved;
};

if (import.meta.main) {
	const args = new Set(process.argv.slice(2));
	try {
		const resolved = resolveDelendaiRoot();
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
