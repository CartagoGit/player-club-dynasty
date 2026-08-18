import { describe, expect, it } from 'vitest';

import {
	compare,
	parseBudgets,
	parseReport,
	type IBudget,
} from './compare-benchmark-budgets.script.ts';

const budget = (
	id: string,
	benchmark: string,
	maxMilliseconds: number,
	operations = 1,
): IBudget => ({
	id,
	benchmark,
	maxMilliseconds,
	operations,
	source: 'spec',
});

describe('parseBudgets', () => {
	it('reads simulation workloads and interaction budgets alike', () => {
		const raw = JSON.stringify({
			workloads: [
				{ id: 'macro', benchmark: 'Ns.Class.Macro', maxMedianMilliseconds: 50 },
				{ id: 'unmeasured', maxMedianMilliseconds: 10 },
			],
			budgets: [{ id: 'frame', benchmark: 'Ns.Class.Frame', maxMilliseconds: 8 }],
		});

		expect(parseBudgets(raw, 'spec')).toEqual([
			{ id: 'macro', benchmark: 'Ns.Class.Macro', maxMilliseconds: 50, operations: 1, source: 'spec' },
			{ id: 'frame', benchmark: 'Ns.Class.Frame', maxMilliseconds: 8, operations: 1, source: 'spec' },
		]);
	});
});

describe('parseReport', () => {
	it('converts the median from nanoseconds to milliseconds', () => {
		const raw = JSON.stringify({
			Benchmarks: [
				{ FullName: 'Ns.Class.Method', Statistics: { Median: 2_500_000 } },
				{ FullName: 'Ns.Class.NoStats' },
			],
		});

		expect(parseReport(raw)).toEqual([
			{ benchmark: 'Ns.Class.Method', medianMilliseconds: 2.5 },
		]);
	});
});

describe('compare', () => {
	it('fails a benchmark that is slower than its budget', () => {
		const result = compare(
			[budget('frame', 'Ns.Class.Frame', 8)],
			[{ benchmark: 'Ns.Class.Frame', medianMilliseconds: 12 }],
		);

		expect(result.failures).toHaveLength(1);
		expect(result.failures[0]).toContain('budget is 8 ms');
	});

	it('fails a benchmark that has no published budget', () => {
		const result = compare([], [{ benchmark: 'Ns.Class.Orphan', medianMilliseconds: 1 }]);

		expect(result.failures[0]).toContain('no published budget');
	});

	it('applies the strictest budget when several claim the same benchmark', () => {
		const result = compare(
			[budget('loose', 'Ns.Class.Shared', 50), budget('strict', 'Ns.Class.Shared', 16)],
			[{ benchmark: 'Ns.Class.Shared', medianMilliseconds: 20 }],
		);

		expect(result.failures).toHaveLength(1);
		expect(result.failures[0]).toContain('strict');
	});

	it('reports an unmeasured budget as uncovered rather than as a failure', () => {
		const result = compare([budget('missing', 'Ns.Class.Missing', 10)], []);

		expect(result.failures).toHaveLength(0);
		expect(result.uncovered[0]).toContain('never measured');
	});

	it('compares a per-operation median against the batch the budget is written for', () => {
		const result = compare(
			[budget('batch', 'Ns.Class.Batch', 100, 32)],
			[{ benchmark: 'Ns.Class.Batch', medianMilliseconds: 0.055 }],
		);

		expect(result.failures).toHaveLength(0);
		expect(result.checked[0]).toContain('1.760 ms of 100 ms over 32 operations');
	});

	it('fails a batch whose per-operation median exceeds the batch budget', () => {
		const result = compare(
			[budget('batch', 'Ns.Class.Batch', 100, 32)],
			[{ benchmark: 'Ns.Class.Batch', medianMilliseconds: 4 }],
		);

		expect(result.failures[0]).toContain('128.000 ms over 32 operations');
	});

	it('passes a benchmark inside its budget', () => {
		const result = compare(
			[budget('frame', 'Ns.Class.Frame', 8)],
			[{ benchmark: 'Ns.Class.Frame', medianMilliseconds: 0.011 }],
		);

		expect(result.failures).toHaveLength(0);
		expect(result.checked[0]).toContain('frame');
	});
});
