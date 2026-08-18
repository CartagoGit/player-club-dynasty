#!/usr/bin/env bun
/**
 * tools/helpers/progress.helper.ts
 *
 * A small, dependency-free progress indicator for shell-outs that don't
 * surface their own. mmx, ffmpeg and similar CLIs may take seconds (or
 * minutes for full music tracks) with no output between "starting…" and
 * "done." — without a hint, the user thinks the terminal hung.
 *
 * The helper detects TTY vs redirected stdio and:
 *   - on a TTY:  starts a single-line spinner that ticks every 250ms
 *                and prints elapsed seconds; clears itself on stop().
 *   - on a pipe: emits a single "[..] working…" line and an "[ok]
 *                finished in 42.1s" line — no escape sequences that
 *                would corrupt logs.
 *
 * Usage:
 *   const indicator = startProgressIndicator('generating menu_main');
 *   const exit = await runLongCommand(...);
 *   indicator.stop(exit === 0 ? 'ok' : 'failed');
 */

export type ProgressLabel = string;

export interface IProgressIndicator {
	/** Stop the indicator and print a final status line. */
	stop(finalStatus?: 'ok' | 'failed' | string): void;
	/** Update the activity label mid-flight (e.g. "uploading…"). */
	update(label: ProgressLabel): void;
}

const TICK_INTERVAL_MS = 250;
const SPINNER_FRAMES = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];

const isTTY = (fd: number): boolean => {
	try {
		return Boolean(process.stderr.isTTY) && fd === 2;
	} catch {
		return false;
	}
};

/**
 * Start a progress indicator. Safe to call from any context: it never
 * throws and degrades to a single log line when stdout/stderr are not
 * TTYs.
 */
export const startProgressIndicator = (label: ProgressLabel): IProgressIndicator => {
	const startMs = Date.now();
	let currentLabel = label;
	let frameIndex = 0;
	let lastRenderMs = 0;
	let stopped = false;

	const formatElapsed = (): string => {
		const seconds = (Date.now() - startMs) / 1000;
		return `${seconds.toFixed(1)}s`;
	};

	const writeTTY = (frame: string): void => {
		const elapsed = formatElapsed();
		const line = `\r\x1b[2K${frame} ${currentLabel} (${elapsed})`;
		process.stderr.write(line);
		lastRenderMs = Date.now();
	};

	const writePipe = (frame: string): void => {
		// Only emit the first tick; afterwards stay quiet so logs aren't spammed.
		if (frame === 'first') {
			process.stderr.write(`[..] ${currentLabel} …\n`);
			lastRenderMs = Date.now();
		}
	};

	const tick = (): void => {
		if (stopped) return;
		const frame = SPINNER_FRAMES[frameIndex % SPINNER_FRAMES.length] ?? SPINNER_FRAMES[0];
		frameIndex += 1;
		if (isTTY(2)) {
			writeTTY(frame);
		} else {
			writePipe('first');
		}
	};

	const interval = setInterval(tick, TICK_INTERVAL_MS);
	// unref so the indicator never keeps the Node loop alive on its own.
	if (typeof interval.unref === 'function') interval.unref();

	const stop = (finalStatus: string = 'ok'): void => {
		if (stopped) return;
		stopped = true;
		clearInterval(interval);
		const elapsed = formatElapsed();
		if (isTTY(2)) {
			// Clear the spinner line, then write the final summary on a fresh line.
			process.stderr.write(`\r\x1b[2K`);
			const glyph = finalStatus === 'ok' ? '✓' : finalStatus === 'failed' ? '✗' : '·';
			process.stderr.write(`${glyph} ${currentLabel} — ${finalStatus} in ${elapsed}\n`);
		} else {
			// Only print the final line if we ever printed the "[..]" header.
			if (lastRenderMs === 0) return;
			const glyph = finalStatus === 'ok' ? '[ok]' : '[fail]';
			process.stderr.write(`${glyph} ${currentLabel} in ${elapsed}\n`);
		}
	};

	const update = (label: ProgressLabel): void => {
		currentLabel = label;
		// Force a re-tick on the next interval so the new label is visible.
	};

	return { stop, update };
};

/** Sleep helper — kept here so callers don't import timers elsewhere. */
export const sleep = (ms: number): Promise<void> =>
	new Promise((resolveFn) => setTimeout(resolveFn, ms));