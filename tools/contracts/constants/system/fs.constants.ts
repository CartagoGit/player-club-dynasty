/**
 * tools/contracts/constants/system/fs.constants.ts
 *
 * Filesystem constants — default directories, executable modes, build
 * artefact names. Keep this file free of logic.
 */

/** Default location of the Godot archive once expanded. */
export const TOOLS_GODOT_DIR = 'godot' as const;

/** Default workspace-relative location of the provisioned toolchains. */
export const TOOLS_DIR = '.tools' as const;

/** Default location of the Godot version pin. */
export const GODOT_VERSION_FILE_RELATIVE = 'game/godot.version' as const;

/** Default location of the Godot presentation project. */
export const GODOT_PROJECT_DIR_RELATIVE = 'game/godot' as const;

/** Default location of the C# presentation project within the Godot project. */
export const GODOT_PRESENTATION_CSPROJ = 'PCDynasty.Presentation.csproj' as const;

/** Default location of the engine-neutral C# solution. */
export const GAME_SOLUTION_FILE = 'PCDynasty.sln' as const;

/** Default location of the architecture-rules test project. */
export const ARCHITECTURE_TEST_PROJECT = 'tests/PCDynasty.Architecture.Tests/PCDynasty.Architecture.Tests.csproj' as const;

/** Default location of the BenchmarkDotNet project. */
export const BENCHMARK_PROJECT = 'benchmarks/PCDynasty.Benchmarks' as const;

/** Default location of the benchmark artefact directory. */
export const BENCHMARK_ARTIFACTS_DIR = 'BenchmarkDotNet.Artifacts' as const;

/** Default location of the benchmark context directory. */
export const BENCHMARK_CONTEXT_DIR = 'context' as const;

/** Budget files copied into the benchmark context before each run. */
export const BENCHMARK_BUDGET_FILES = {
	simulation: 'benchmarks/budgets.json',
	interaction: 'benchmarks/interaction-budgets.json',
} as const;

/** XDG executable bits, copied into `stat.S_IXUSR | stat.S_IXGRP | stat.S_IXOTH`. */
export const FILE_EXECUTABLE_MODE = 0o755 as const;

/** File mode used when writing text files that should not be marked executable. */
export const FILE_TEXT_MODE = 0o644 as const;
