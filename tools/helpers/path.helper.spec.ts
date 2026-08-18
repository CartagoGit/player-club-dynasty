/**
 * tools/helpers/path.helper.spec.ts
 *
 * Pin the workspace anchors used by every script. Renames in
 * `tools/scripts/<area>/` would otherwise sneak past typecheck.
 */

import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

import { describe, expect, it } from 'vitest';

import { resolveProjectPaths, resolveToolsDir } from './path.helper';

const here = dirname(fileURLToPath(import.meta.url));

describe('path.helper', () => {
	it('resolves the workspace root from a nested src/lib file', () => {
		const paths = resolveProjectPaths(join(here, '..', '..', 'scripts', 'workspace', 'run.script.ts'));
		expect(paths.workspaceRoot.endsWith('player-club-dynasty')).toBe(true);
		expect(paths.gameRoot.endsWith(join('player-club-dynasty', 'game'))).toBe(true);
		expect(paths.projectDir.endsWith(join('player-club-dynasty', 'game', 'godot'))).toBe(true);
	});

	it('defaults toolsDir to <workspace>/.tools', () => {
		const fallback = resolveToolsDir('/workspace');
		expect(fallback).toBe('/workspace/.tools');
	});

	it('honours PCDYNASTY_TOOLS_DIR when absolute', () => {
		const previous = process.env.PCDYNASTY_TOOLS_DIR;
		process.env.PCDYNASTY_TOOLS_DIR = '/alt/cache';
		try {
			expect(resolveToolsDir('/workspace')).toBe('/alt/cache');
		} finally {
			if (previous === undefined) {
				delete process.env.PCDYNASTY_TOOLS_DIR;
			} else {
				process.env.PCDYNASTY_TOOLS_DIR = previous;
			}
		}
	});
});
