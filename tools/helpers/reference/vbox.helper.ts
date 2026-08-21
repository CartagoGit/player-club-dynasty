#!/usr/bin/env bun
/**
 * tools/helpers/reference/vbox.helper.ts
 *
 * Drive VirtualBox from WSL: locate `VBoxManage.exe` on the Windows side,
 * start/stop a named VM, read its state, and pull a screenshot. This is
 * the layer that actually shells out; `vm.script.ts` is the CLI on top
 * of it.
 *
 * Why VirtualBox and not QEMU: the reference titles' copy-protection
 * checks enumerate real optical hardware (`SetupDiCreateDeviceInfoList`),
 * which only a genuine ATAPI CD-ROM — the kind VirtualBox emulates —
 * satisfies. See `docs/wiki/in-game/como-esta-montado.md`.
 */

import { existsSync } from 'node:fs';

import { POWERSHELL_WINDOWS_PATHS, VBOXMANAGE_WINDOWS_PATHS } from '../../contracts/constants/reference/vbox.constants.ts';
import type { IVBoxMouseAction, IVBoxVmInfo, VBoxVmState } from '../../contracts/interfaces/reference/vbox.interface.ts';
import { run } from '../system/process.helper.ts';

/** Find VBoxManage.exe among the known install locations, WSL-side. */
export const locateVBoxManage = (): string | null => VBOXMANAGE_WINDOWS_PATHS.find(existsSync) ?? null;

/** Find powershell.exe among the known install locations, WSL-side. */
export const locatePowerShell = (): string | null => POWERSHELL_WINDOWS_PATHS.find(existsSync) ?? null;

const vboxManage = async (
	vboxPath: string,
	args: readonly string[],
): Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }> => run(vboxPath, { args });

/** Start a VM. `visible` opens the normal VirtualBox window; otherwise headless (no window at all). */
export const startVm = async (
	vboxPath: string,
	vmName: string,
	options: { readonly visible?: boolean } = {},
): Promise<boolean> => {
	const type = options.visible ? 'gui' : 'headless';
	const result = await vboxManage(vboxPath, ['startvm', vmName, '--type', type]);
	return result.exitCode === 0;
};

/** Save state and stop the VM (safe to call whether it is running, saved, or already off). */
export const stopVm = async (vboxPath: string, vmName: string): Promise<boolean> => {
	const info = await vmInfo(vboxPath, vmName);
	if (info.state !== 'running' && info.state !== 'paused') return true;
	const result = await vboxManage(vboxPath, ['controlvm', vmName, 'savestate']);
	return result.exitCode === 0;
};

/**
 * Hard power-off (discards VM state, unlike `stopVm`'s savestate). Needed to
 * escape a genuinely stuck screen — a save/resume cycle reproduces a stuck
 * modal identically (confirmed in `docs/wiki/in-game/pcf6/07-despacho.md`),
 * so only a real cold reboot clears it.
 */
export const powerOffVm = async (vboxPath: string, vmName: string): Promise<boolean> => {
	const info = await vmInfo(vboxPath, vmName);
	if (info.state !== 'running' && info.state !== 'paused') return true;
	const result = await vboxManage(vboxPath, ['controlvm', vmName, 'poweroff']);
	return result.exitCode === 0;
};

const parseState = (raw: string): VBoxVmState => {
	const match = /^VMState="(\w+)"/m.exec(raw);
	const state = match?.[1];
	if (state === 'running' || state === 'poweroff' || state === 'saved' || state === 'paused') return state;
	return 'unknown';
};

/** Read the VM's current power state and config file path. */
export const vmInfo = async (vboxPath: string, vmName: string): Promise<IVBoxVmInfo> => {
	const result = await vboxManage(vboxPath, ['showvminfo', vmName, '--machinereadable']);
	if (result.exitCode !== 0) return { name: vmName, state: 'unknown', configFile: null };
	const cfgMatch = /^CfgFile="([^"]+)"/m.exec(result.stdout);
	return { name: vmName, state: parseState(result.stdout), configFile: cfgMatch?.[1]?.replace(/\\\\/g, '\\') ?? null };
};

/** Save a PNG of the VM's current framebuffer to a Windows-side path. */
export const screenshot = async (vboxPath: string, vmName: string, windowsOutPath: string): Promise<boolean> => {
	const result = await vboxManage(vboxPath, ['controlvm', vmName, 'screenshotpng', windowsOutPath]);
	return result.exitCode === 0;
};

/**
 * Send a keystroke by raw scancode pair (make, break) straight into the
 * guest's virtual keyboard controller — the same non-invasive channel as
 * the mouse, and the only reliable way found so far to reach system-level
 * shell hotkeys (Alt+Tab) when the game window has lost real Win32 focus.
 * `hex` pairs are e.g. `['38', '0f', '8f', 'b8']` for Alt-down, Tab-down,
 * Tab-up, Alt-up.
 */
export const sendScancodes = async (vboxPath: string, vmName: string, hex: readonly string[]): Promise<boolean> => {
	const result = await vboxManage(vboxPath, ['controlvm', vmName, 'keyboardputscancode', ...hex]);
	return result.exitCode === 0;
};

/**
 * Move the mouse (optionally reset to the top-left corner first) and
 * optionally click, via VirtualBox's `IMouse.PutMouseEvent` COM interface.
 * This never touches the host's real cursor or window focus — see
 * `docs/wiki/in-game/como-esta-montado.md` for why this is the only
 * mechanism that satisfies that constraint, and for the coordinate
 * calibration approach (device-delta space isn't 1:1 with screenshot
 * pixels; calibrate per session with `resetCorner` + a known probe delta).
 */
export const moveMouse = async (
	psPath: string,
	scriptWindowsPath: string,
	vmName: string,
	action: IVBoxMouseAction,
): Promise<boolean> => {
	const args = ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', scriptWindowsPath, '-VmName', vmName];
	if (action.resetCorner) args.push('-ResetCorner');
	if (action.dx) args.push('-Dx', String(action.dx));
	if (action.dy) args.push('-Dy', String(action.dy));
	if (action.click) args.push('-Click');
	if (action.rightClick) args.push('-RightClick');
	if (action.holdMs) args.push('-HoldMs', String(action.holdMs));
	if (action.dragToDx !== undefined) args.push('-DragToDx', String(action.dragToDx));
	if (action.dragToDy !== undefined) args.push('-DragToDy', String(action.dragToDy));
	const result = await run(psPath, { args, timeoutMs: 20_000 });
	return result.exitCode === 0;
};

/** List every registered VM as `{name, uuid}` pairs. */
export const listVms = async (vboxPath: string): Promise<readonly { readonly name: string; readonly uuid: string }[]> => {
	const result = await vboxManage(vboxPath, ['list', 'vms']);
	const entries: { name: string; uuid: string }[] = [];
	for (const line of result.stdout.split('\n')) {
		const match = /^"(.+)" \{(.+)\}$/.exec(line.trim());
		if (match) entries.push({ name: match[1]!, uuid: match[2]! });
	}
	return entries;
};
