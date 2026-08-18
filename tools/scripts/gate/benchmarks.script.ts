#!/usr/bin/env bun
/**
 * tools/scripts/gate/benchmarks.script.ts
 *
 * Replaces `game/scripts/run-benchmarks.sh`. Stages the benchmark
 * context, then runs `dotnet run --project benchmarks/...`. Selects
 * every benchmark by default (the `*` filter) because with no TTY
 * BenchmarkDotNet would otherwise print the menu and exit 0 without
 * measuring anything.
 *
 * Usage:
 *   bun tools/scripts/gate/benchmarks.script.ts
 *   BENCHMARK_FILTER=MyBenchmark* bun tools/scripts/gate/benchmarks.script.ts
 */

import { readFileSync, writeFileSync } from 'node:fs';
import { mkdirSync } from 'node:fs';
import { dirname, join } from 'node:path';

import { BENCHMARK_BUDGET_FILES } from '../../contracts/constants/fs.constants.ts';
import { resolveProjectPaths } from '../../helpers/path.helper.ts';
import { runInherit } from '../../helpers/run.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

const contextDir = join(paths.benchmarkArtifactsDir, 'context');
mkdirSync(contextDir, { recursive: true });

const copyIntoContext = (source: string, dest: string): void => {
	const payload = readFileSync(join(paths.workspaceRoot, source), 'utf8');
	writeFileSync(dest, payload);
};

copyIntoContext(BENCHMARK_BUDGET_FILES.simulation, join(contextDir, 'simulation-budgets.json'));
copyIntoContext(BENCHMARK_BUDGET_FILES.interaction, join(contextDir, 'interaction-budgets.json'));

const head = (() => {
	try {
		return readFileSync(join(paths.workspaceRoot, '.git', 'HEAD'), 'utf8').trim();
	} catch {
		return 'unknown';
	}
})();
writeFileSync(join(contextDir, 'commit.txt'), head);

const dotnetInfo = (() => {
	try {
		// We just want the version; the helper already validated it.
		return readFileSync(join(paths.workspaceRoot, '.tools', 'dotnet-version.txt'), 'utf8');
	} catch {
		return 'unknown';
	}
})();
writeFileSync(join(contextDir, 'dotnet-info.txt'), dotnetInfo);

const benchmarkFilter = process.env.BENCHMARK_FILTER ?? '*';

const exitCode = await runInherit('dotnet', [
	'run',
	'--configuration',
	'Release',
	'--project',
	paths.benchmarkProject,
	'--',
	'--join',
	'--filter',
	benchmarkFilter,
	...process.argv.slice(2),
], { cwd: paths.gameRoot });
process.exit(exitCode);

void dirname;
