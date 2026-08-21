#!/usr/bin/env bun
/**
 * tools/scripts/reference/vm.script.ts
 *
 * Drive the reference-game VirtualBox VMs from WSL, the same way
 * `download.script.ts` drives the disk-image downloads. One VM per
 * title (see `VBOX_VM_NAMES`); this script only ever touches the one
 * named by `--game`.
 *
 * Usage:
 *   bun run vm:status --game=pcf6
 *   bun run vm:start  --game=pcf6            # headless — no window at all
 *   bun run vm:start  --game=pcf6 --visible  # normal VirtualBox window
 *   bun run vm:stop   --game=pcf6            # savestate; safe if already off
 *   bun run vm:poweroff --game=pcf6          # hard reset, discards state — use when a
 *                                             # screen is genuinely stuck (savestate/resume
 *                                             # reproduces the same stuck state identically)
 *   bun run vm:screenshot --game=pcf6 --out shots/01-menu.png
 *   bun run vm:move   --game=pcf6 --reset --dx 150 --dy 300  # hover, no click
 *   bun run vm:click  --game=pcf6 --dx 9 --dy -36             # relative move + click
 *   bun run vm:click  --game=pcf6 --dx 9 --dy -36 --right      # right-click instead
 *   bun run vm:click  --game=pcf6 --dx 9 --dy -36 --hold-ms 500 # longer button-down hold
 *   bun run vm:drag   --game=pcf6 --reset --dx 100 --dy 100 --to-dx 150 --to-dy 120
 *                                                             # press at (dx,dy), drag to (to-dx,to-dy), release
 *   bun run vm:key    --game=pcf6 38 0f 8f b8                 # raw scancodes (Alt+Tab)
 *
 * Mouse/keyboard injection goes through VirtualBox's COM `IMouse` API and
 * `VBoxManage keyboardputscancode` — neither ever touches the host's real
 * cursor or window focus. See `docs/wiki/in-game/como-esta-montado.md` for
 * the coordinate-calibration approach and the window-focus pitfall (clicks
 * silently stop registering if the guest's foreground window is lost —
 * fixed by an Alt+Tab away-and-back via `vm:key`).
 *
 * Requires VirtualBox on the Windows host (WSL interop) — see
 * `docs/wiki/in-game/como-esta-montado.md` for how the VM itself was
 * built and why it lives on `V:\pcd-reference-games\`, not under WSL.
 */

