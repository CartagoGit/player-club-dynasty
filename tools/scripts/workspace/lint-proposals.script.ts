#!/usr/bin/env bun
/**
 * tools/scripts/workspace/lint-proposals.script.ts
 *
 * Lint **this workspace's** proposal store with the canonical
 * mcp-vertex scaffold linter. The canonical linter resolves its
 * proposals directory relative to the mcp-vertex checkout, so it
 * lints mcp-vertex's proposals and never ours. This wrapper imports
 * the same linter as a library and points it at ours.
 *
 * Migrated from `scripts/lint-proposals.script.ts`.
 *
 * Usage:
 *   bun tools/scripts/workspace/lint-proposals.script.ts
 */

import { join } from 'node:path';

import { resolveMcpVertexRoot, workspaceRoot } from './root.script.ts';

interface IDuplicateGroup {
	readonly id: string;
	readonly absPaths: readonly string[];
}

interface ILintSummary {
	readonly filesChecked: number;
	readonly legacySkipped: number;
	readonly fatalErrors: number;
	readonly duplicateIds: readonly IDuplicateGroup[];
	readonly ok: boolean;
}

type LintProposalsDir = (proposalsDirAbs: string) => Promise<ILintSummary>;

const LINTER_MODULE = 'tools/scripts/lint/proposals.script.ts';

const loadLinter = async (): Promise<LintProposalsDir> => {
	const root = resolveMcpVertexRoot();
	const module = (await import(join(root.path, LINTER_MODULE))) as {
		lintProposalsDir?: LintProposalsDir;
	};
	if (typeof module.lintProposalsDir !== 'function') {
		throw new Error(`${LINTER_MODULE} does not export lintProposalsDir.`);
	}
	return module.lintProposalsDir;
};

export const lintWorkspaceProposals = async (workspace: string = workspaceRoot): Promise<ILintSummary> => {
	const lintProposalsDir = await loadLinter();
	return lintProposalsDir(join(workspace, 'docs', 'mcp-vertex', 'proposals'));
};

if (import.meta.main) {
	const summary = await lintWorkspaceProposals();
	const duplicates =
		summary.duplicateIds.length === 0
			? 'no duplicate ids'
			: `${summary.duplicateIds.length} duplicate id(s)`;
	process.stdout.write(
		`\nlint-proposals: ${summary.filesChecked} file(s) checked, ${summary.legacySkipped} legacy skipped, ${summary.fatalErrors} fatal error(s), ${duplicates}.\n`,
	);
	process.exit(summary.ok ? 0 : 1);
}
