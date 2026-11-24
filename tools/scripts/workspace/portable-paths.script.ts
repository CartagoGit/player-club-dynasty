#!/usr/bin/env bun
/**
 * tools/scripts/workspace/portable-paths.script.ts
 *
 * Fail when a tracked file carries a machine path (`/home/<user>`,
 * `/Users/<user>`, a Windows profile) next to a delendai reference.
 * Tracked files may only name `.cache/delendai-runtime`.
 *
 * Usage: bun tools/scripts/workspace/portable-paths.script.ts --check
 */

import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

import { workspaceRoot } from './root.script.ts';

const MACHINE_PATH = /(?:\/home\/[\w.-]+|\/Users\/[\w.-]+|[A-Za-z]:\\Users\\)/;
const EXEMPT = new Set(['bun.lock', 'tools/scripts/workspace/portable-paths.script.ts']);

/** `file:line` of every line that mixes a machine path with delendai. */
export const findMachinePathLines = (files: ReadonlyMap<string, string>): string[] => {
	const hits: string[] = [];
	for (const [file, text] of files) {
		if (EXEMPT.has(file) || file.startsWith('docs/delendai/proposals/')) continue;
		const index = text
			.split('\n')
			.findIndex((line) => MACHINE_PATH.test(line) && /delendai/i.test(line));
		if (index >= 0) hits.push(`${file}:${index + 1}`);
	}
	return hits;
};

const readTrackedFiles = (workspace: string): Map<string, string> => {
	const names = execFileSync('git', ['ls-files', '-z'], { cwd: workspace, encoding: 'utf8' })
		.split('\0')
		.filter((name) => name.length > 0);
	const files = new Map<string, string>();
	for (const name of names) {
		try {
			const text = readFileSync(join(workspace, name), 'utf8');
			if (!text.includes('\0')) files.set(name, text);
		} catch {
			// deleted or unreadable: nothing to scan
		}
	}
	return files;
};

if (import.meta.main) {
	const hits = findMachinePathLines(readTrackedFiles(workspaceRoot));
	if (hits.length > 0) {
		process.stderr.write(`machine paths in tracked files:\n  ${hits.join('\n  ')}\n`);
		process.exit(1);
	}
}
