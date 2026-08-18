#!/usr/bin/env bun
/**
 * scripts/game-doctor.script.ts
 *
 * Diagnostics for the Godot/.NET presentation. Reports:
 *   - .NET SDK location and version
 *   - Available .NET 8 runtime
 *   - Godot binary path and version
 *   - Whether the Godot build is the Mono variant (needed for C#)
 *   - `.tools/godot/current` symlink target
 *
 * Usage:
 *   bun scripts/game-doctor.script.ts            # human-readable report
 *   bun scripts/game-doctor.script.ts --resolve  # JSON for other scripts
 */

import { spawnSync } from 'node:child_process';
import { existsSync, readlinkSync, statSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

const scriptDir = import.meta.dirname;
const gameRoot = resolve(scriptDir, '..', 'game');
const repositoryRoot = resolve(gameRoot, '..');
const projectDir = join(gameRoot, 'godot');

function isExecutable(path: string): boolean {
  try {
    const stat = statSync(path);
    return stat.isFile() && (stat.mode & 0o111) !== 0;
  } catch {
    return false;
  }
}

function locateOnPath(name: string): string | null {
  for (const dir of (process.env.PATH ?? '').split(':')) {
    if (!dir) continue;
    if (isExecutable(join(dir, name))) return join(dir, name);
  }
  return null;
}

function findDotnet(): string | null {
  if (
    process.env.DOTNET_ROOT &&
    isExecutable(join(process.env.DOTNET_ROOT, 'dotnet'))
  ) {
    return join(process.env.DOTNET_ROOT, 'dotnet');
  }
  const home = join(process.env.HOME ?? '', '.dotnet/dotnet');
  if (isExecutable(home)) return home;
  return locateOnPath('dotnet');
}

function findGodot(): string | null {
  if (process.env.GODOT_BIN && isExecutable(process.env.GODOT_BIN)) {
    return process.env.GODOT_BIN;
  }
  const current = join(repositoryRoot, '.tools/godot/current');
  if (existsSync(current)) {
    try {
      const target = readlinkSync(current);
      const resolved = resolve(dirname(current), target);
      if (isExecutable(resolved)) return resolved;
    } catch {
      if (isExecutable(current)) return current;
    }
  }
  return locateOnPath('godot') ?? locateOnPath('godot4');
}

function main(): number {
  const args = process.argv.slice(2);
  if (args.includes('--resolve')) {
    process.stdout.write(
      `${JSON.stringify(
        {
          dotnet: findDotnet(),
          godot: findGodot(),
          projectDir,
          repositoryRoot,
        },
        null,
        2,
      )}\n`,
    );
    return 0;
  }

  process.stdout.write('PC Dynasty — environment diagnostics\n');
  process.stdout.write('=====================================\n\n');

  const dotnet = findDotnet();
  if (dotnet) {
    process.stdout.write(`dotnet: ${dotnet}\n`);
    const info = spawnSync(dotnet, ['--info'], { encoding: 'utf8' });
    const versionMatch = info.stdout.match(/Version:\s+([^\n]+)/);
    if (versionMatch) {
      process.stdout.write(`  version: ${versionMatch[1].trim()}\n`);
    }
    const runtimes = spawnSync(dotnet, ['--list-runtimes'], { encoding: 'utf8' });
    const has8 = runtimes.stdout
      .split('\n')
      .some((line) => line.startsWith('Microsoft.NETCore.App 8.'));
    process.stdout.write(`  .NET 8 runtime: ${has8 ? 'present' : 'MISSING'}\n`);
  } else {
    process.stdout.write('dotnet: NOT FOUND (set DOTNET_ROOT or install .NET 8 SDK)\n');
  }

  process.stdout.write('\n');

  const godot = findGodot();
  if (godot) {
    process.stdout.write(`godot: ${godot}\n`);
    const version = spawnSync(godot, ['--version'], { encoding: 'utf8' });
    if (version.stdout.trim()) {
      process.stdout.write(`  version: ${version.stdout.trim()}\n`);
    }
    const isMono = godot.includes('mono');
    process.stdout.write(
      `  flavor: ${isMono ? 'mono (C# supported)' : 'standard (no C# support)'}\n`,
    );
  } else {
    process.stdout.write(
      'godot: NOT FOUND (set GODOT_BIN or install Godot 4.7.1 mono)\n',
    );
  }

  process.stdout.write('\nProject:\n');
  process.stdout.write(`  game root: ${gameRoot}\n`);
  process.stdout.write(`  project:   ${projectDir}\n`);
  process.stdout.write(`  config:    ${join(projectDir, 'project.godot')}\n`);

  return 0;
}

process.exit(main());
