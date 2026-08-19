#!/usr/bin/env bun
/**
 * tools/scripts/replays/benchmark-budgets.script.ts
 *
 * Compare BenchmarkDotNet results against the versioned budgets.
 *
 * CI used to *publish* benchmark reports and never compare them, so a
 * regression could ship as long as the run completed — audit `a00001` listed
 * that as one reason the performance obligations of the master plan drifted
 * out of reach. This comparison fails the build instead.
 *
 * Two rules, both derived from the budget files:
 *
 *   - a measured benchmark with no budget fails (nothing may be measured
 *     without a published threshold);
 *   - a measured benchmark slower than the strictest budget that claims it
 *     fails, reported in milliseconds.
 *
 * A budget's `operations` field is the batch size its threshold is written
 * for. BenchmarkDotNet reports a per-operation median whenever the benchmark
 * declares `OperationsPerInvoke`, so the comparison multiplies by it: a budget
 * of 100 ms for 32 contests is compared against 32 × the reported median. Keep
 * `operations` equal to the benchmark's `OperationsPerInvoke`, or 1.
 *
 * A budget with no measurement is reported as uncovered. That is the known
 * debt owned by `v00004`; pass `--require-complete-coverage` once every
 * mandatory workload exists, and the gap becomes an error too.
 *
 * Usage:
 *   bun tools/scripts/replays/benchmark-budgets.script.ts [--artifacts <dir>] [--require-complete-coverage]
 */

import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

import { workspaceRoot } from '../workspace/root.script.ts';

const NANOSECONDS_PER_MILLISECOND = 1_000_000;

/** Budget files, relative to the workspace. Both share the `benchmark` field. */
export const BUDGET_FILES: readonly string[] = [
	'game/benchmarks/budgets.json',
	'game/benchmarks/interaction-budgets.json',
];

export const DEFAULT_ARTIFACTS: readonly string[] = [
	'game/BenchmarkDotNet.Artifacts/results',
	'BenchmarkDotNet.Artifacts/results',
];

export interface IBudget {
	readonly id: string;
	readonly benchmark: string;
	readonly maxMilliseconds: number;
	/** Batch size the threshold is written for; 1 when the benchmark is per-invocation. */
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

/** Read every budget entry that names the benchmark measuring it. */
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

/** Read the medians out of one BenchmarkDotNet `-report-full.json`. */
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

/**
 * Compare measurements against budgets. Pure over its inputs so the rules are
 * testable without a benchmark run.
 */
export const compare = (
	budgets: readonly IBudget[],
	measurements: readonly IMeasurement[],
): IComparison => {
	const failures: string[] = [];
	const checked: string[] = [];
	const measured = new Set<string>();

	for (const measurement of measurements) {
		measured.add(measurement.benchmark);
		const applicable = budgets.filter((budget) => budget.benchmark === measurement.benchmark);
		if (applicable.length === 0) {
			failures.push(
				`${measurement.benchmark} ran at ${measurement.medianMilliseconds.toFixed(3)} ms with no published budget.`,
			);
			continue;
		}
		// Several budgets may claim the same benchmark; the strictest governs,
		// compared on the same batch size the threshold is written for.
		const strictest = applicable.reduce((left, right) =>
			right.maxMilliseconds / right.operations < left.maxMilliseconds / left.operations
				? right
				: left,
		);
		const elapsed = measurement.medianMilliseconds * strictest.operations;
		const batch = strictest.operations === 1 ? '' : ` over ${strictest.operations} operations`;
		checked.push(
			`${strictest.id}: ${elapsed.toFixed(3)} ms of ${strictest.maxMilliseconds} ms${batch}`,
		);
		if (elapsed > strictest.maxMilliseconds) {
			failures.push(
				`${strictest.id} (${measurement.benchmark}) took ${elapsed.toFixed(3)} ms${batch}; budget is ${strictest.maxMilliseconds} ms.`,
			);
		}
	}

	const uncovered = budgets
		.filter((budget) => !measured.has(budget.benchmark))
		.map((budget) => `${budget.id} (${budget.benchmark}) was never measured.`);

	return { failures, uncovered, checked };
};

const readBudgets = (workspace: string): readonly IBudget[] =>
	BUDGET_FILES.filter((file) => existsSync(join(workspace, file))).flatMap((file) =>
		parseBudgets(readFileSync(join(workspace, file), 'utf8'), file),
	);

const readMeasurements = (directory: string): readonly IMeasurement[] =>
	readdirSync(directory)
		.filter((entry) => entry.endsWith('-report-full.json'))
		.flatMap((entry) => parseReport(readFileSync(join(directory, entry), 'utf8')));

// CLI ------------------------------------------------------------------------
if (import.meta.main) {
	const args = process.argv.slice(2);
	const requireCompleteCoverage = args.includes('--require-complete-coverage');
	const explicitIndex = args.indexOf('--artifacts');
	const explicit = explicitIndex === -1 ? undefined : args[explicitIndex + 1];

	const directory = explicit
		? join(workspaceRoot, explicit)
		: DEFAULT_ARTIFACTS.map((candidate) => join(workspaceRoot, candidate)).find((candidate) =>
				existsSync(candidate),
			);

	if (directory === undefined || !existsSync(directory)) {
		process.stderr.write(
			'compare-benchmark-budgets: no BenchmarkDotNet results directory found. Run game/scripts/run-benchmarks.sh first.\n',
		);
		process.exit(2);
	}

	const result = compare(readBudgets(workspaceRoot), readMeasurements(directory));

	for (const line of result.checked) process.stdout.write(`  ok  ${line}\n`);
	for (const line of result.uncovered) process.stdout.write(`  --  ${line}\n`);
	for (const line of result.failures) process.stdout.write(`FAIL  ${line}\n`);

	const uncoveredFails = requireCompleteCoverage && result.uncovered.length > 0;
	process.stdout.write(
		`\ncompare-benchmark-budgets: ${result.checked.length} compared, ${result.uncovered.length} uncovered, ${result.failures.length} over budget.\n`,
	);
	process.exit(result.failures.length > 0 || uncoveredFails ? 1 : 0);
}
