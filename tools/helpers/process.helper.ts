#!/usr/bin/env bun
/**
 * tools/helpers/process.helper.ts
 *
 * Single wrapper around `node:child_process`. The rest of the tool layer
 * imports `run()` and `runSync()` so swapping the implementation (for
 * example, to add a telemetry hook) is a one-line change.
 */

import { spawn, spawnSync } from 'node:child_process';

import type { IRunOptions, IRunResult } from '../contracts/interfaces/run.interface.ts';

const captureResult = (
	exitCode: number | null,
	stdout: string,
	stderr: string,
): IRunResult => ({
	exitCode: exitCode ?? -1,
	stdout,
	stderr,
	succeeded: (exitCode ?? 1) === 0,
});

/** Run a child process, optionally inheriting stdio, with a strict timeout. */
export const run = (cmd: string, options: IRunOptions = {}): Promise<IRunResult> => {
	return new Promise((resolveFn, rejectFn) => {
		const args = options.args ? [...options.args] : [];
		const stdio = options.inherit ? 'inherit' : 'pipe';
		const env = { ...process.env, ...(options.env ?? {}) };

		const child = spawn(cmd, args, {
			cwd: options.cwd,
			env,
			stdio,
		});

		let stdout = '';
		let stderr = '';
		if (!options.inherit) {
			child.stdout?.on('data', (chunk: Buffer) => {
				stdout += chunk.toString();
			});
			child.stderr?.on('data', (chunk: Buffer) => {
				stderr += chunk.toString();
			});
		}

		let timeoutHandle: NodeJS.Timeout | undefined;
		if (options.timeoutMs && options.timeoutMs > 0) {
			timeoutHandle = setTimeout(() => {
				child.kill('SIGTERM');
			}, options.timeoutMs);
		}

		child.on('error', (err) => {
			if (timeoutHandle !== undefined) clearTimeout(timeoutHandle);
			rejectFn(err);
		});
		child.on('close', (code) => {
			if (timeoutHandle !== undefined) clearTimeout(timeoutHandle);
			resolveFn(captureResult(code, stdout, stderr));
		});
	});
};

/** Synchronous variant — useful for CLI scripts that shell out to dotnet test. */
export const runSync = (cmd: string, args: readonly string[], options: IRunOptions = {}): IRunResult => {
	const result = spawnSync(cmd, [...args], {
		cwd: options.cwd,
		env: { ...process.env, ...(options.env ?? {}) },
		encoding: 'utf8',
		stdio: options.inherit ? 'inherit' : 'pipe',
		timeout: options.timeoutMs,
	});
	return captureResult(result.status, result.stdout ?? '', result.stderr ?? '');
};

/**
 * Stream `chunk` to both `stdout` and the FD so the helper can both
 * surface output and capture it for post-run grepping.
 */
export const teeAndCapture = (fd: number, chunk: Buffer | string): void => {
	const text = typeof chunk === 'string' ? chunk : chunk.toString();
	process.stdout.write(text);
	Bun.write(fd as unknown as string, text);
};
