#!/usr/bin/env bun
/**
 * tools/scripts/replays/visible-routes.script.ts
 *
 * Gate that catches "the slice closes, but the player never sees it".
 * A proposal marking itself as player-visible must name at least one
 * file inside `game/godot/`, the .csproj's compile-time palette.
 *
 * Migrated from `scripts/check-visible-capability-routes.script.ts`.
 *
 * Usage:
 *   bun tools/scripts/replays/visible-routes.script.ts
 */

import { existsSync } from 'node:fs';
import { join } from 'node:path';

import { workspaceRoot } from '../workspace/root.script.ts';

export const PRESENTATION_ROOT = 'game/godot/';
export const MAIN_SCENE = 'game/godot/Scenes/Main.tscn';
export const PRODUCT_TRACKS: ReadonlySet<string> = new Set(['product', 'roadmap']);
export const BEHAVIOUR_KINDS: ReadonlySet<string> = new Set(['feat', 'fix', 'refactor', 'perf', 'breaking']);
export const VISIBILITY_MARKERS: readonly string[] = [
	'godot', 'smokeboot', 'main.tscn', 'screen', 'workspace', 'sidebar', 'menu', 'on screen', 'visible', 'navigat',
];
const MATERIALISED_STATUSES: ReadonlySet<string> = new Set(['done']);
const SKIPPED_FOLDERS: ReadonlySet<string> = new Set(['legacy', 'retired']);
const PROPOSAL_FILENAME = /^[a-z]\d{5}-[a-z0-9-]+\.md$/;

export { workspaceRoot };

const readFrontmatterField = (markdown: string, field: string): string => {
	const fenceEnd = markdown.indexOf('\n---', 3);
	const head = fenceEnd === -1 ? markdown.slice(0, 4096) : markdown.slice(0, fenceEnd);
	const match = new RegExp(`^${field}:\\s*"?([^"\\n]+)"?\\s*$`, 'm').exec(head);
	return match?.[1]?.trim() ?? '';
};

export const parseDeclaredFiles = (markdown: string): readonly string[] => {
	const files: string[] = [];
	for (const line of markdown.split('\n')) {
		const trimmed = line.trim();
		if (!trimmed.startsWith('- **Files**:')) continue;
		for (const match of trimmed.matchAll(/`([^`]+)`/g)) {
			const value = match[1]?.trim() ?? '';
			if (value.length > 0) files.push(value);
		}
	}
	return files;
};

export interface IProposalRecord {
	readonly path: string;
	readonly id: string;
	readonly status: string;
	readonly kind: string;
	readonly track: string;
	readonly files: readonly string[];
	readonly body: string;
}

const listProposalFiles = (proposalsDir: string): string[] => {
	const { readdirSync, statSync } = require('node:fs') as typeof import('node:fs');
	const out: string[] = [];
	const queue: string[] = [proposalsDir];
	while (queue.length > 0) {
		const dir = queue.pop() as string;
		for (const entry of readdirSync(dir)) {
			const full = join(dir, entry);
			const stat = statSync(full);
			if (stat.isDirectory()) {
				if (SKIPPED_FOLDERS.has(entry)) continue;
				queue.push(full);
			} else if (stat.isFile() && PROPOSAL_FILENAME.test(entry)) {
				out.push(full);
			}
		}
	}
	return out;
};

export const parseProposal = (path: string, markdown: string): IProposalRecord => ({
	path,
	id: readFrontmatterField(markdown, 'id'),
	status: readFrontmatterField(markdown, 'status'),
	kind: readFrontmatterField(markdown, 'kind'),
	track: readFrontmatterField(markdown, 'track'),
	files: parseDeclaredFiles(markdown),
	body: markdown,
});

export interface IRouteViolation {
	readonly id: string;
	readonly path: string;
	readonly reason: string;
	readonly fix: string;
}

const main = (): number => {
	const proposalsDir = join(workspaceRoot, 'docs', 'mcp-vertex', 'proposals');
	const files = listProposalFiles(proposalsDir);
	const violations: IRouteViolation[] = [];
	const { readFileSync } = require('node:fs') as typeof import('node:fs');

	for (const path of files) {
		const markdown = readFileSync(path, 'utf8');
		const record = parseProposal(path, markdown);
		if (!PRODUCT_TRACKS.has(record.track)) continue;
		if (!BEHAVIOUR_KINDS.has(record.kind)) continue;
		const visible = VISIBILITY_MARKERS.some((marker) => record.body.toLowerCase().includes(marker));
		if (!visible) continue;
		const touchesPresentation = record.files.some((file) => file.startsWith(PRESENTATION_ROOT));
		if (!touchesPresentation) {
			violations.push({
				id: record.id,
				path,
				reason: 'Visibility vocabulary but no `game/godot/**` file in slice `Files`.',
				fix: 'Add at least one path that lives under `game/godot/` (the executable surface).',
			});
			continue;
		}
		if (MATERIALISED_STATUSES.has(record.status)) {
			for (const file of record.files) {
				if (file.startsWith(PRESENTATION_ROOT) && !existsSync(join(workspaceRoot, file))) {
					violations.push({
						id: record.id,
						path,
						reason: `Declared file ${file} does not exist.`,
						fix: 'Create the file or remove the path from the proposal.',
					});
				}
			}
		}
	}

	if (violations.length === 0) {
		process.stdout.write('visible-routes: every `product` proposal routes through game/godot/.\n');
		return 0;
	}
	for (const violation of violations) {
		process.stderr.write(`- ${violation.id}: ${violation.reason}\n  ${violation.path}\n  fix: ${violation.fix}\n`);
	}
	process.exit(1);
};

if (import.meta.main) process.exit(main());