import { existsSync, mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';

import { VBOX_VM_NAMES } from '../../contracts/constants/reference/vbox.constants.ts';
import {
	listVms,
	locatePowerShell,
	locateVBoxManage,
	moveMouse,
	powerOffVm,
	screenshot,
	sendScancodes,
	startVm,
	stopVm,
	vmInfo,
} from '../../helpers/reference/vbox.helper.ts';
import { resolveProjectPaths } from '../../helpers/system/path.helper.ts';
import { run } from '../../helpers/system/process.helper.ts';

const paths = resolveProjectPaths(import.meta.dirname);
const MOUSE_SCRIPT_PATH = resolve(import.meta.dirname, 'vbox-mouse.ps1');

const flagValue = (argv: readonly string[], flag: string): string | null => {
	const inline = argv.find((entry) => entry.startsWith(`${flag}=`));
	if (inline !== undefined) return inline.slice(flag.length + 1);
	const index = argv.indexOf(flag);
	return index === -1 || index + 1 >= argv.length ? null : argv[index + 1]!;
};

/** Convert a WSL path to the Windows-style path VBoxManage.exe expects. */
const toWindowsPath = async (wslPath: string): Promise<string> => {
	const result = await run('wslpath', { args: ['-w', wslPath] });
	if (!result.succeeded) throw new Error(`wslpath could not resolve ${wslPath}: ${result.stderr}`);
	return result.stdout.trim();
};

const main = async (): Promise<number> => {
	const argv = process.argv.slice(2);
	const command = argv[0];
	const gameId = flagValue(argv, '--game');

	if (command === 'list') {
		const vboxPath = locateVBoxManage();
		if (vboxPath === null) {
			process.stderr.write('vm: VBoxManage.exe not found. Is VirtualBox installed on the Windows host?\n');
			return 3;
		}
		const vms = await listVms(vboxPath);
		process.stdout.write(`registered VMs known to VBoxManage:\n`);
		for (const vm of vms) process.stdout.write(`  ${vm.name}  {${vm.uuid}}\n`);
		process.stdout.write(`\nreference games this workspace knows about:\n`);
		for (const [id, name] of Object.entries(VBOX_VM_NAMES)) process.stdout.write(`  ${id} -> ${name}\n`);
		return 0;
	}

	if (gameId === null || !(gameId in VBOX_VM_NAMES)) {
		process.stderr.write(
			`vm: pass --game=<id>. Known ids: ${Object.keys(VBOX_VM_NAMES).join(', ')}. ` +
				`Run \`bun run vm:list\` to see the registered VMs.\n`,
		);
		return 2;
	}

	const vmName = VBOX_VM_NAMES[gameId]!;
	const vboxPath = locateVBoxManage();
	if (vboxPath === null) {
		process.stderr.write('vm: VBoxManage.exe not found. Is VirtualBox installed on the Windows host?\n');
		return 3;
	}

	switch (command) {
		case 'status': {
			const info = await vmInfo(vboxPath, vmName);
			process.stdout.write(`${gameId} (${vmName}): ${info.state}\n`);
			if (info.configFile) process.stdout.write(`  config: ${info.configFile}\n`);
			return 0;
		}

		case 'start': {
			const visible = argv.includes('--visible');
			process.stdout.write(`vm: starting ${vmName} ${visible ? '(visible window)' : '(headless — nothing on screen)'}...\n`);
			const ok = await startVm(vboxPath, vmName, { visible });
			if (!ok) {
				process.stderr.write(`vm: failed to start ${vmName}. Is it already running, or mid-save?\n`);
				return 1;
			}
			process.stdout.write(`vm: ${vmName} is starting. Boot to the main menu takes ~30-60s from cold.\n`);
			return 0;
		}

		case 'stop': {
			process.stdout.write(`vm: saving state and stopping ${vmName}...\n`);
			const ok = await stopVm(vboxPath, vmName);
			if (!ok) {
				process.stderr.write(`vm: failed to stop ${vmName}.\n`);
				return 1;
			}
			return 0;
		}

		case 'poweroff': {
			process.stdout.write(`vm: hard power-off (discards state) for ${vmName}...\n`);
			const ok = await powerOffVm(vboxPath, vmName);
			if (!ok) {
				process.stderr.write(`vm: failed to power off ${vmName}.\n`);
				return 1;
			}
			return 0;
		}

		case 'screenshot': {
			const outArg = flagValue(argv, '--out') ?? `.generate/vm-shots/${gameId}-${Date.now()}.png`;
			const outPath = resolve(paths.workspaceRoot, outArg);
			mkdirSync(dirname(outPath), { recursive: true });
			const windowsOutPath = await toWindowsPath(outPath);
			const ok = await screenshot(vboxPath, vmName, windowsOutPath);
			if (!ok || !existsSync(outPath)) {
				process.stderr.write(`vm: screenshot failed. Is ${vmName} running? (\`bun run vm:status --game=${gameId}\`)\n`);
				return 1;
			}
			process.stdout.write(`vm: saved ${outPath}\n`);
			return 0;
		}

		case 'move':
		case 'click': {
			const psPath = locatePowerShell();
			if (psPath === null) {
				process.stderr.write('vm: powershell.exe not found on the Windows host.\n');
				return 3;
			}
			const dx = Number(flagValue(argv, '--dx') ?? '0');
			const dy = Number(flagValue(argv, '--dy') ?? '0');
			const resetCorner = argv.includes('--reset');
			const rightClick = argv.includes('--right');
			const holdMsArg = flagValue(argv, '--hold-ms');
			const holdMs = holdMsArg === null ? undefined : Number(holdMsArg);
			const scriptWindowsPath = await toWindowsPath(MOUSE_SCRIPT_PATH);
			const ok = await moveMouse(psPath, scriptWindowsPath, vmName, {
				resetCorner,
				dx,
				dy,
				click: command === 'click' && !rightClick,
				rightClick: command === 'click' && rightClick,
				holdMs,
			});
			if (!ok) {
				process.stderr.write(`vm: mouse ${command} failed against ${vmName}.\n`);
				return 1;
			}
			process.stdout.write(
				`vm: ${command} reset=${resetCorner} dx=${dx} dy=${dy} right=${rightClick} holdMs=${holdMs ?? 120} on ${vmName}\n`,
			);
			return 0;
		}

		case 'drag': {
			const psPath = locatePowerShell();
			if (psPath === null) {
				process.stderr.write('vm: powershell.exe not found on the Windows host.\n');
				return 3;
			}
			const dx = Number(flagValue(argv, '--dx') ?? '0');
			const dy = Number(flagValue(argv, '--dy') ?? '0');
			const toDx = flagValue(argv, '--to-dx');
			const toDy = flagValue(argv, '--to-dy');
			if (toDx === null || toDy === null) {
				process.stderr.write('vm: usage: bun run vm:drag --game=<id> --dx N --dy N --to-dx N --to-dy N [--reset]\n');
				return 2;
			}
			const resetCorner = argv.includes('--reset');
			const holdMsArg = flagValue(argv, '--hold-ms');
			const holdMs = holdMsArg === null ? undefined : Number(holdMsArg);
			const scriptWindowsPath = await toWindowsPath(MOUSE_SCRIPT_PATH);
			const ok = await moveMouse(psPath, scriptWindowsPath, vmName, {
				resetCorner,
				dx,
				dy,
				dragToDx: Number(toDx) - dx,
				dragToDy: Number(toDy) - dy,
				holdMs,
			});
			if (!ok) {
				process.stderr.write(`vm: drag failed against ${vmName}.\n`);
				return 1;
			}
			process.stdout.write(
				`vm: drag reset=${resetCorner} from=(${dx},${dy}) to=(${toDx},${toDy}) on ${vmName}\n`,
			);
			return 0;
		}

		case 'key': {
			const scancodes = argv.slice(1).filter((entry) => !entry.startsWith('--'));
			if (scancodes.length === 0) {
				process.stderr.write('vm: usage: bun run vm:key --game=<id> <hex-scancode>...\n');
				return 2;
			}
			const ok = await sendScancodes(vboxPath, vmName, scancodes);
			if (!ok) {
				process.stderr.write(`vm: key send failed against ${vmName}.\n`);
				return 1;
			}
			process.stdout.write(`vm: sent scancodes [${scancodes.join(' ')}] to ${vmName}\n`);
			return 0;
		}

		default:
			process.stderr.write(
				'vm: usage: bun run vm:<start|stop|poweroff|status|screenshot|list|move|click|drag|key> --game=<id> ' +
					'[--visible] [--out <path>] [--dx N --dy N] [--to-dx N --to-dy N] [--reset] [--right] [--hold-ms N]\n',
			);
			return 2;
	}
};

process.exit(await main());
