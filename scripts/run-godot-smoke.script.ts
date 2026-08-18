#!/usr/bin/env bun
/**
 * scripts/run-godot-smoke.script.ts
 *
 * Build the Godot presentation and run the SmokeBoot scene headlessly.
 * TypeScript replacement for `game/scripts/run-godot-smoke.sh`.
 *
 * Steps:
 *   1. Find Godot 4.7.1 Mono (env, .tools/godot/current, system, or
 *      download if `--provision` is set).
 *   2. Refresh the `.tools/godot/current` symlink so other tools
 *      (godot-mcp, etc.) find the same binary.
 *   3. Run `godot --headless --import` to populate `.godot/`.
 *   4. Build the presentation in Debug and Release (both required
 *      because Godot loads Debug at runtime and the exporter uses
 *      Release).
 *   5. Smoke: `godot --headless --script res://Tests/SmokeBoot.gd`.
 *      Output is streamed to the terminal AND captured to a tmp log so
 *      we can grep for engine errors (SCRIPT ERROR / ERROR: / USER
 *      ERROR) that might exit with code 0.
 *
 * Usage:
 *   bun scripts/run-godot-smoke.script.ts              # use installed Godot
 *   bun scripts/run-godot-smoke.script.ts --provision  # download if missing
 *   PCDYNASTY_PROVISION_GODOT=1 bun scripts/run-godot-smoke.script.ts
 *   PCDYNASTY_TOOLS_DIR=/alt/cache bun scripts/run-godot-smoke.script.ts
 */

import { spawn, spawnSync } from 'node:child_process';
import {
  chmodSync,
  closeSync,
  existsSync,
  mkdirSync,
  openSync,
  readFileSync,
  rmSync,
  statSync,
  symlinkSync,
  unlinkSync,
  writeSync,
} from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';

const scriptDir = import.meta.dirname;
const gameDir = resolve(scriptDir, '..', 'game');
const repositoryRoot = resolve(gameDir, '..');
const projectDir = join(gameDir, 'godot');
const csprojFile = join(projectDir, 'PCDynasty.Presentation.csproj');
const toolsDir = process.env.PCDYNASTY_TOOLS_DIR ?? join(repositoryRoot, '.tools');
const version = readFileSync(join(gameDir, 'godot.version'), 'utf8').trim();
const installDir = join(toolsDir, 'godot', version);

const argv = process.argv.slice(2);
const provision =
  argv.includes('--provision') || process.env.PCDYNASTY_PROVISION_GODOT === '1';

function platformArchive(): string {
  switch (process.platform) {
    case 'linux':
      return `Godot_v${version}_mono_linux_x86_64`;
    case 'darwin':
      return `Godot_v${version}_mono_macos.universal`;
    default:
      throw new Error(`No pinned Godot build for ${process.platform}.`);
  }
}

function platformBinaryRelative(): string {
  switch (process.platform) {
    case 'linux':
      return join(`Godot_v${version}_mono_linux_x86_64`, `Godot_v${version}_mono_linux.x86_64`);
    case 'darwin':
      return join(`Godot_v${version}_mono_macos.universal/Godot.app/Contents/MacOS/Godot`);
    default:
      throw new Error(`No pinned Godot binary for ${process.platform}.`);
  }
}

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

async function downloadGodot(): Promise<string> {
  const archive = platformArchive();
  const binary = join(installDir, platformBinaryRelative());
  if (isExecutable(binary)) return binary;

  mkdirSync(installDir, { recursive: true });
  const url = `https://github.com/godotengine/godot/releases/download/${version}/${archive}.zip`;
  const zipPath = join(installDir, `${archive}.zip`);
  process.stderr.write(`run-godot-smoke: downloading Godot ${version} ...\n`);

  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`HTTP ${response.status} ${response.statusText} for ${url}`);
  }
  await Bun.write(zipPath, response);

  const unzip = spawnSync('unzip', ['-q', '-o', zipPath, '-d', installDir], {
    stdio: 'inherit',
  });
  if ((unzip.status ?? 0) !== 0) {
    throw new Error('unzip failed; install it or run `apt install unzip`.');
  }
  rmSync(zipPath);
  chmodSync(binary, 0o755);
  if (!isExecutable(binary)) {
    throw new Error(`Downloaded binary not executable: ${binary}`);
  }
  return binary;
}

