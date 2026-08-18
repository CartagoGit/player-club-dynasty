#!/usr/bin/env bun
/**
 * tools/helpers/audio-encoder.helper.ts
 *
 * Thin ffmpeg wrapper for the audio approve step.
 *
 * One responsibility: turn a wav file on disk into the slot's target
 * format (.ogg / .opus / .mp3). Single public function, single output
 * shape (`IAudioEncodeResult`) so the runner doesn't have to know which
 * codec we picked or how ffmpeg was invoked.
 */

import { existsSync, statSync } from 'node:fs';
import { spawnSync } from 'node:child_process';

import { run } from './process.helper.ts';
import type { IAudioEncodeResult } from '../contracts/interfaces/audio-spec.interface.ts';

/** Pick a libavcodec family name for the slot's target format. */
const codecFor = (format: string): string => {
	switch (format) {
		case 'mp3':
			return 'libmp3lame';
		case 'opus':
			return 'libopus';
		case 'wav':
			return 'pcm_s16le';
		case 'ogg':
		default:
			return 'libvorbis';
	}
};

/** Cheap probe — returns true when `ffmpeg` is on PATH. */
export const isFfmpegAvailable = (): boolean => {
	const probe = spawnSync('ffmpeg', ['-version'], { encoding: 'utf8' });
	return probe.status === 0;
};

/** Encode `wavAbs` into `targetAbs` using the codec for `format`. */
export const encodeWavTo = (
	wavAbs: string,
	targetAbs: string,
	format: string,
): IAudioEncodeResult => {
	if (!existsSync(wavAbs)) {
		return { ok: false, stderr: `source wav missing: ${wavAbs}`, bytes: null, codec: null };
	}
	if (!isFfmpegAvailable()) {
		return {
			ok: false,
			stderr: 'ffmpeg is required to encode the approved variant. Install it or set FFMPEG_BIN.',
			bytes: null,
			codec: null,
		};
	}
	const codec = codecFor(format);
	const result = run('ffmpeg', {
		args: ['-y', '-i', wavAbs, '-vn', '-acodec', codec, '-q:a', '5', targetAbs],
	});
	if (!result.succeeded) {
		return { ok: false, stderr: result.stderr, bytes: null, codec };
	}
	const bytes = existsSync(targetAbs) ? statSync(targetAbs).size : null;
	return { ok: true, stderr: '', bytes, codec };
};