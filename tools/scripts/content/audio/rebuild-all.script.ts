#!/usr/bin/env bun
/**
 * Rebuild all audio slots end-to-end.
 *
 * Usage:
 *   bun tools/scripts/content/audio/rebuild-all.script.ts --clean
 *   bun tools/scripts/content/audio/rebuild-all.script.ts --kind=music
 *   bun tools/scripts/content/audio/rebuild-all.script.ts --group=music.menu
 *   bun tools/scripts/content/audio/rebuild-all.script.ts --limit=3
 *
 * Workflow:
 *   1. Optionally wipe staged files + status entries (--clean)
 *   2. Stage every selected slot through generate.script.ts
 *   3. Precompress staged mp3/wav to ogg via precompress.script.ts
 *   4. Rebuild REVIEW-CHECKLIST.md
 *
 * This script is intentionally thin orchestration around the existing
 * single-slot commands so all validation and state transitions stay in one
 * place.
 */

import { existsSync, readdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

import { readAudioSpec } from '../../../helpers/content/audio/spec.helper.ts';
import { workspaceRoot } from '../../workspace/root.script.ts';

const ARGS = new Map<string, string>();
for (const arg of Bun.argv.slice(2)) {
  const [k, v] = arg.replace(/^--/, '').split('=');
  ARGS.set(k, v ?? 'true');
}

if (ARGS.has('help') || ARGS.has('h')) {
  process.stdout.write(`Usage: bun ${import.meta.file} [options]

  --clean          remove staged generated files and clear spec.status first
  --kind=<kind>    music | sfx
  --group=<id>     e.g. music.menu
  --limit=<n>      only rebuild the first N selected slots
  --help, -h
`);
  process.exit(0);
}

const specPath = resolve(workspaceRoot, 'game/content/audio/audio-spec.json');
const stagingRoot = resolve(workspaceRoot, '.generate/audios/staging');
const CLEAN = ARGS.has('clean');
const KIND = ARGS.get('kind');
const GROUP = ARGS.get('group');
const LIMIT = ARGS.get('limit') ? Number(ARGS.get('limit')) : null;

const run = (args: string[]): { ok: boolean; stdout: string; stderr: string } => {
  const r = spawnSync('bun', args, { cwd: workspaceRoot, encoding: 'utf8' });
  return { ok: (r.status ?? 1) === 0, stdout: r.stdout ?? '', stderr: r.stderr ?? '' };
};

const runMmxProbe = (): { ok: boolean; message: string } => {
  const r = spawnSync(
    'mmx',
    [
      'music',
      'generate',
      '--model',
      'music-3.0',
      '--instrumental',
      '--prompt',
      'short retro loop, fm piano, 100 bpm',
      '--format',
      'mp3',
      '--out',
      '/tmp/pcdynasty-mmx-probe.mp3',
    ],
    { cwd: workspaceRoot, encoding: 'utf8' },
  );
  const combined = `${r.stdout ?? ''}\n${r.stderr ?? ''}`;
  if ((r.status ?? 1) === 0) {
    return { ok: true, message: 'mmx music probe ok' };
  }
  if (combined.includes('Music API is no longer available to new users')) {
    return {
      ok: false,
      message:
        'mmx auth is valid, but this account is not entitled to the legacy Music API. The provider returns HTTP 410. Do not wipe staging/status and expect rebuild to work until the pipeline is migrated to MiniMax Audio or another backend.',
    };
  }
  if (combined.includes('Token Plan usage limit reached')) {
    return {
      ok: false,
      message:
        'mmx auth is valid, but the current Token Plan is exhausted for music generation. Wait for quota reset or top up before running rebuild-all.',
    };
  }
  return {
    ok: false,
    message: `mmx probe failed before rebuild-all: ${combined.trim()}`,
  };
};

const probe = runMmxProbe();
if (!probe.ok) {
  process.stderr.write(`${probe.message}\n`);
  process.exit(1);
}

if (CLEAN) {
  if (existsSync(stagingRoot)) {
    for (const child of readdirSync(stagingRoot)) {
      rmSync(join(stagingRoot, child), { recursive: true, force: true });
    }
  }
  const raw = JSON.parse(readFileSync(specPath, 'utf8')) as Record<string, unknown>;
  raw.status = {};
  writeFileSync(specPath, JSON.stringify(raw, null, 2) + '\n');
  process.stdout.write(`✓ cleaned staging + cleared spec.status\n`);
}

const spec = readAudioSpec(workspaceRoot);
const slots = spec.groups
  .filter((g) => (KIND ? g.kind === KIND : true))
  .filter((g) => (GROUP ? g.id === GROUP : true))
  .flatMap((g) => g.entries.map((e) => `${g.id}.${e.id}`));
const selected = LIMIT !== null ? slots.slice(0, LIMIT) : slots;

if (selected.length === 0) {
  process.stderr.write('No slots selected. Check --kind/--group/--limit.\n');
  process.exit(1);
}

process.stdout.write(`Rebuilding ${selected.length} audio slots...\n`);
let ok = 0;
let failed = 0;
for (const slot of selected) {
  process.stdout.write(`\n=== ${slot} ===\n`);
  const result = run(['tools/scripts/content/audio/generate.script.ts', 'stage', slot]);
  if (!result.ok) {
    failed++;
    process.stderr.write(result.stderr || result.stdout || `stage failed: ${slot}\n`);
    continue;
  }
  ok++;
  process.stdout.write(result.stdout);
}

process.stdout.write(`\nStage done: ${ok} ok, ${failed} failed\n`);
if (failed > 0) {
  process.stderr.write('Stopping before precompress because at least one stage failed.\n');
  process.exit(1);
}

const pre = run(['tools/scripts/content/audio/precompress.script.ts']);
if (!pre.ok) {
  process.stderr.write(pre.stderr || pre.stdout);
  process.exit(1);
}
process.stdout.write(pre.stdout);

const check = run(['tools/scripts/content/audio/checklist.script.ts']);
if (!check.ok) {
  process.stderr.write(check.stderr || check.stdout);
  process.exit(1);
}
process.stdout.write(check.stdout);

process.stdout.write('\n✓ audio rebuild finished\n');
