#!/usr/bin/env bun
/**
 * tools/scripts/gate/tests.script.ts
 *
 * Replaces `game/scripts/test-all.sh`. Runs the engine-neutral C# suite
 * with `dotnet test` and forwards every extra argument as-is.
 *
 * Usage:
 *   bun tools/scripts/gate/tests.script.ts
 *   bun tools/scripts/gate/tests.script.ts --no-restore
 */

import { resolveProjectPaths } from '../../helpers/path.helper.ts';
import { runInherit } from '../../helpers/run.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

const exitCode = await runInherit('dotnet', [
	'test',
	paths.solution,
	'--configuration',
	'Release',
	...process.argv.slice(2),
]);
process.exit(exitCode);
