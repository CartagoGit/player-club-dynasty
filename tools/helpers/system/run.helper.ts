#!/usr/bin/env bun
/**
 * tools/helpers/system/run.helper.ts
 *
 * High-level helpers for the workflows scripts use most often:
 *   - runInherit: prints child output to the terminal and returns the
 *     exit code (the smoke script uses it for `--import` and `dotnet
 *     build`).
 *   - runTee: streams the child output to a tee'd file descriptor so it
 *     can be grepped for engine errors after the child exits.
 *   - failOnPattern: returns the exit code plus a boolean that tells
 *     the caller whether the captured log matched a regex.
 */

import { closeSync, openSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

import { spawn } from 'node:child_process';

import type { IRunResult } from '../../contracts/interfaces/system/run.interface.ts';
import { run, teeAndCapture } from './process.helper.ts';

/** Run a child whose stdio is forwarded to the parent TTY. */
export const runInherit = async (cmd: string, args: readonly string[], opts: { cwd?: string; env?: Readonly<Record<string, string>> } = {}): Promise<number> => {
	const result = await run(cmd, { args, cwd: opts.cwd, env: opts.env, inherit: true });
	return result.exitCode;
};

/** Run a child whose stdout/stderr are streamed to the terminal AND a temp file. */
export const runTee = async (
	cmd: string,
	args: readonly string[],
	opts: { cwd?: string; env?: Readonly<Record<string, string>>; logPath?: string } = {},
): Promise<{ exitCode: number; logPath: string }> => {
	const logPath = opts.logPath ?? join(tmpdir(), `pcdynasty-run-${Date.now()}.log`);
	const fd = openSync(logPath, 'w');
	const child = spawn(cmd, [...args], {
		cwd: opts.cwd,
		env: { ...process.env, ...(opts.env ?? {}) },
		stdio: ['inherit', 'pipe', 'pipe'],
	});
	child.stdout.on('data', (chunk) => teeAndCapture(fd, chunk));
	child.stderr.on('data', (chunk) => teeAndCapture(fd, chunk));
	const exitCode = (await new Promise<number>((resolveFn) => {
		child.on('close', (code) => resolveFn(code ?? -1));
	})) as number;
	closeSync(fd);
	return { exitCode, logPath };
};

/** Read a tee'd log and report whether it matches a regex. */
export const logMatches = (logPath: string, regex: RegExp): boolean => {
	const content = readFileSync(logPath, 'utf8');
	return regex.test(content);
};

/** Free a tee'd log after the caller is done with it. */
export const disposeLog = (logPath: string): void => {
	rmSync(logPath, { force: true });
};

/** Re-export the lower-level helpers so callers only need this file. */
export { run, runSync } from './process.helper.ts';
export type { IRunResult };
