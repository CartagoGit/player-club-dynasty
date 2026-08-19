#!/usr/bin/env bun
/**
 * tools/helpers/engine/godot.helper.ts
 *
 * Locate the Godot binary, refresh the canonical symlink, and download
 * the pinned editor when nothing is on disk. The smoke script and the
 * play script both depend on this helper.
 */

import { spawnSync } from 'node:child_process';
import { chmodSync, existsSync, mkdirSync, readFileSync, rmSync, symlinkSync, unlinkSync } from 'node:fs';
import { homedir } from 'node:os';
import { dirname, join } from 'node:path';

import {
	GODOT_BIN_ENV_VAR,
	GODOT_CURRENT_SYMLINK,
	GODOT_MONO_FLAVOUR_MARKER,
	GODOT_VERSION,
} from '../../contracts/constants/engine/godot.constants.ts';
import { GODOT_VERSION_FILE_RELATIVE } from '../../contracts/constants/system/fs.constants.ts';
import type { IGodotBinary, IGodotBinarySource, IGodotLocatorInput } from '../../contracts/interfaces/engine/godot.interface.ts';
import { isExecutable, readSymlinkTarget, readText } from '../system/fs.helper.ts';
import { resolveToolsDir } from '../system/path.helper.ts';

const versionFileFor = (workspaceRoot: string): string => join(workspaceRoot, GODOT_VERSION_FILE_RELATIVE);

const readPinnedVersion = (workspaceRoot: string): string => {
	try {
		return readText(versionFileFor(workspaceRoot)).trim();
	} catch {
		return GODOT_VERSION;
	}
};

const platformArchive = (version: string): string => {
	switch (process.platform) {
		case 'linux':
			return `Godot_v${version}_mono_linux_x86_64`;
		case 'darwin':
			return `Godot_v${version}_mono_macos.universal`;
		default:
			throw new Error(`No pinned Godot build for ${process.platform}.`);
	}
};

const platformBinaryRelative = (version: string): string => {
	switch (process.platform) {
		case 'linux':
			return join(`Godot_v${version}_mono_linux_x86_64`, `Godot_v${version}_mono_linux.x86_64`);
		case 'darwin':
			return join(`Godot_v${version}_mono_macos.universal/Godot.app/Contents/MacOS/Godot`);
		default:
			throw new Error(`No pinned Godot binary for ${process.platform}.`);
	}
};

const candidateFromPath = (): string | null => {
	for (const dir of (process.env.PATH ?? '').split(':')) {
		if (!dir) continue;
		const candidate = join(dir, 'godot');
		if (isExecutable(candidate)) return candidate;
		for (const alt of ['godot4']) {
			const altPath = join(dir, alt);
			if (isExecutable(altPath)) return altPath;
		}
	}
	return null;
};

/** Locate the Godot binary across the standard sources in priority order. */
export const locateGodot = (input: IGodotLocatorInput): IGodotBinary | null => {
	const envOverride = process.env[GODOT_BIN_ENV_VAR];
	if (envOverride && isExecutable(envOverride)) {
		return makeBinary(envOverride, 'env');
	}
	const toolsDir = input.toolsDir ?? resolveToolsDir(input.workspaceRoot);
	const symlinkPath = join(toolsDir, 'godot', GODOT_CURRENT_SYMLINK);
	const target = readSymlinkTarget(symlinkPath);
	if (target && isExecutable(target)) {
		return makeBinary(target, 'symlink');
	}
	const fromPath = candidateFromPath();
	if (fromPath) return makeBinary(fromPath, 'path');
	return null;
};

const makeBinary = (path: string, source: IGodotBinarySource): IGodotBinary => ({
	path,
	isMono: path.includes(GODOT_MONO_FLAVOUR_MARKER),
	source,
});

/** Refresh `.tools/godot/current` so other tools (godot-mcp, doctor) find the same binary. */
export const refreshCurrentSymlink = (input: IGodotLocatorInput, binary: IGodotBinary): void => {
	const toolsDir = input.toolsDir ?? resolveToolsDir(input.workspaceRoot);
	const godotToolsDir = join(toolsDir, 'godot');
	mkdirSync(godotToolsDir, { recursive: true });
	const linkPath = join(godotToolsDir, GODOT_CURRENT_SYMLINK);
	try {
		unlinkSync(linkPath);
	} catch {
		// Missing link is fine.
	}
	symlinkSync(binary.path, linkPath);
};

/** Download the pinned Godot build and return its absolute path. */
export const downloadGodot = async (input: IGodotLocatorInput): Promise<IGodotBinary> => {
	const version = readPinnedVersion(input.workspaceRoot);
	const toolsDir = input.toolsDir ?? resolveToolsDir(input.workspaceRoot);
	const installDir = join(toolsDir, 'godot', version);
	const archive = platformArchive(version);
	const binaryRelative = platformBinaryRelative(version);
	const binaryPath = join(installDir, binaryRelative);

	if (isExecutable(binaryPath)) return makeBinary(binaryPath, 'downloaded');

	mkdirSync(installDir, { recursive: true });
	const zipPath = join(installDir, `${archive}.zip`);
	const url = `https://github.com/godotengine/godot/releases/download/${version}/${archive}.zip`;

	const response = await fetch(url);
	if (!response.ok) {
		throw new Error(`HTTP ${response.status} ${response.statusText} for ${url}`);
	}
	await Bun.write(zipPath, response);

	const unzip = spawnSync('unzip', ['-q', '-o', zipPath, '-d', installDir], { stdio: 'inherit' });
	if ((unzip.status ?? 0) !== 0) {
		throw new Error('unzip failed; install it or run `apt install unzip`.');
	}
	rmSync(zipPath);
	chmodSync(binaryPath, 0o755);
	if (!isExecutable(binaryPath)) {
		throw new Error(`Downloaded binary not executable: ${binaryPath}`);
	}
	return makeBinary(binaryPath, 'downloaded');
};

/** Locate with an optional download fallback. */
export const obtainGodot = async (input: IGodotLocatorInput): Promise<IGodotBinary> => {
	const found = locateGodot(input);
	if (found) return found;
	if (!input.autoDownload) {
		throw new Error(
			`Godot 4.7.1 Mono was not found. Install it, set ${GODOT_BIN_ENV_VAR}, or pass --provision.`,
		);
	}
	return downloadGodot(input);
};

/** Read the version reported by the binary. Returns `null` when it cannot be parsed. */
export const godotVersion = (binary: IGodotBinary): string | null => {
	const result = spawnSync(binary.path, ['--version'], { encoding: 'utf8' });
	const text = (result.stdout ?? '').trim();
	return text.length > 0 ? text : null;
};

/** Read the version pin from the workspace. */
export const readWorkspaceVersion = (workspaceRoot: string): string => {
	const candidate = join(workspaceRoot, GODOT_VERSION_FILE_RELATIVE);
	if (existsSync(candidate)) return readFileSync(candidate, 'utf8').trim();
	return GODOT_VERSION;
};
