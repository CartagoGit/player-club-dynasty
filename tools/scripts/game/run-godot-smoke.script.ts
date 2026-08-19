#!/usr/bin/env bun
/**
 * tools/scripts/game/run-godot-smoke.script.ts
 *
 * Build the Godot presentation and run the SmokeBoot scene headlessly.
 *
 * Steps:
 *   1. Resolve Godot (env, symlink, PATH, or download if `--provision`).
 *   2. Refresh `.tools/godot/current` so other tools find the same binary.
 *   3. Run `godot --headless --import` to populate `.godot/`.
 *   4. Build the presentation in Debug and Release (Godot loads Debug at
 *      runtime, the exporter uses Release).
 *   5. Smoke: `godot --headless --script res://Tests/SmokeBoot.gd`, with
 *      output teed to a tmp log that is grepped for engine errors.
 *
 * Usage:
 *   bun tools/scripts/game/run-godot-smoke.script.ts              # installed
 *   bun tools/scripts/game/run-godot-smoke.script.ts --provision  # download
 *   PCDYNASTY_PROVISION_GODOT=1 bun tools/scripts/game/run-godot-smoke.script.ts
 */

import { PCDYNASTY_ALLOW_MULTIPLE_ENV_VAR, GODOT_SMOKE_ERROR_REGEX } from '../../contracts/constants/engine/godot.constants.ts';
import { obtainGodot, refreshCurrentSymlink } from '../../helpers/engine/godot.helper.ts';
import { resolveProjectPaths } from '../../helpers/system/path.helper.ts';
import { disposeLog, logMatches, runInherit, runTee } from '../../helpers/system/run.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);
const argv = process.argv.slice(2);
const provision =
	argv.includes('--provision') || process.env.PCDYNASTY_PROVISION_GODOT === '1';

const main = async (): Promise<number> => {
	let godot;
	try {
		godot = await obtainGodot({
			workspaceRoot: paths.workspaceRoot,
			toolsDir: paths.toolsDir,
			autoDownload: provision,
		});
	} catch (err) {
		process.stderr.write(`run-godot-smoke: ${(err as Error).message}\n`);
		return 1;
	}
	process.stderr.write(`run-godot-smoke: using ${godot.path}\n`);

	refreshCurrentSymlink({ workspaceRoot: paths.workspaceRoot, toolsDir: paths.toolsDir }, godot);

	const importCode = await runInherit(godot.path, ['--headless', '--path', paths.projectDir, '--import']);
	if (importCode !== 0) {
		process.stderr.write(`run-godot-smoke: godot --import failed with exit code ${importCode}.\n`);
		return importCode;
	}

	for (const configuration of ['Debug', 'Release']) {
		process.stderr.write(`run-godot-smoke: building ${configuration}...\n`);
		const buildCode = await runInherit('dotnet', ['build', paths.csproj, '-c', configuration, '--nologo']);
		if (buildCode !== 0) {
			process.stderr.write(`run-godot-smoke: ${configuration} build failed with exit code ${buildCode}.\n`);
			return buildCode;
		}
	}

	const { exitCode, logPath } = await runTee(
		godot.path,
		['--headless', '--path', paths.projectDir, '--script', 'res://Tests/SmokeBoot.gd'],
		{
			env: { [PCDYNASTY_ALLOW_MULTIPLE_ENV_VAR]: '1' },
		},
	);
	try {
		if (exitCode !== 0) {
			process.stderr.write(`run-godot-smoke: SmokeBoot failed with exit code ${exitCode}.\n`);
			return exitCode;
		}
		if (logMatches(logPath, GODOT_SMOKE_ERROR_REGEX)) {
			process.stderr.write('run-godot-smoke: SmokeBoot reported engine errors.\n');
			return 1;
		}
	} finally {
		disposeLog(logPath);
	}
	process.stderr.write('run-godot-smoke: SmokeBoot passed.\n');
	return 0;
};

process.exit(await main());
