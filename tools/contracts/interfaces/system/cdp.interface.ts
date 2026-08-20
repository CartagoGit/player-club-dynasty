/**
 * tools/contracts/interfaces/system/cdp.interface.ts
 *
 * The narrow slice of the Chrome DevTools Protocol this workspace needs:
 * launch a headless Chrome, open one page, send commands to it, and
 * listen for events. Not a general automation library — just enough to
 * drive a real browser through a real page.
 */

export interface ICdpBrowser {
	readonly wsUrl: string;
	kill(): void;
}

export interface ICdpPageSession {
	readonly sessionId: string;
	send(method: string, params?: Record<string, unknown>): Promise<any>;
}

export interface ICdpConnection {
	send(method: string, params?: Record<string, unknown>): Promise<any>;
	createPageSession(): Promise<ICdpPageSession>;
	/** Fires for every unsolicited protocol event, browser-wide and per-session alike. */
	on(handler: (method: string, params: any, sessionId?: string) => void): void;
	close(): void;
}
