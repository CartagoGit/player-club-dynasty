#!/usr/bin/env bun
/**
 * tools/helpers/reference/kiosk-browser-download.helper.ts
 *
 * The archive signs its disk-image URLs with a short-lived token that a
 * page-context `fetch()` receives from `papi/sign` — verified against a
 * raw HTTP client (403 even with the right headers and a valid token)
 * and against a top-level navigation to the same signed URL (also 403,
 * because the request itself is not the same-site `fetch()` the WAF
 * requires). A real page's own `fetch()` for that same URL is the one
 * shape that passes.
 *
 * So this drives a real headless Chrome onto the game's kiosk page,
 * asks it for a signed link exactly as its own emulator would, and
 * relays the bytes out to disk: `fetch` the signed URL from inside the
 * page into a `Blob`, then click a synthetic `<a download>` so Chrome's
 * own download manager — not the CDP wire — writes the file.
 */

import { existsSync, mkdtempSync, rmSync, statSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';

import type { IKioskDownloadFile, IKioskDownloadResult } from '../../contracts/interfaces/reference/reference-game.interface.ts';
import type { ICdpConnection, ICdpPageSession } from '../../contracts/interfaces/system/cdp.interface.ts';
import { connectCdp, launchHeadlessChrome, locateChrome } from '../system/cdp.helper.ts';

const DESKTOP_UA =
	'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36';
const kioskUrl = (gameId: string): string => `https://online.dinamicmultimedia.es/kiosk.html?game=${gameId}`;
const DOWNLOAD_TIMEOUT_MS = 20 * 60 * 1000;

const waitFor = (ms: number): Promise<void> => new Promise((resolveFn) => setTimeout(resolveFn, ms));

const alreadyComplete = (file: IKioskDownloadFile): boolean =>
	file.expectedBytes !== null && existsSync(file.targetPath) && statSync(file.targetPath).size === file.expectedBytes;

/**
 * Fetch every still-missing file for one game through a single kiosk
 * session, reusing one `papi/sign` call for the whole batch.
 */
export const downloadGameViaKiosk = async (
	gameId: string,
	files: readonly IKioskDownloadFile[],
	onProgress?: (filename: string, bytes: number, total: number | null) => void,
): Promise<readonly IKioskDownloadResult[]> => {
	const done: IKioskDownloadResult[] = files
		.filter(alreadyComplete)
		.map((file) => ({ filename: file.filename, ok: true, skipped: true, bytes: file.expectedBytes ?? 0, error: null }));
	const pending = files.filter((file) => !alreadyComplete(file));
	if (pending.length === 0) return done;

	const chromePath = locateChrome();
	if (chromePath === null) {
		return [
			...done,
			...pending.map((file) => ({
				filename: file.filename,
				ok: false,
				skipped: false,
				bytes: 0,
				error: 'no Chrome/Chromium binary found',
			})),
		];
	}

	const profileDir = mkdtempSync(join(tmpdir(), 'pcd-kiosk-'));
	const port = 9200 + Math.floor(Math.random() * 300);
	const browser = await launchHeadlessChrome({ chromePath, port, userDataDir: profileDir });

	try {
		const cdp = await connectCdp(browser.wsUrl);
		try {
			const downloadDir = dirname(pending[0]!.targetPath);
			await cdp.send('Browser.setDownloadBehavior', {
				behavior: 'allow',
				downloadPath: downloadDir,
				eventsEnabled: true,
			});

			const page = await cdp.createPageSession();
			await page.send('Network.enable');
			await page.send('Network.setUserAgentOverride', { userAgent: DESKTOP_UA });
			await page.send('Page.enable');
			await page.send('Runtime.enable');
			await page.send('Page.navigate', { url: kioskUrl(gameId) });
			await waitFor(3000);

			const signed = await mintSignedUrls(page, pending);
			const results: IKioskDownloadResult[] = [...done];
			for (const file of pending) {
				const url = signed[file.filename];
				results.push(
					url === undefined
						? { filename: file.filename, ok: false, skipped: false, bytes: 0, error: 'missing from papi/sign response' }
						: await downloadOne(cdp, page, file, url, onProgress),
				);
			}

			return results;
		} finally {
			cdp.close();
		}
	} finally {
		browser.kill();
		rmSync(profileDir, { recursive: true, force: true });
	}
};

const mintSignedUrls = async (
	page: ICdpPageSession,
	pending: readonly IKioskDownloadFile[],
): Promise<Record<string, string>> => {
	const fileNames = pending.map((file) => file.filename).join(',');
	const result = await page.send('Runtime.evaluate', {
		expression: `fetch('/papi/sign?files=${encodeURIComponent(fileNames)}').then(r => r.ok ? r.json() : Promise.reject(new Error('sign HTTP '+r.status)))`,
		awaitPromise: true,
		returnByValue: true,
	});
	if (result.exceptionDetails) {
		throw new Error(result.exceptionDetails.text ?? 'papi/sign failed');
	}
	return result.result.value as Record<string, string>;
};

const downloadOne = async (
	cdp: ICdpConnection,
	page: ICdpPageSession,
	file: IKioskDownloadFile,
	signedUrl: string,
	onProgress?: (filename: string, bytes: number, total: number | null) => void,
): Promise<IKioskDownloadResult> => {
	let guid: string | null = null;
	let outcome: 'completed' | 'canceled' | 'interrupted' | null = null;

	const finished = new Promise<void>((resolveFn) => {
		cdp.on((method, params) => {
			if (method === 'Browser.downloadWillBegin' && params.suggestedFilename === file.filename) {
				guid = params.guid;
			}
			if (method === 'Browser.downloadProgress' && params.guid === guid) {
				onProgress?.(file.filename, params.receivedBytes, params.totalBytes || file.expectedBytes);
				if (params.state !== 'inProgress') {
					outcome = params.state;
					resolveFn();
				}
			}
		});
	});

	const triggered = await page.send('Runtime.evaluate', {
		expression: `
			fetch(${JSON.stringify(signedUrl)})
				.then(r => { if (!r.ok) throw new Error('fetch HTTP ' + r.status); return r.blob(); })
				.then(blob => {
					const a = document.createElement('a');
					a.href = URL.createObjectURL(blob);
					a.download = ${JSON.stringify(file.filename)};
					document.body.appendChild(a);
					a.click();
					return { size: blob.size };
				})
		`,
		awaitPromise: true,
		returnByValue: true,
	});

	if (triggered.exceptionDetails) {
		return {
			filename: file.filename,
			ok: false,
			skipped: false,
			bytes: 0,
			error: triggered.exceptionDetails.text ?? 'fetch failed inside the page',
		};
	}

	await Promise.race([finished, waitFor(DOWNLOAD_TIMEOUT_MS)]);

	if (outcome === null) {
		return { filename: file.filename, ok: false, skipped: false, bytes: 0, error: 'timed out waiting for the download to land' };
	}
	if (outcome !== 'completed') {
		return { filename: file.filename, ok: false, skipped: false, bytes: 0, error: `download ${outcome}` };
	}

	const bytes = existsSync(file.targetPath) ? statSync(file.targetPath).size : 0;
	return { filename: file.filename, ok: true, skipped: false, bytes, error: null };
};
