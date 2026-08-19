/**
 * tools/contracts/interfaces/engine/godot.interface.ts
 *
 * Outcome of `locateGodot()` plus the extra context the smoke script
 * needs to refresh the symlink and report the binary flavour.
 */

export interface IGodotBinary {
	/** Absolute path to the executable. */
	readonly path: string;
	/** Substring used to detect the C# flavour. */
	readonly isMono: boolean;
	/**
	 * Source that resolved this binary — useful for error messages.
	 * One of `'env'`, `'symlink'`, `'path'`, `'downloaded'`.
	 */
	readonly source: IGodotBinarySource;
}

export type IGodotBinarySource = 'env' | 'symlink' | 'path' | 'downloaded';

export interface IGodotLocatorInput {
	/** Workspace root that hosts the `.tools/godot/current` symlink. */
	readonly workspaceRoot: string;
	/** Override the `.tools` cache directory (env-derived). */
	readonly toolsDir?: string;
	/**
	 * When `true`, the helper downloads the pinned Godot if every other
	 * lookup fails. The smoke script sets this through `--provision`.
	 */
	readonly autoDownload?: boolean;
}
