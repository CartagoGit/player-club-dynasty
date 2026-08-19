#!/usr/bin/env bun
/**
 * tools/helpers/system/lock.helper.ts
 *
 * Single-launch lock used by `play.script.ts`. A PID file is preferred
 * over `flock()` because it survives across reboots and exposes the
 * owning PID for telemetry. The helper is idempotent and reclaims the
 * lock when the previous owner is dead.
 */

import { closeSync, existsSync, mkdirSync, openSync, readFileSync, unlinkSync, writeSync } from 'node:fs';
import { dirname, join } from 'node:path';

import { XDG_RUNTIME_DIR_ENV_VAR } from '../../contracts/constants/system/env.constants.ts';
import { PCDYNASTY_GAME_LOCK_FILE } from '../../contracts/constants/engine/godot.constants.ts';
import type { ILockHandle, LockAcquired } from '../../contracts/interfaces/system/lock.interface.ts';

const isPidAlive = (pid: number): boolean => {
	if (!Number.isInteger(pid) || pid <= 0) return false;
	try {
		process.kill(pid, 0);
		return true;
	} catch {
		return false;
	}
};

/** Default lock path under `${XDG_RUNTIME_DIR:-/tmp}/pcdynasty-game.lock`. */
export const defaultLockPath = (): string => {
	const runtimeDir = process.env[XDG_RUNTIME_DIR_ENV_VAR] ?? '/tmp';
	return join(runtimeDir, PCDYNASTY_GAME_LOCK_FILE);
};

/** Try to acquire the lock. Returns a handle, or `false` if another live process holds it. */
export const acquireLock = (lockPath: string = defaultLockPath()): LockAcquired => {
	mkdirSync(dirname(lockPath), { recursive: true });

	try {
		const fd = openSync(lockPath, 'wx');
		writeSync(fd, `${process.pid}\n${Date.now()}\n`);
		closeSync(fd);
		return makeHandle(lockPath);
	} catch (err) {
		if ((err as NodeJS.ErrnoException).code !== 'EEXIST') return false;
		if (!isLockOwnerAlive(lockPath)) {
			// Stale lock — take it over.
			try {
				unlinkSync(lockPath);
			} catch {
				// Race lost; another process grabbed it first.
				return false;
			}
			return acquireLock(lockPath);
		}
		return false;
	}
};

const isLockOwnerAlive = (lockPath: string): boolean => {
	if (!existsSync(lockPath)) return false;
	try {
		const content = readFileSync(lockPath, 'utf8');
		const pid = parseInt(content.split('\n')[0] ?? '', 10);
		return isPidAlive(pid);
	} catch {
		return false;
	}
};

const makeHandle = (lockPath: string): ILockHandle => {
	let released = false;
	return {
		path: lockPath,
		release(): void {
			if (released) return;
			released = true;
			try {
				if (existsSync(lockPath)) {
					const content = readFileSync(lockPath, 'utf8');
					const pid = parseInt(content.split('\n')[0] ?? '', 10);
					if (pid === process.pid) unlinkSync(lockPath);
				}
			} catch {
				// Best-effort.
			}
		},
	};
};