function findGodot(): string | null {
  if (process.env.GODOT_BIN && isExecutable(process.env.GODOT_BIN)) {
    return process.env.GODOT_BIN;
  }
  const cached = join(installDir, platformBinaryRelative());
  if (isExecutable(cached)) return cached;
  return locateOnPath('godot') ?? locateOnPath('godot4');
}

async function obtainGodot(): Promise<string> {
  const found = findGodot();
  if (found) return found;
  if (!provision) {
    throw new Error(
      `Godot ${version} Mono was not found. Install it, set GODOT_BIN, or pass --provision.`,
    );
  }
  return downloadGodot();
}

function refreshCurrentLink(godotBin: string): void {
  mkdirSync(join(toolsDir, 'godot'), { recursive: true });
  const linkPath = join(toolsDir, 'godot', 'current');
  try {
    unlinkSync(linkPath);
  } catch {
    // missing — fine
  }
  symlinkSync(godotBin, linkPath);
}

function runShell(cmd: string, args: string[]): Promise<number> {
  return new Promise((resolve, reject) => {
    const proc = spawn(cmd, args, { stdio: 'inherit' });
    proc.on('close', (code) => resolve(code ?? 0));
    proc.on('error', reject);
  });
}

async function runSmoke(godotBin: string): Promise<number> {
  const logPath = join(tmpdir(), `pcdynasty-smoke-${Date.now()}.log`);
  const logFd = openSync(logPath, 'w');
  const proc = spawn(
    godotBin,
    ['--headless', '--path', projectDir, '--script', 'res://Tests/SmokeBoot.gd'],
    {
      stdio: ['inherit', 'pipe', 'pipe'],
      env: { ...process.env, PCDYNASTY_ALLOW_MULTIPLE: '1' },
    },
  );
  proc.stdout.on('data', (chunk) => {
    process.stdout.write(chunk);
    writeSync(logFd, chunk);
  });
  proc.stderr.on('data', (chunk) => {
    process.stderr.write(chunk);
    writeSync(logFd, chunk);
  });
  const exitCode = await new Promise<number>((resolve) => {
    proc.on('close', (code) => resolve(code ?? 0));
  });
  closeSync(logFd);

  const logContent = readFileSync(logPath, 'utf8');
  rmSync(logPath, { force: true });

  if (exitCode !== 0) {
    process.stderr.write(`run-godot-smoke: SmokeBoot failed with exit code ${exitCode}.\n`);
    return exitCode;
  }
  if (/(SCRIPT ERROR|ERROR:|USER ERROR)/.test(logContent)) {
    process.stderr.write('run-godot-smoke: SmokeBoot reported engine errors.\n');
    return 1;
  }
  return 0;
}

async function main(): Promise<number> {
  let godotBin: string;
  try {
    godotBin = await obtainGodot();
  } catch (err) {
    process.stderr.write(`run-godot-smoke: ${(err as Error).message}\n`);
    return 1;
  }
  process.stderr.write(`run-godot-smoke: using ${godotBin}\n`);

  refreshCurrentLink(godotBin);

  const importCode = await runShell(godotBin, [
    '--headless',
    '--path',
    projectDir,
    '--import',
  ]);
  if (importCode !== 0) {
    process.stderr.write(
      `run-godot-smoke: godot --import failed with exit code ${importCode}.\n`,
    );
    return importCode;
  }

  for (const configuration of ['Debug', 'Release']) {
    process.stderr.write(`run-godot-smoke: building ${configuration}...\n`);
    const buildCode = await runShell('dotnet', [
      'build',
      csprojFile,
      '-c',
      configuration,
      '--nologo',
    ]);
    if (buildCode !== 0) {
      process.stderr.write(
        `run-godot-smoke: ${configuration} build failed with exit code ${buildCode}.\n`,
      );
      return buildCode;
    }
  }

  const smokeCode = await runSmoke(godotBin);
  if (smokeCode === 0) {
    process.stderr.write('run-godot-smoke: SmokeBoot passed.\n');
  }
  return smokeCode;
}

process.exit(await main());
