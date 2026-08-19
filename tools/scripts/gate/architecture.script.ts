#!/usr/bin/env bun
/**
 * tools/scripts/gate/architecture.script.ts
 *
 * Replaces `game/scripts/check-architecture.sh`. Runs the
 * architecture-rules test project that enforces dependency boundaries.
 *
 * Usage:
 *   bun tools/scripts/gate/architecture.script.ts
 */

import { resolveProjectPaths } from '../../helpers/system/path.helper.ts';
import { runInherit } from '../../helpers/system/run.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

const exitCode = await runInherit('dotnet', [
	'test',
	paths.architectureTestProject,
	'--configuration',
	'Release',
	...process.argv.slice(2),
]);
process.exit(exitCode);
