/**
 * tools/contracts/constants/dotnet.constants.ts
 *
 * Single source of truth for every .NET-related string the tool layer
 * depends on (commands, env vars, runtime prefix, error patterns). The
 * helpers consume these constants so the values are not duplicated across
 * scripts.
 */

/** Name of the `dotnet` executable the OS expects. */
export const DOTNET_EXECUTABLE = 'dotnet' as const;

/** Substring that must appear in `dotnet --list-runtimes` for the project to launch. */
export const DOTNET_RUNTIME_REQUIRED_PREFIX = 'Microsoft.NETCore.App 8.' as const;

/** Value `run --info` must show for the SDK to be considered the project's expected one. */
export const DOTNET_REQUIRED_SDK_MAJOR = 8 as const;

/** Environment variables that pin the .NET path. Helpers read them in this order. */
export const DOTNET_ENV_VARS = {
	root: 'DOTNET_ROOT',
	sdk: 'DOTNET_SDK_VERSION',
	toolsDir: 'PCDYNASTY_TOOLS_DIR',
} as const;
