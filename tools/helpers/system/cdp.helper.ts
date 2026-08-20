#!/usr/bin/env bun
/**
 * tools/helpers/system/cdp.helper.ts
 *
 * Launch a headless Chrome and speak the Chrome DevTools Protocol to it
 * over its own debugger WebSocket. Some archives only serve a resource
 * to a same-site `fetch()` initiated by a loaded page — no header a raw
 * HTTP client sends reproduces that, so the only faithful client is a
 * real browser.
 */

import { spawn } from 'node:child_process';
import { existsSync } from 'node:fs';

import type { ICdpBrowser, ICdpConnection, ICdpPageSession } from '../../contracts/interfaces/system/cdp.interface.ts';

const CHROME_CANDIDATES: readonly string[] = [
	'/usr/bin/google-chrome',
	'/usr/bin/chromium',
	'/usr/bin/chromium-browser',
];

/** Find an installed Chrome or Chromium binary. `null` when none is on disk. */
export const locateChrome = (): string | null => CHROME_CANDIDATES.find(existsSync) ?? null;

export interface ILaunchChromeOptions {
	readonly chromePath: string;
	readonly port: number;
	readonly userDataDir: string;
}

/** Start headless Chrome and wait for its debugger port to answer. */
export const launchHeadlessChrome = async (options: ILaunchChromeOptions): Promise<ICdpBrowser> => {
	const proc = spawn(
		options.chromePath,
		[
			'--headless=new',
			'--disable-gpu',
			'--no-sandbox',
			'--disable-dev-shm-usage',
			`--remote-debugging-port=${options.port}`,
			`--user-data-dir=${options.userDataDir}`,
		],
		{ stdio: 'ignore' },
	);

	const versionUrl = `http://localhost:${options.port}/json/version`;
	let wsUrl: string | null = null;
	for (let attempt = 0; attempt < 50 && wsUrl === null; attempt += 1) {
		await new Promise((resolveFn) => setTimeout(resolveFn, 200));
		try {
			const info = (await fetch(versionUrl).then((r) => r.json())) as { webSocketDebuggerUrl: string };
			wsUrl = info.webSocketDebuggerUrl;
		} catch {
			// Chrome is still starting; keep polling.
		}
	}

	if (wsUrl === null) {
		proc.kill();
		throw new Error(`chrome did not open a debugger port on ${options.port}`);
	}

	return { wsUrl, kill: () => proc.kill() };
};

/** Open a CDP connection and expose a minimal command/event surface over it. */
export const connectCdp = async (wsUrl: string): Promise<ICdpConnection> => {
	const ws = new WebSocket(wsUrl);
	await new Promise<void>((resolveFn, rejectFn) => {
		ws.onopen = () => resolveFn();
		ws.onerror = (event) => rejectFn(new Error(`CDP socket error: ${String(event)}`));
	});

	let nextId = 1;
	const pending = new Map<number, { resolve: (value: any) => void; reject: (error: Error) => void }>();
	const listeners: Array<(method: string, params: any, sessionId?: string) => void> = [];

	ws.onmessage = (event) => {
		const msg = JSON.parse(event.data as string);
		if (msg.id !== undefined && pending.has(msg.id)) {
			const { resolve, reject } = pending.get(msg.id)!;
			pending.delete(msg.id);
			if (msg.error) reject(new Error(msg.error.message ?? 'CDP error'));
			else resolve(msg.result);
			return;
		}
		if (msg.method !== undefined) {
			for (const listener of listeners) listener(msg.method, msg.params, msg.sessionId);
		}
	};

	const send = (method: string, params: Record<string, unknown> = {}, sessionId?: string): Promise<any> => {
		const id = nextId++;
		ws.send(JSON.stringify({ id, method, params, ...(sessionId ? { sessionId } : {}) }));
		return new Promise((resolveFn, rejectFn) => pending.set(id, { resolve: resolveFn, reject: rejectFn }));
	};

	return {
		send: (method, params) => send(method, params),
		on: (handler) => listeners.push(handler),
		close: () => ws.close(),
		createPageSession: async (): Promise<ICdpPageSession> => {
			const target = await send('Target.createTarget', { url: 'about:blank' });
			const attach = await send('Target.attachToTarget', { targetId: target.targetId, flatten: true });
			const sessionId = attach.sessionId as string;
			return { sessionId, send: (method, params) => send(method, params, sessionId) };
		},
	};
};
