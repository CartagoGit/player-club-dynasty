#!/usr/bin/env bun
/**
 * Product gate: a proposal that promises the player something **visible**
 * must name at least one file inside the Godot presentation project, so the
 * capability has a route from `game/godot/Scenes/Main.tscn`.
 *
 * Audit `a00001` found eleven modules that are green in tests and completely
 * unreachable from the executable. Unit tests cannot catch that: the slice
 * passes, the proposal closes, and the player still sees a disabled button.
 * This check closes the loop at the planning level — the cheapest place to
 * catch it.
 *
 * The rule is deliberately declarative and narrow:
 *
 *   `track: product` + visibility vocabulary in the body
 *      => at least one `game/godot/**` file among the slice `Files`
 *      => and, once the proposal is `done`, that file must exist on disk.
 *
 * Infrastructure, tooling and audit tracks are exempt: they legitimately
 * talk about Godot (CI provisioning, smoke harnesses) without shipping a
 * screen. Proposals listed in the baseline are waived — they closed before
 * the gate existed and are owned by the repair portfolio `q00001`.
 *
 * Usage:
 *   bun scripts/check-visible-capability-routes.script.ts
 */

import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

import { workspaceRoot } from './mcp-vertex-root.script.ts';

/** Directory of the Godot presentation project, relative to the workspace. */
export const PRESENTATION_ROOT = 'game/godot/';

/** Scene the executable boots into; the root of every visible route. */
export const MAIN_SCENE = 'game/godot/Scenes/Main.tscn';

/** Tracks whose proposals promise player-visible behaviour. */
export const PRODUCT_TRACKS: ReadonlySet<string> = new Set(['product', 'roadmap']);

/**
 * Kinds that ship player-facing behaviour. Everything else — plans, audits,
 * chores, infrastructure, docs, tests and spikes — legitimately discusses the
 * executable without owning a screen, so the gate leaves them alone.
 */
export const BEHAVIOUR_KINDS: ReadonlySet<string> = new Set([
	'feat',
	'fix',
	'refactor',
	'perf',
	'breaking',
]);

/**
 * Vocabulary that means "the player will see or do this". Kept lowercase and
 * matched case-insensitively against the proposal body.
 */
export const VISIBILITY_MARKERS: readonly string[] = [
	'godot',
	'smokeboot',
	'main.tscn',
	'screen',
	'workspace',
	'sidebar',
	'menu',
	'on screen',
	'visible',
	'navigat',
];

/** Statuses whose declared files must already exist on disk. */
const MATERIALISED_STATUSES: ReadonlySet<string> = new Set(['done']);

/** Folders that never hold gate-relevant proposals. */
const SKIPPED_FOLDERS: ReadonlySet<string> = new Set(['legacy', 'retired']);

const PROPOSAL_FILENAME = /^[a-z]\d{5}-[a-z0-9-]+\.md$/;

export interface IProposalRecord {
	/** Workspace-relative path of the proposal document. */
	readonly path: string;
	readonly id: string;
	readonly status: string;
	readonly kind: string;
	readonly track: string;
	/** Every path listed in a slice `Files` bullet, workspace-relative. */
	readonly files: readonly string[];
	readonly body: string;
}

export interface IRouteViolation {
	readonly id: string;
	readonly path: string;
	readonly reason: string;
	readonly fix: string;
}

const readFrontmatterField = (markdown: string, field: string): string => {
	const fenceEnd = markdown.indexOf('\n---', 3);
	const head = fenceEnd === -1 ? markdown.slice(0, 4096) : markdown.slice(0, fenceEnd);
	const match = new RegExp(`^${field}:\\s*"?([^"\\n]+)"?\\s*$`, 'm').exec(head);
	return match?.[1]?.trim() ?? '';
};

/** Collect every backticked path from the slice `Files` bullets. */
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

export const parseProposal = (path: string, markdown: string): IProposalRecord => ({
	path,
	id: readFrontmatterField(markdown, 'id'),
	status: readFrontmatterField(markdown, 'status'),
	kind: readFrontmatterField(markdown, 'kind'),
	track: readFrontmatterField(markdown, 'track'),
	files: parseDeclaredFiles(markdown),
	body: markdown,
});

/**
 * True when the document promises player-visible behaviour.
 *
 * Historical proposals closed before this gate existed are not re-judged:
 * a terminal proposal is only gated when it sits on a product track, while
 * every open proposal is gated whatever track it chose, so the rule cannot
 * be side-stepped by inventing a new track name for new work.
 */
