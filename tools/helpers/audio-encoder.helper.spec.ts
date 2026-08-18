#!/usr/bin/env bun
/**
 * tools/helpers/audio-encoder.helper.spec.ts
 *
 * Tests for the ffmpeg wrapper. We avoid running ffmpeg end-to-end in
 * the test suite (it requires a real wav file and the system ffmpeg);
 * we instead test the pure helpers:
 *   - isFfmpegAvailable returns a boolean,
 *   - encodeWavTo surfaces a clear error when the source is missing,
 *   - encodeWavTo surfaces a clear error when ffmpeg is not installed
 *     (the latter is hard to simulate without mutating PATH — we cover
 *     the missing-source case as the most important regression guard).
 */

import { describe, expect, it } from 'vitest';

import { encodeWavTo, isFfmpegAvailable } from './audio-encoder.helper';

describe('audio-encoder.helper.ts', () => {
	it('isFfmpegAvailable returns a boolean', () => {
		const result = isFfmpegAvailable();
		expect(typeof result).toBe('boolean');
	});

	it('encodeWavTo fails fast when the source wav is missing', () => {
		const result = encodeWavTo('/nonexistent/path.wav', '/tmp/out.ogg', 'ogg');
		expect(result.ok).toBe(false);
		expect(result.stderr).toMatch(/source wav missing/);
		expect(result.bytes).toBeNull();
		expect(result.codec).toBeNull();
	});
});