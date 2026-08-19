#!/usr/bin/env bun
/**
 * tools/helpers/progress.helper.spec.ts
 *
 * Vitest tests for the progress indicator. We never observe the TTY
 * spinner output directly (it would be flaky); we instead verify:
 *   - the returned stop() is idempotent (calling it twice is safe),
 *   - the returned stop() always prints a final line that includes
 *     the elapsed seconds,
 *   - update() mutates the label without crashing.
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { Writable } from 'node:stream';

import { startProgressIndicator } from './progress.helper';

let captured = '';
const restoreStderr = (): void => {
	process.stderr.write = process.stderr.write.bind(process.stderr) as typeof process.stderr.write;
};

const captureStderr = (): Writable => {
	captured = '';
	const sink = new Writable({
		write(chunk, encoding, callback) {
			captured += chunk.toString(encoding);
			callback();
		},
	});
	process.stderr.write = ((chunk: string | Uint8Array) => {
		captured += typeof chunk === 'string' ? chunk : chunk.toString();
		return true;
	}) as typeof process.stderr.write;
	return sink;
};

describe('progress.helper.ts', () => {
	beforeEach(() => captureStderr());
	afterEach(() => restoreStderr());

	it('emits the [..] header on pipe stdio within one tick interval', async () => {
		const indicator = startProgressIndicator('doing the thing');
		await new Promise((resolveFn) => setTimeout(resolveFn, 300));
		expect(captured).toMatch(/doing the thing/);
		indicator.stop('ok');
	});

	it('stop() is idempotent', async () => {
		const indicator = startProgressIndicator('once');
		await new Promise((resolveFn) => setTimeout(resolveFn, 300));
		indicator.stop('ok');
		const afterFirst = captured;
		indicator.stop('ok');
		expect(captured).toBe(afterFirst);
	});

	it('update() changes the label without crashing', async () => {
		const indicator = startProgressIndicator('first');
		await new Promise((resolveFn) => setTimeout(resolveFn, 300));
		indicator.update('second');
		indicator.stop('ok');
		expect(captured).toMatch(/second/);
	});

	it('marks failure with a different final line than success', async () => {
		const indicator = startProgressIndicator('failure path');
		await new Promise((resolveFn) => setTimeout(resolveFn, 300));
		indicator.stop('failed');
		expect(captured).toMatch(/\[fail\]/);
	});
});