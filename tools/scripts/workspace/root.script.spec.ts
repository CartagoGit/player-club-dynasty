import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

import { afterEach, describe, expect, it } from 'vitest';

import { expandPath, resolveRoot, smokeCheck } from './root.script.ts';

const created: string[] = [];

/** Build a directory that passes the checkout smoke check. */
const fakeCheckout = (path: string): string => {
	mkdirSync(join(path, 'tools', 'scripts', 'host'), { recursive: true });
	mkdirSync(join(path, 'packages', 'core'), { recursive: true });
	writeFileSync(join(path, 'tools', 'scripts', 'host', 'host-server.script.ts'), '');
	writeFileSync(join(path, 'packages', 'core', 'package.json'), '{}');
	return path;
};

const sandbox = (): { workspace: string; checkout: string } => {
	const base = mkdtempSync(join(tmpdir(), 'pcd-root-'));
	created.push(base);
	const workspace = join(base, 'games', 'player-club-dynasty');
	mkdirSync(workspace, { recursive: true });
	const checkout = fakeCheckout(join(base, 'mcp-vertex'));
	return { workspace, checkout };
};

afterEach(() => {
	delete process.env.MCP_VERTEX_ROOT;
	while (created.length > 0) {
		const path = created.pop();
		if (path !== undefined) rmSync(path, { recursive: true, force: true });
	}
});

describe('expandPath', () => {
	it('keeps an absolute path untouched', () => {
		expect(expandPath('/opt/mcp-vertex', '/workspace')).toBe('/opt/mcp-vertex');
	});

	it('resolves a relative path against the workspace', () => {
		expect(expandPath('../../mcp-vertex', '/home/dev/games/project')).toBe(
			'/home/dev/mcp-vertex',
		);
	});
});

describe('smokeCheck', () => {
	it('reports the sentinels a directory is missing', () => {
		const { workspace } = sandbox();

		expect(smokeCheck(workspace).ok).toBe(false);
		expect(smokeCheck(workspace).missing).toHaveLength(2);
	});
});

describe('resolveRoot', () => {
	it('follows a relative pointer file, so the same value works on any machine', () => {
		const { workspace, checkout } = sandbox();
		writeFileSync(join(workspace, '.mcp-vertex-root'), '../../mcp-vertex\n');

		expect(resolveRoot(workspace)?.path).toBe(checkout);
	});

	it('finds a sibling checkout with no pointer and no environment variable', () => {
		const { workspace, checkout } = sandbox();

		expect(resolveRoot(workspace)?.path).toBe(checkout);
	});

	it('lets the environment variable win over the pointer file', () => {
		const { workspace } = sandbox();
		writeFileSync(join(workspace, '.mcp-vertex-root'), '../../mcp-vertex\n');
		const override = fakeCheckout(join(workspace, 'vendor', 'mcp-vertex'));
		process.env.MCP_VERTEX_ROOT = override;

		expect(resolveRoot(workspace)?.path).toBe(override);
	});

	it('reports a broken explicit pointer instead of silently using another checkout', () => {
		const { workspace } = sandbox();
		writeFileSync(join(workspace, '.mcp-vertex-root'), './not-a-checkout\n');

		const resolved = resolveRoot(workspace);

		expect(resolved?.path).toBe(join(workspace, 'not-a-checkout'));
		expect(smokeCheck(resolved?.path ?? '').ok).toBe(false);
	});
});
