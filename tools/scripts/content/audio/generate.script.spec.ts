#!/usr/bin/env bun
/**
 * tools/scripts/content/audio/generate.script.spec.ts
 *
 * Validates the new variant-based audio workflow:
 *   - the runner rejects music groups whose bpm drifts from the anchor
 *   - validate() rejects orphan status slots
 *   - list/status commands never crash even when no variants exist
 *   - the runner refuses approve/deprecate/listen on a non-existent variant
 *   - the staging dir (.generate/audios/staging) and deprecation dir (.deprecated)
 *     are created on demand and stay isolated from git (via .gitignore)
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { spawnSync } from 'node:child_process';
import {
	existsSync,
	mkdtempSync,
	readFileSync,
	rmSync,
	writeFileSync,
} from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';

import { workspaceRoot } from '../../workspace/root.script';

const SPEC_RELATIVE = join('game', 'content', 'audio', 'audio-spec.json');
const SPEC_FULL = join(workspaceRoot, SPEC_RELATIVE);

interface IRunResult {
	stdout: string;
	stderr: string;
	exitCode: number;
}

const runCli = (args: readonly string[]): IRunResult => {
	const result = spawnSync('bun', ['tools/scripts/content/audio/generate.script.ts', ...args], {
		cwd: workspaceRoot,
		encoding: 'utf8',
	});
	return {
		stdout: result.stdout ?? '',
		stderr: result.stderr ?? '',
		exitCode: result.status ?? 1,
	};
};

describe('generate-audio.script.ts (variant workflow)', () => {
	let originalSpec: string | null = null;
	let workspaceBackup: string | null = null;
	let sandboxRoot: string | null = null;

	beforeEach(() => {
		originalSpec = existsSync(SPEC_FULL) ? readFileSync(SPEC_FULL, 'utf8') : null;
		workspaceBackup = process.env.WORKSPACE_ROOT ?? null;
		sandboxRoot = mkdtempSync(join(tmpdir(), 'pcd-audio-'));
		process.env.WORKSPACE_ROOT = sandboxRoot;
	});

	afterEach(() => {
		if (originalSpec === null) {
			try {
				rmSync(SPEC_FULL, { force: true });
			} catch {
				// ignore
			}
		} else {
			writeFileSync(SPEC_FULL, originalSpec);
		}
		if (workspaceBackup === null) {
			delete process.env.WORKSPACE_ROOT;
		} else {
			process.env.WORKSPACE_ROOT = workspaceBackup;
		}
		if (sandboxRoot) {
			rmSync(sandboxRoot, { recursive: true, force: true });
			sandboxRoot = null;
		}
		// keep the audio-staging/deprecation dirs out of test runs
		const staging = resolve(workspaceRoot, '.generate/audios/staging');
		const deprecated = resolve(workspaceRoot, '.deprecated');
		if (existsSync(staging)) rmSync(staging, { recursive: true, force: true });
		if (existsSync(deprecated)) rmSync(deprecated, { recursive: true, force: true });
	});

	it('validates the shipped audio spec against the global music anchor', () => {
		const result = runCli(['--validate']);
		expect(result.exitCode).toBe(0);
		expect(result.stdout).toMatch(/audio spec: ok/);
	});

	it('rejects music groups whose bpm drifts from the anchor', () => {
		if (originalSpec === null) throw new Error('expected shipped spec');
		const drift = JSON.parse(originalSpec) as Record<string, unknown>;
		const groups = drift.groups as Array<Record<string, unknown>>;
		const menu = groups.find((group) => group.id === 'music.menu');
		expect(menu).toBeDefined();
		const coherence = menu!.coherence as Record<string, unknown>;
		coherence.bpm = 120;
		writeFileSync(SPEC_FULL, JSON.stringify(drift, null, 2));

		const result = runCli(['--validate']);
		expect(result.exitCode).toBe(1);
		expect(result.stderr).toMatch(/declares bpm=120/);
	});

	it('flags status slots that point at non-existent groups', () => {
		if (originalSpec === null) throw new Error('expected shipped spec');
		const stripped = JSON.parse(originalSpec) as Record<string, unknown>;
		const status = (stripped.status as Record<string, unknown>) ?? {};
		(stripped as Record<string, unknown>).status = {
			'ghost.menu_main': {
				approvedVariantId: null,
				variants: {},
			},
			...status,
		};
		writeFileSync(SPEC_FULL, JSON.stringify(stripped, null, 2));

		const result = runCli(['--validate']);
		expect(result.exitCode).toBe(1);
		expect(result.stderr).toMatch(/ghost\.menu_main/);
	});

	it('list --all prints a header even when there are no variants', () => {
		const result = runCli(['list', '--all']);
		expect(result.exitCode).toBe(0);
		expect(result.stdout).toMatch(/\| slot \| variant \| state \| wav \| final \|/);
	});

	it('status prints the empty-state hint when there are no variants yet', () => {
		const result = runCli(['status']);
		expect(result.exitCode).toBe(0);
		expect(result.stdout).toMatch(/No audio slots have any variants yet/);
	});

	it('approve refuses to run on a variant that does not exist', () => {
		const result = runCli(['approve', 'sfx.ui.ui_click', '--variant', 'v_doesnotexist']);
		expect(result.exitCode).toBe(1);
		expect(result.stderr).toMatch(/variant v_doesnotexist not found/);
	});

	it('deprecate refuses to run on a slot that has no status', () => {
		const result = runCli(['deprecate', 'sfx.ui.ui_click', '--variant', 'v_doesnotexist']);
		expect(result.exitCode).toBe(1);
		expect(result.stderr).toMatch(/slot sfx\.ui\.ui_click has no status/);
	});

	it('listen refuses to run on a variant that does not exist', () => {
		const result = runCli(['listen', 'sfx.ui.ui_click', '--variant', 'v_doesnotexist']);
		expect(result.exitCode).toBe(1);
		expect(result.stderr).toMatch(/slot sfx\.ui\.ui_click has no staged variants/);
	});

	it('stage with an unknown slot fails fast with a helpful message', () => {
		const result = runCli(['stage', 'ghost.menu_main']);
		expect(result.exitCode).toBe(1);
		expect(result.stderr).toMatch(/group ghost not declared in spec/);
	});
});