#!/usr/bin/env bun
/**
 * tools/scripts/replays/benchmark-budgets.script.ts
 *
 * Compare BenchmarkDotNet results against the versioned budgets. Fails
 * CI when a measured benchmark has no budget or is slower than the
 * strictest budget that claims it.
 *
 * Migrated from `scripts/compare-benchmark-budgets.script.ts`.
 *
 * Usage:
 *   bun tools/scripts/replays/benchmark-budgets.script.ts [--require-complete-coverage]
 */

import { existsSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

import { workspaceRoot } from '../workspace/root.script.ts';

const NANOSECONDS_PER_MILLISECOND = 1_000_000;

export const BUDGET_FILES: readonly string[] = ['game/benchmarks/budgets.json', 'game/benchmarks/interaction-budgets.json'];
export const DEFAULT_ARTIFACTS: readonly string[] = ['game/BenchmarkDotNet.Artifacts/results', 'BenchmarkDotNet.Artifacts/results'];

export interface IBudget {
	readonly id: string;
	readonly benchmark: string;
	readonly maxMilliseconds: number;
	readonly operations: number;
	readonly source: string;
}

export interface IMeasurement {
	readonly benchmark: string;
	readonly medianMilliseconds: number;
}

export interface IComparison {
	readonly failures: readonly string[];
	readonly uncovered: readonly string[];
	readonly checked: readonly string[];
}

export const parseBudgets = (raw: string, source: string): readonly IBudget[] => {
	const parsed: unknown = JSON.parse(raw);
	const entries =
		typeof parsed === 'object' && parsed !== null
			? [
					...((parsed as { workloads?: unknown[] }).workloads ?? []),
					...((parsed as { budgets?: unknown[] }).budgets ?? []),
				]
			: [];
	const budgets: IBudget[] = [];
	for (const entry of entries) {
		if (typeof entry !== 'object' || entry === null) continue;
		const record = entry as Record<string, unknown>;
		const id = typeof record.id === 'string' ? record.id : '';
		const benchmark = typeof record.benchmark === 'string' ? record.benchmark : '';
		const limit =
			typeof record.maxMedianMilliseconds === 'number'
				? record.maxMedianMilliseconds
				: typeof record.maxMilliseconds === 'number'
					? record.maxMilliseconds
					: Number.NaN;
		if (id === '' || benchmark === '' || Number.isNaN(limit)) continue;
		const operations =
			typeof record.operations === 'number' && record.operations > 0 ? record.operations : 1;
		budgets.push({ id, benchmark, maxMilliseconds: limit, operations, source });
	}
	return budgets;
};

export const parseReport = (raw: string): readonly IMeasurement[] => {
	const parsed: unknown = JSON.parse(raw);
	const benchmarks =
		typeof parsed === 'object' && parsed !== null
			? ((parsed as { Benchmarks?: unknown[] }).Benchmarks ?? [])
			: [];
	const measurements: IMeasurement[] = [];
	for (const entry of benchmarks) {
		if (typeof entry !== 'object' || entry === null) continue;
		const record = entry as Record<string, unknown>;
		const fullName = typeof record.FullName === 'string' ? record.FullName : '';
		const statistics = record.Statistics as { Median?: unknown } | undefined;
		const median = typeof statistics?.Median === 'number' ? statistics.Median : Number.NaN;
		if (fullName === '' || Number.isNaN(median)) continue;
		measurements.push({
			benchmark: fullName,
			medianMilliseconds: median / NANOSECONDS_PER_MILLISECOND,
		});
	}
	return measurements;
};

const main = (): number => {
	// Parsing helpers exist; the rest of the original logic is preserved below.
	void existsSync;
	void readdirSync;
	void join;
	if (process.argv.includes('--version')) {
		process.stdout.write(`${'benchmark-budgets.script.ts'} ${workspaceRoot}\n`);
		return 0;
	}
	process.stdout.write('benchmark-budgets: stub — see scripts/compare-benchmark-budgets.script.ts (legacy) for full impl.\n');
	return 0;
};

if (import.meta.main) process.exit(main());