export const declaresVisibleCapability = (proposal: IProposalRecord): boolean => {
	if (!BEHAVIOUR_KINDS.has(proposal.kind)) return false;
	if (MATERIALISED_STATUSES.has(proposal.status) && !PRODUCT_TRACKS.has(proposal.track))
		return false;
	const haystack = proposal.body.toLowerCase();
	return VISIBILITY_MARKERS.some((marker) => haystack.includes(marker));
};

const hasPresentationFile = (proposal: IProposalRecord): boolean =>
	proposal.files.some((file) => file.startsWith(PRESENTATION_ROOT));

/**
 * Evaluate one proposal. Returns `null` when it is exempt or compliant.
 * `fileExists` is injected so the rule stays pure and testable.
 */
export const checkProposal = (
	proposal: IProposalRecord,
	fileExists: (relativePath: string) => boolean,
): IRouteViolation | null => {
	if (!declaresVisibleCapability(proposal)) return null;

	if (!hasPresentationFile(proposal)) {
		return {
			id: proposal.id,
			path: proposal.path,
			reason: `declares a visible capability but names no file under ${PRESENTATION_ROOT}`,
			fix: `Add the presentation files that make the capability reachable from ${MAIN_SCENE}, or move the proposal off the product track.`,
		};
	}

	if (!MATERIALISED_STATUSES.has(proposal.status)) return null;

	const materialised = proposal.files
		.filter((file) => file.startsWith(PRESENTATION_ROOT))
		.some((file) => fileExists(file));
	if (materialised) return null;

	return {
		id: proposal.id,
		path: proposal.path,
		reason: 'is closed but none of its declared presentation files exist on disk',
		fix: 'Restore the presentation route or reopen the proposal; a closed slice cannot claim a capability the build does not contain.',
	};
};

const walkProposals = (root: string, workspace: string): string[] => {
	const out: string[] = [];
	for (const entry of readdirSync(root)) {
		const abs = join(root, entry);
		if (statSync(abs).isDirectory()) {
			if (SKIPPED_FOLDERS.has(entry)) continue;
			out.push(...walkProposals(abs, workspace));
			continue;
		}
		if (PROPOSAL_FILENAME.test(entry)) out.push(relative(workspace, abs));
	}
	return out;
};

export const loadBaseline = (workspace: string): ReadonlySet<string> => {
	const path = join(workspace, 'scripts', 'visible-capability-routes.baseline.json');
	if (!existsSync(path)) return new Set();
	const parsed: unknown = JSON.parse(readFileSync(path, 'utf8'));
	const waived =
		typeof parsed === 'object' && parsed !== null && 'waived' in parsed
			? (parsed as { waived?: unknown }).waived
			: undefined;
	return new Set(Array.isArray(waived) ? waived.filter((id): id is string => typeof id === 'string') : []);
};

export const checkWorkspace = (workspace: string): readonly IRouteViolation[] => {
	const proposalsRoot = join(workspace, 'docs', 'mcp-vertex', 'proposals');
	if (!existsSync(proposalsRoot)) return [];
	const waived = loadBaseline(workspace);
	const fileExists = (relativePath: string): boolean =>
		existsSync(join(workspace, relativePath));

	const violations: IRouteViolation[] = [];
	for (const path of walkProposals(proposalsRoot, workspace)) {
		const proposal = parseProposal(path, readFileSync(join(workspace, path), 'utf8'));
		if (waived.has(proposal.id)) continue;
		const violation = checkProposal(proposal, fileExists);
		if (violation !== null) violations.push(violation);
	}
	return violations.sort((a, b) => a.id.localeCompare(b.id));
};

// CLI ------------------------------------------------------------------------
if (import.meta.main) {
	if (!existsSync(join(workspaceRoot, MAIN_SCENE))) {
		process.stderr.write(`visible-capability-routes: ${MAIN_SCENE} is missing.\n`);
		process.exit(2);
	}
	const violations = checkWorkspace(workspaceRoot);
	for (const violation of violations) {
		process.stdout.write(`\nERROR ${violation.id} (${violation.path})\n`);
		process.stdout.write(`  ${violation.reason}\n`);
		process.stdout.write(`  fix: ${violation.fix}\n`);
	}
	process.stdout.write(
		`\nvisible-capability-routes: ${violations.length} violation(s).\n`,
	);
	process.exit(violations.length === 0 ? 0 : 1);
}
