#!/usr/bin/env bun
/**
 * tools/helpers/system/download.helper.ts
 *
 * Resumable HTTP download with progress, for files too big to retry from
 * scratch.
 *
 * A gigabyte-sized transfer that restarts on every hiccup never finishes,
 * so this one asks for the remainder with a `Range` header and appends.
 * It also refuses to trust a file just because it exists: a half-written
 * image is the same size as no image at all to anything that reads it,
 * and the failure surfaces much later and much more confusingly.
 */

import { createWriteStream, existsSync, mkdirSync, statSync } from 'node:fs';
import { dirname } from 'node:path';
import type { WriteStream } from 'node:fs';
import { once } from 'node:events';

export interface IDownloadOutcome {
	readonly ok: boolean;
	/** True when the target was already complete and nothing was fetched. */
	readonly skipped: boolean;
	readonly bytes: number;
	readonly error: string | null;
}

export interface IDownloadOptions {
	/** Declared size. When known, it decides "complete" and enables resume. */
	readonly expectedBytes?: number | null;
	/** Called with bytes-so-far, at most a few times a second. */
	readonly onProgress?: (bytes: number, total: number | null) => void;
	/** Identifies the caller in the User-Agent. */
	readonly userAgent?: string;
}

const USER_AGENT =
	'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36';

/** Human-readable size, for logs a person reads while waiting. */
export const formatBytes = (bytes: number): string => {
	if (bytes >= 1e9) return `${(bytes / 1e9).toFixed(2)} GB`;
	if (bytes >= 1e6) return `${(bytes / 1e6).toFixed(0)} MB`;
	if (bytes >= 1e3) return `${(bytes / 1e3).toFixed(0)} KB`;
	return `${bytes} B`;
};

const sizeOnDisk = (path: string): number => (existsSync(path) ? statSync(path).size : 0);

/** Wait for the bytes to actually reach the disk before judging the size. */
const closeStream = async (sink: WriteStream): Promise<void> => {
	await new Promise<void>((resolveClose) => sink.end(resolveClose));
};

/**
 * Fetch `url` into `targetPath`, continuing an interrupted transfer when
 * the server allows it.
 *
 * Returns rather than throws: a batch of downloads should report which
 * ones failed and carry on, not die on the first one.
 */
export const downloadResumable = async (
	url: string,
	targetPath: string,
	options: IDownloadOptions = {},
): Promise<IDownloadOutcome> => {
	const expected = options.expectedBytes ?? null;
	const already = sizeOnDisk(targetPath);

	if (expected !== null && already === expected) {
		return { ok: true, skipped: true, bytes: already, error: null };
	}

	// A local file larger than declared cannot be a prefix of the real one,
	// so resuming would append to garbage. Start it again instead.
	const resumeFrom = expected !== null && already > expected ? 0 : already;

	try {
		const headers: Record<string, string> = {
			'User-Agent': options.userAgent ?? USER_AGENT,
		};
		if (resumeFrom > 0) headers.Range = `bytes=${resumeFrom}-`;

		const response = await fetch(url, { headers, redirect: 'follow' });
		if (!response.ok) {
			return {
				ok: false,
				skipped: false,
				bytes: already,
				error: `HTTP ${response.status} ${response.statusText}`,
			};
		}

		// Asking for a range and getting 200 means the server ignored it and
		// is sending the whole file: append would corrupt, so overwrite.
		const serverHonouredRange = response.status === 206;
		const startAt = serverHonouredRange ? resumeFrom : 0;
		if (response.body === null) {
			return { ok: false, skipped: false, bytes: already, error: 'empty response body' };
		}

		mkdirSync(dirname(targetPath), { recursive: true });
		const sink = createWriteStream(targetPath, startAt > 0 ? { flags: 'a' } : { flags: 'w' });

		let written = startAt;
		let lastReport = 0;
		const reader = response.body.getReader();
		try {
			for (;;) {
				const { done, value } = await reader.read();
				if (done) break;
				if (value === undefined) continue;
				// `write` returning false means the buffer is full; waiting for
				// `drain` is what keeps a gigabyte transfer from being held in
				// memory all at once.
				if (!sink.write(value)) await once(sink, 'drain');
				written += value.byteLength;
				const now = Date.now();
				if (options.onProgress && now - lastReport > 400) {
					lastReport = now;
					options.onProgress(written, expected);
				}
			}
		} finally {
			await closeStream(sink);
		}

		options.onProgress?.(written, expected);

		if (expected !== null && written !== expected) {
			return {
				ok: false,
				skipped: false,
				bytes: written,
				error: `expected ${expected} bytes, got ${written}`,
			};
		}

		return { ok: true, skipped: false, bytes: written, error: null };
	} catch (error) {
		return {
			ok: false,
			skipped: false,
			bytes: sizeOnDisk(targetPath),
			error: error instanceof Error ? error.message : String(error),
		};
	}
};
