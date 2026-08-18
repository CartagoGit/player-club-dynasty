#!/usr/bin/env bun
/**
 * tools/scripts/game/doctor.script.ts
 *
 * Diagnostics for the Godot/.NET presentation. Read-only — never touches
 * the lock, the symlink, or any project file.
 *
 * Usage:
 *   bun tools/scripts/game/doctor.script.ts            # human-readable
 *   bun tools/scripts/game/doctor.script.ts --resolve  # JSON for chains
 */

import { PCDYNASTY_TOOLS_DIR_ENV_VAR } from '../../contracts/constants/godot.constants.ts';
import { GODOT_VERSION } from '../../contracts/constants/godot.constants.ts';
import { locateDotnet, probeDotnet8Sync } from '../../helpers/dotnet.helper.ts';
import { godotVersion, locateGodot } from '../../helpers/godot.helper.ts';
import { resolveProjectPaths } from '../../helpers/path.helper.ts';
import { runSync } from '../../helpers/process.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);

interface IResolved {
	readonly dotnet: string | null;
	readonly godot: string | null;
	readonly projectDir: string;
	readonly repositoryRoot: string;
	readonly toolsDir: string;
}

const collect = (): IResolved => {
	const dotnet = locateDotnet({ workspaceRoot: paths.workspaceRoot });
	const godot = locateGodot({ workspaceRoot: paths.workspaceRoot, toolsDir: paths.toolsDir });
	return {
		dotnet: dotnet?.bin ?? null,
		godot: godot?.path ?? null,
		projectDir: paths.projectDir,
		repositoryRoot: paths.workspaceRoot,
		toolsDir: paths.toolsDir,
	};
};

const main = (): number => {
	if (process.argv.includes('--resolve')) {
		process.stdout.write(`${JSON.stringify(collect(), null, 2)}\n`);
		return 0;
	}

	process.stdout.write('PC Dynasty — environment diagnostics\n');
	process.stdout.write('=====================================\n\n');

	const install = locateDotnet({ workspaceRoot: paths.workspaceRoot });
	if (install) {
		const verified = probeDotnet8Sync(install);
		process.stdout.write(`dotnet: ${verified.bin}\n`);
		process.stdout.write(`  version: ${verified.sdkVersion ?? 'unknown'}\n`);
		process.stdout.write(`  .NET 8 runtime: ${verified.has8Runtime ? 'present' : 'MISSING'}\n`);
	} else {
		process.stdout.write('dotnet: NOT FOUND (set DOTNET_ROOT or install .NET 8 SDK)\n');
	}

	process.stdout.write('\n');

	const godot = locateGodot({ workspaceRoot: paths.workspaceRoot, toolsDir: paths.toolsDir });
	if (godot) {
		process.stdout.write(`godot: ${godot.path}\n`);
		const version = godotVersion(godot);
		if (version) process.stdout.write(`  version: ${version}\n`);
		process.stdout.write(`  flavour: ${godot.isMono ? 'mono (C# supported)' : 'standard'}\n`);
	} else {
		process.stdout.write(`godot: NOT FOUND (set GODOT_BIN or install Godot ${GODOT_VERSION} mono)\n`);
	}

	process.stdout.write('\nProject:\n');
	process.stdout.write(`  workspace: ${paths.workspaceRoot}\n`);
	process.stdout.write(`  game root: ${paths.gameRoot}\n`);
	process.stdout.write(`  project:   ${paths.projectDir}\n`);
	process.stdout.write(`  config:    ${paths.projectDir}/project.godot\n`);
	process.stdout.write(`  tools dir: ${paths.toolsDir} (override: ${PCDYNASTY_TOOLS_DIR_ENV_VAR})\n`);

	return 0;
};

process.exit(main());
