import { mkdirSync, mkdtempSync, readlinkSync, realpathSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

import { afterEach, describe, expect, it } from 'vitest';

import { ensureRuntimeLink, expandPath, resolveRoot, runtimeTarget, smokeCheck } from './root.script.ts';

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
	const checkout = fakeCheckout(join(base, 'delendai'));
	return { workspace, checkout };
};

afterEach(() => {
	delete process.env.DELENDAI_ROOT;
	delete process.env.DELENDAI_PATH;
	while (created.length > 0) {
		const path = created.pop();
		if (path !== undefined) rmSync(path, { recursive: true, force: true });
	}
});

describe('expandPath', () => {
	it('keeps an absolute path untouched', () => {
		expect(expandPath('/opt/delendai', '/workspace')).toBe('/opt/delendai');
	});

	it('resolves a relative path against the workspace', () => {
		expect(expandPath('../../delendai', '/srv/dev/games/project')).toBe(
			'/srv/dev/delendai',
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
		writeFileSync(join(workspace, '.delendai-root'), '../../delendai\n');

		expect(resolveRoot(workspace)?.path).toBe(checkout);
	});

	it('finds a sibling checkout with no pointer and no environment variable', () => {
		const { workspace, checkout } = sandbox();

		expect(resolveRoot(workspace, join(workspace, 'no-home'))?.path).toBe(checkout);
	});

	it('lets the environment variable win over the pointer file', () => {
		const { workspace } = sandbox();
		writeFileSync(join(workspace, '.delendai-root'), '../../delendai\n');
		const override = fakeCheckout(join(workspace, 'vendor', 'delendai'));
		process.env.DELENDAI_ROOT = override;

		expect(resolveRoot(workspace)?.path).toBe(override);
	});

	it('reports a broken explicit pointer instead of silently using another checkout', () => {
		const { workspace } = sandbox();
		writeFileSync(join(workspace, '.delendai-root'), './not-a-checkout\n');

		const resolved = resolveRoot(workspace);

		expect(resolved?.path).toBe(join(workspace, 'not-a-checkout'));
		expect(smokeCheck(resolved?.path ?? '').ok).toBe(false);
	});
});

describe('runtime link', () => {
	it('targets DELENDAI_PATH when set, else the stable install', () => {
		expect(runtimeTarget('/h')).toBe('/h/.local/share/delendai/stable');
		process.env.DELENDAI_PATH = '/opt/delendai';

		expect(runtimeTarget('/h')).toBe('/opt/delendai');
	});

	it('creates the link, refreshes a stale one and resolves through it', () => {
		const { workspace, checkout } = sandbox();
		const other = fakeCheckout(join(workspace, 'vendor', 'delendai'));

		const link = ensureRuntimeLink(workspace, other);
		expect(readlinkSync(link)).toBe(other);
		ensureRuntimeLink(workspace, checkout);

		expect(readlinkSync(link)).toBe(checkout);
		expect(realpathSync(resolveRoot(workspace)?.path ?? '')).toBe(realpathSync(checkout));
	});

	it('fails with a clear error when no install exists', () => {
		const { workspace } = sandbox();

		expect(() => ensureRuntimeLink(workspace, join(workspace, 'missing'))).toThrow(
			/DELENDAI_PATH/,
		);
	});
});
