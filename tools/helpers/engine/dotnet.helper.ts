#!/usr/bin/env bun
/**
 * tools/helpers/engine/dotnet.helper.ts
 *
 * Locate the .NET SDK, verify the .NET 8 runtime, and export the
 * environment the child Godot process needs.
 */

import { homedir } from 'node:os';
import { dirname, isAbsolute, join } from 'node:path';

import { DOTNET_ENV_VARS, DOTNET_EXECUTABLE, DOTNET_RUNTIME_REQUIRED_PREFIX } from '../../contracts/constants/engine/dotnet.constants.ts';
import { PATH_ENV_VAR, PATH_LIST_SEPARATOR } from '../../contracts/constants/system/env.constants.ts';
import type { IDotnetConfig, IDotnetInstall, IDotnetLocatorInput } from '../../contracts/interfaces/engine/dotnet.interface.ts';
import { isExecutable } from '../system/fs.helper.ts';
import { run, runSync } from '../system/process.helper.ts';

/** Look for `dotnet` in the user environment, falling back to standard locations. */
export const locateDotnet = (input: IDotnetLocatorInput = { workspaceRoot: process.cwd() }): IDotnetInstall | null => {
	const candidates: string[] = [];
	const envRoot = process.env[DOTNET_ENV_VARS.root];
	if (envRoot && envRoot.length > 0) {
		candidates.push(join(envRoot, DOTNET_EXECUTABLE));
	}
	candidates.push(DOTNET_EXECUTABLE);
	candidates.push(join(homedir(), '.dotnet', DOTNET_EXECUTABLE));
	candidates.push(join(input.workspaceRoot, '.tools', 'dotnet', DOTNET_EXECUTABLE));

	for (const candidate of candidates) {
		if (isExecutable(candidate)) {
			const root = isAbsolute(candidate) ? dirname(candidate) : dirname(candidate);
			return {
				bin: candidate,
				root,
				has8Runtime: false,
				sdkVersion: null,
			};
		}
	}
	return null;
};

/** Verify the .NET 8 runtime is registered for the resolved install. */
export const probeDotnet8 = async (install: IDotnetInstall): Promise<IDotnetInstall> => {
	const result = await run(install.bin, { args: ['--list-runtimes'] });
	if (result.exitCode !== 0) {
		throw new Error(`dotnet --list-runtimes failed: ${result.stderr.trim()}`);
	}
	const has8 = result.stdout.split('\n').some((line) => line.trim().startsWith(DOTNET_RUNTIME_REQUIRED_PREFIX));
	const info = await run(install.bin, { args: ['--info'] });
	const sdkMatch = /Version:\s+([^\n]+)/.exec(info.stdout);
	return {
		...install,
		has8Runtime: has8,
		sdkVersion: sdkMatch?.[1]?.trim() ?? null,
	};
};

/** Build the env vars needed to spawn a child process that finds hostfxr. */
export const buildDotnetConfig = (install: IDotnetInstall): IDotnetConfig => {
	const originalPath = process.env[PATH_ENV_VAR] ?? '';
	const path = originalPath.startsWith(install.root)
		? originalPath
		: `${install.root}${PATH_LIST_SEPARATOR}${originalPath}`;
	return {
		install,
		path,
	};
};

/** Synchronous sibling for CLI scripts that prefer `spawnSync`. */
export const locateDotnetSync = (input: IDotnetLocatorInput = { workspaceRoot: process.cwd() }): IDotnetInstall | null => {
	const placeholder = locateDotnet(input);
	return placeholder;
};

export const probeDotnet8Sync = (install: IDotnetInstall): IDotnetInstall => {
	const list = runSync(install.bin, ['--list-runtimes']);
	if (list.exitCode !== 0) {
		throw new Error(`dotnet --list-runtimes failed: ${list.stderr.trim()}`);
	}
	const has8 = list.stdout.split('\n').some((line) => line.trim().startsWith(DOTNET_RUNTIME_REQUIRED_PREFIX));
	const info = runSync(install.bin, ['--info']);
	const sdkMatch = /Version:\s+([^\n]+)/.exec(info.stdout);
	return {
		...install,
		has8Runtime: has8,
		sdkVersion: sdkMatch?.[1]?.trim() ?? null,
	};
};
