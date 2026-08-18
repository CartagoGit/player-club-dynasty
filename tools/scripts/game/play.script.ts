#!/usr/bin/env bun
/**
 * tools/scripts/game/play.script.ts
 *
 * Launch PC Dynasty's Godot presentation. Thin orchestrator on top of
 * the helpers — every meaningful action is delegated.
 *
 * Usage:
 *   bun tools/scripts/game/play.script.ts                  # launch the game
 *   bun tools/scripts/game/play.script.ts --editor         # open the editor
 *   bun tools/scripts/game/play.script.ts --headless --import
 *   bun tools/scripts/game/play.script.ts --quit-after 30
 */

import { spawn } from 'node:child_process';

import { PCDYNASTY_ALLOW_MULTIPLE_ENV_VAR } from '../../contracts/constants/godot.constants.ts';
import { acquireLock } from '../../helpers/lock.helper.ts';
import { resolveProjectPaths } from '../../helpers/path.helper.ts';
import { run } from '../../helpers/process.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

const main = async (): Promise<number> => {
	if (process.env[PCDYNASTY_ALLOW_MULTIPLE_ENV_VAR] !== '1') {
		const lock = acquireLock();
		if (!lock) {
			process.stderr.write('PC Dynasty is already running.\n');
			return 2;
		}
		// We intentionally do NOT release on early exit: the Godot
		// process inherits the lock's lifetime because the lock file
		// stays as a PID marker. The actual cleanup happens on next
		// launch when the owner is detected as dead.
	}

	const { locateDotnet, probeDotnet8, buildDotnetConfig } = await import('../../helpers/dotnet.helper.ts');
	const { locateGodot } = await import('../../helpers/godot.helper.ts');

	const install = locateDotnet({ workspaceRoot: paths.workspaceRoot });
	if (!install) {
		process.stderr.write('PC Dynasty requires the .NET 8 SDK. Install it or set DOTNET_ROOT.\n');
		return 1;
	}
	const verified = await probeDotnet8(install);
	if (!verified.has8Runtime) {
		process.stderr.write(`PC Dynasty requires the .NET 8 runtime; ${verified.root} does not provide it.\n`);
		return 1;
	}
	const config = buildDotnetConfig(verified);
	process.env.DOTNET_ROOT = verified.root;
	process.env.PATH = config.path;

	const godot = locateGodot({ workspaceRoot: paths.workspaceRoot, toolsDir: paths.toolsDir });
	if (!godot) {
		process.stderr.write('Godot 4.7.1 Mono was not found. Install it or set GODOT_BIN.\n');
		return 1;
	}

	const args = ['--path', paths.projectDir, ...process.argv.slice(2)];
	process.stderr.write(`play: ${godot.path} ${args.join(' ')}\n`);

	// Use the async `run` helper so a timeout can be re-implemented later
	// without surgery; the spawn is still synchronous-style for the user.
	const child = spawn(godot.path, args, { stdio: 'inherit', env: process.env });
	return new Promise<number>((resolveFn) => {
		child.on('close', (code) => resolveFn(code ?? 0));
	});
};

process.exit(await main());
