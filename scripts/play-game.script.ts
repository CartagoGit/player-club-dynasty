#!/usr/bin/env bun
/**
 * scripts/play-game.script.ts
 *
 * Launch PC Dynasty's Godot presentation. TypeScript replacement for
 * `game/scripts/run-game.sh` so the launcher can be invoked without a
 * POSIX shell wrapper.
 *
 * Why TS instead of bash:
 *   - the bash version only does path discovery, env-var plumbing, and a
 *     single `exec(2)` for the Godot binary. All of that reads better
 *     in TS, and we get typed env, real exit codes, and a real lock.
 *   - Godot is spawned directly via `spawn()` — no shell, no
 *     `exec $godot --path …` indirection. The .NET 8 runtime is set up
 *     by exporting DOTNET_ROOT and prepending it to PATH so the Godot
 *     Mono host finds hostfxr the same way the bash launcher did.
 *
 * Resolution order for the Godot binary:
 *   1. `$GODOT_BIN` (must exist and be executable)
 *   2. `.tools/godot/current` symlink (kept current by the smoke script)
 *   3. `godot` or `godot4` on `$PATH`
 *
 * Lock: a PID file at `${XDG_RUNTIME_DIR:-/tmp}/pcdynasty-game.lock`
 * prevents two concurrent launches. `PCDYNASTY_ALLOW_MULTIPLE=1` bypasses
 * it (used by the smoke script for parallel CI).
 *
 * Usage:
 *   bun scripts/play-game.script.ts                     # launch the game
 *   bun scripts/play-game.script.ts --editor            # open the editor
 *   bun scripts/play-game.script.ts --headless --import # headless import
 *   bun scripts/play-game.script.ts --quit-after 30     # forward any flag
 */

import { spawn } from 'node:child_process';
import {
  closeSync,
  existsSync,
  mkdirSync,
  openSync,
  readFileSync,
  realpathSync,
  rmSync,
  statSync,
  writeSync,
} from 'node:fs';
import { dirname, join, resolve } from 'node:path';

const scriptDir = import.meta.dirname;
const gameRoot = resolve(scriptDir, '..', 'game');
const repositoryRoot = resolve(gameRoot, '..');
const projectDir = join(gameRoot, 'godot');

const ENV = process.env as Record<string, string | undefined>;

function isExecutable(path: string): boolean {
  try {
    const stat = statSync(path);
    return stat.isFile() && (stat.mode & 0o111) !== 0;
  } catch {
    return false;
  }
}

function locateOnPath(name: string): string | null {
  for (const dir of (ENV.PATH ?? '').split(':')) {
    if (!dir) continue;
    if (isExecutable(join(dir, name))) return join(dir, name);
  }
  return null;
}

function locateDotnet(): string | null {
  if (ENV.DOTNET_ROOT && isExecutable(join(ENV.DOTNET_ROOT, 'dotnet'))) {
    return join(ENV.DOTNET_ROOT, 'dotnet');
  }
  const candidates = [
    join(ENV.HOME ?? '', '.dotnet/dotnet'),
    join(repositoryRoot, '.tools/dotnet/dotnet'),
  ];
  for (const candidate of candidates) {
    if (isExecutable(candidate)) return candidate;
  }
  return locateOnPath('dotnet');
}

function listRuntimes(dotnetBin: string): Promise<string[]> {
  return new Promise((resolve, reject) => {
    const proc = spawn(dotnetBin, ['--list-runtimes']);
    let stdout = '';
    let stderr = '';
    proc.stdout.on('data', (c) => (stdout += c.toString()));
    proc.stderr.on('data', (c) => (stderr += c.toString()));
    proc.on('close', (code) => {
      if (code !== 0) {
        reject(new Error(`dotnet --list-runtimes failed: ${stderr.trim()}`));
        return;
      }
      resolve(stdout.split('\n').map((line) => line.trim()));
    });
  });
}

function locateGodot(): string | null {
  if (ENV.GODOT_BIN && isExecutable(ENV.GODOT_BIN)) {
    return realpathSync(ENV.GODOT_BIN);
  }
  const current = join(repositoryRoot, '.tools/godot/current');
  if (existsSync(current)) {
    try {
      const resolved = realpathSync(current);
      if (isExecutable(resolved)) return resolved;
    } catch {
      // broken symlink — fall through to PATH lookup
    }
  }
  return locateOnPath('godot') ?? locateOnPath('godot4');
}

function isPidAlive(pid: number): boolean {
  if (!Number.isInteger(pid) || pid <= 0) return false;
  try {
    process.kill(pid, 0);
    return true;
  } catch {
    return false;
  }
}

function acquireLock(lockPath: string): boolean {
  mkdirSync(dirname(lockPath), { recursive: true });
  try {
    const fd = openSync(lockPath, 'wx');
    writeSync(fd, `${process.pid}\n${Date.now()}\n`);
    closeSync(fd);
    return true;
  } catch (err: unknown) {
    if ((err as NodeJS.ErrnoException).code !== 'EEXIST') return false;
    try {
      const content = readFileSync(lockPath, 'utf8');
      const pid = parseInt(content.split('\n')[0] ?? '', 10);
      if (isPidAlive(pid)) return false;
      rmSync(lockPath);
      return acquireLock(lockPath);
    } catch {
      return false;
    }
  }
}

async function main(): Promise<number> {
  if (ENV.PCDYNASTY_ALLOW_MULTIPLE !== '1') {
    const runtimeDir = ENV.XDG_RUNTIME_DIR ?? '/tmp';
    if (!acquireLock(join(runtimeDir, 'pcdynasty-game.lock'))) {
      process.stderr.write('PC Dynasty is already running.\n');
      return 2;
    }
  }

  const dotnetBin = locateDotnet();
  if (!dotnetBin) {
    process.stderr.write(
      'PC Dynasty requires the .NET 8 SDK. Install it or set DOTNET_ROOT.\n',
    );
    return 1;
  }
  const dotnetRoot = dirname(realpathSync(dotnetBin));
  ENV.DOTNET_ROOT = dotnetRoot;
  if (!ENV.PATH?.startsWith(`${dotnetRoot}:`)) {
    ENV.PATH = `${dotnetRoot}:${ENV.PATH ?? ''}`;
  }

  try {
    const runtimes = await listRuntimes(dotnetBin);
    if (!runtimes.some((line) => line.startsWith('Microsoft.NETCore.App 8.'))) {
      process.stderr.write(
        `PC Dynasty requires the .NET 8 runtime; ${dotnetRoot} does not provide it.\n`,
      );
      return 1;
    }
  } catch (err) {
    process.stderr.write(`${(err as Error).message}\n`);
    return 1;
  }

  const godotBin = locateGodot();
  if (!godotBin) {
    process.stderr.write(
      'Godot 4.7.1 Mono was not found. Install it or set GODOT_BIN.\n',
    );
    return 1;
  }

  const godotArgs = ['--path', projectDir, ...process.argv.slice(2)];
  process.stderr.write(`play-game: ${godotBin} ${godotArgs.join(' ')}\n`);
  const proc = spawn(godotBin, godotArgs, { stdio: 'inherit', env: process.env });
  return await new Promise((resolve) => {
    proc.on('close', (code) => resolve(code ?? 0));
  });
}

process.exit(await main());
