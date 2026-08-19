#!/usr/bin/env bun
/**
 * tools/helpers/system/fs.helper.ts
 *
 * Filesystem primitives — `isExecutable`, `readText`, `writeFileAtomic`,
 * `mkdirp`. Pure functions of the OS API so the rest of the tool layer
 * never imports `node:fs` directly.
 */

import {
	existsSync,
	lstatSync,
	mkdirSync,
	readFileSync,
	realpathSync,
	statSync,
	unlinkSync,
	writeFileSync,
} from 'node:fs';
import { dirname, join, parse } from 'node:path';

import { FILE_EXECUTABLE_MODE, FILE_TEXT_MODE } from '../../contracts/constants/system/fs.constants.ts';

/** True when `path` is a regular file with at least one executable bit set. */
export const isExecutable = (path: string): boolean => {
	try {
		const stat = statSync(path);
		return stat.isFile() && (stat.mode & 0o111) !== 0;
	} catch {
		return false;
	}
};

/** Returns the target of a symlink or `undefined` when the link is broken. */
export const readSymlinkTarget = (path: string): string | undefined => {
	try {
		const lstat = lstatSync(path);
		if (!lstat.isSymbolicLink()) return undefined;
		return realpathSync(path);
	} catch {
		return undefined;
	}
};

/** Read a UTF-8 text file, throwing an Error with context on failure. */
export const readText = (path: string): string => {
	try {
		return readFileSync(path, 'utf8');
	} catch (err) {
		throw new Error(`readText failed for ${path}: ${(err as Error).message}`);
	}
};

/** Make a directory (and parents) without throwing when it already exists. */
export const mkdirp = (path: string): void => {
	mkdirSync(path, { recursive: true });
};

/** Atomically write `content` to `path`, ensuring the parent directory exists. */
export const writeFileAtomic = (path: string, content: string, mode = FILE_TEXT_MODE): void => {
	mkdirp(dirname(path));
	writeFileSync(path, content, { mode });
};

/** Set the executable bit on a file. Throws when the file does not exist. */
export const chmodExecutable = (path: string): void => {
	const stat = statSync(path);
	const mode = stat.mode | FILE_EXECUTABLE_MODE;
	writeFileSync(path, readFileSync(path), { mode });
};

/** Removes `path` if it exists; failures are swallowed (idempotent). */
export const removeIfExists = (path: string): void => {
	try {
		unlinkSync(path);
	} catch {
		// missing — fine
	}
};

/** `realpath` that returns `path` when the entry does not exist. */
export const realpathOrUndefined = (path: string): string | undefined => {
	try {
		return realpathSync(path);
	} catch {
		return undefined;
	}
};

/** Build a project-relative path (no absolute, no traversal). */
export const resolveWithin = (root: string, ...segments: string[]): string => {
	const base = parse(root).root === '' ? join(process.cwd(), root) : root;
	return join(base, ...segments);
};
