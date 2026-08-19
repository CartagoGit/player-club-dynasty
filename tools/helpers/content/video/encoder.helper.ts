#!/usr/bin/env bun
/**
 * tools/helpers/content/video/encoder.helper.ts
 *
 * Thin ffmpeg wrapper for the video approve step.
 *
 * One responsibility: turn whatever the generator produced (an mp4) into
 * the format Godot can actually play, and report the result in a single
 * shape so the runner never has to know which codec was used.
 *
 * Why Ogg Theora and not the mp4 we already have: Godot 4's
 * `VideoStreamPlayer` reads Ogg Theora out of the box and nothing else
 * without a third-party addon. Shipping the mp4 would mean shipping an
 * addon, or a file the engine silently refuses to open — which is the kind
 * of "it's in the repo so it works" assumption this project keeps finding
 * the hard way.
 */

import { existsSync, statSync } from 'node:fs';
import { spawnSync } from 'node:child_process';

import { runSync } from '../../system/process.helper.ts';
import type { IVideoEncodeResult } from '../../../contracts/interfaces/content/video-spec.interface.ts';

/** Video codec for a target container. */
const videoCodecFor = (format: string): string => {
	switch (format) {
		case 'webm':
			return 'libvpx-vp9';
		case 'mp4':
			return 'libx264';
		case 'ogv':
		default:
			return 'libtheora';
	}
};

/** Audio codec that belongs with the container. */
const audioCodecFor = (format: string): string => {
	switch (format) {
		case 'webm':
			return 'libopus';
		case 'mp4':
			return 'aac';
		case 'ogv':
		default:
			return 'libvorbis';
	}
};

/** Cheap probe — true when `ffmpeg` is on PATH. */
export const isFfmpegAvailable = (): boolean => {
	const probe = spawnSync('ffmpeg', ['-version'], { encoding: 'utf8' });
	return probe.status === 0;
};

/** True when this ffmpeg can actually produce the codec we need. */
export const supportsCodec = (codec: string): boolean => {
	const probe = spawnSync('ffmpeg', ['-hide_banner', '-encoders'], { encoding: 'utf8' });
	if (probe.status !== 0) return false;
	return (probe.stdout ?? '').includes(codec);
};

/** Transcode `sourceAbs` into `targetAbs` for the slot's shipping `format`. */
export const encodeVideoTo = (
	sourceAbs: string,
	targetAbs: string,
	format: string,
): IVideoEncodeResult => {
	if (!existsSync(sourceAbs)) {
		return { ok: false, stderr: `source clip missing: ${sourceAbs}`, bytes: null, codec: null };
	}
	if (!isFfmpegAvailable()) {
		return {
			ok: false,
			stderr: 'ffmpeg is required to encode the approved clip. Install it or set FFMPEG_BIN.',
			bytes: null,
			codec: null,
		};
	}
	const codec = videoCodecFor(format);
	if (!supportsCodec(codec)) {
		return {
			ok: false,
			stderr: `this ffmpeg has no ${codec} encoder, so it cannot produce ${format}. Install an ffmpeg built with it.`,
			bytes: null,
			codec,
		};
	}
	const result = runSync('ffmpeg', [
		'-y',
		'-i',
		sourceAbs,
		'-c:v',
		codec,
		// Theora's quality scale is 0–10; 7 keeps a menu backdrop clean
		// without the file growing past a few megabytes.
		'-q:v',
		'7',
		'-c:a',
		audioCodecFor(format),
		'-q:a',
		'5',
		targetAbs,
	]);
	if (!result.succeeded) {
		return { ok: false, stderr: result.stderr, bytes: null, codec };
	}
	const bytes = existsSync(targetAbs) ? statSync(targetAbs).size : null;
	return { ok: true, stderr: '', bytes, codec };
};
