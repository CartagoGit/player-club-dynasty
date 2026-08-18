#!/usr/bin/env bun
/**
 * tools/scripts/localization/generate-locale.script.ts
 *
 * Generate the built-in locale JSON files from the C# records. The
 * C# code under game/src/PCDynasty.Presentation.Core/Localization/
 * is the single source of truth: this script is a thin wrapper that
 * invokes the PCDynasty.LocaleGenerator console app and lets the
 * generator validate every record before writing.
 *
 * Usage:
 *   bun tools/scripts/localization/generate-locale.script.ts
 *     → writes game/godot/locale/langs/built/{es,en}.json
 *
 *   bun tools/scripts/localization/generate-locale.script.ts --check
 *     → exit 1 when the on-disk JSON would change (CI guard).
 */

import { existsSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

const workspaceRoot = resolve(import.meta.dirname, '..', '..', '..');
const projectPath = join(workspaceRoot, 'tools', 'locale-generator', 'PCDynasty.LocaleGenerator.csproj');
const outputFolder = join(workspaceRoot, 'game', 'godot', 'locale', 'langs', 'built');
const args = new Set(process.argv.slice(2));
const check = args.has('--check');
const force = args.has('--force');

if (!existsSync(projectPath))
{
    process.stderr.write(`Locale generator project not found: ${projectPath}\n`);
    process.exit(2);
}

const dotnetArgs = ['run', '--project', projectPath, outputFolder];
if (check)
{
    dotnetArgs.push('--check');
}
else if (force)
{
    // Force overwrite is already the default for non-check runs; the
    // flag exists so CI scripts can express intent explicitly.
}

const result = spawnSync('dotnet', dotnetArgs, { stdio: 'inherit', cwd: workspaceRoot });
process.exit(result.status ?? 1);
