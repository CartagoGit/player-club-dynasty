/**
 * tools/contracts/interfaces/dotnet.interface.ts
 *
 * What `locateDotnet()` returns and what `ensureDotnet8()` consumes. The
 * helper layer is the only owner of these types — scripts never reach
 * into a bag of strings.
 */

export interface IDotnetInstall {
	/** Absolute path to the `dotnet` executable. */
	readonly bin: string;
	/** Absolute path to the directory containing the executable. */
	readonly root: string;
	/** Whether the dotnet at this path ships the .NET 8 runtime. */
	readonly has8Runtime: boolean;
	/** SDK version reported by `dotnet --info`, or `null` if it could not be parsed. */
	readonly sdkVersion: string | null;
}

export interface IDotnetConfig {
	/** Resolved install location. */
	readonly install: IDotnetInstall;
	/** Path list with `${root}` prepended so the next `spawn` finds the binary. */
	readonly path: string;
}

export interface IDotnetLocatorInput {
	/**
	 * Workspace root, used to resolve `<workspace>/.tools/dotnet/dotnet`.
	 * Defaults to `process.cwd()`.
	 */
	readonly workspaceRoot: string;
}
