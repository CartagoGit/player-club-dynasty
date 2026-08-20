/**
 * tools/contracts/interfaces/reference/vbox.interface.ts
 *
 * The narrow slice of VirtualBox this workspace drives: locate
 * `VBoxManage.exe` from WSL, start/stop one named VM headless-or-visible,
 * and pull a screenshot. Not a general VirtualBox wrapper.
 */

export type VBoxVmState = 'running' | 'poweroff' | 'saved' | 'paused' | 'unknown';

export interface IVBoxManage {
	/** The Windows path to VBoxManage.exe, as WSL sees it (e.g. `/mnt/c/...`). */
	readonly binaryPath: string;
	run(args: readonly string[]): Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
}

export interface IVBoxVmInfo {
	readonly name: string;
	readonly state: VBoxVmState;
	readonly configFile: string | null;
}

/** One relative mouse move, optionally anchored to (0,0) first and/or followed by a click. */
export interface IVBoxMouseAction {
	readonly resetCorner?: boolean;
	readonly dx?: number;
	readonly dy?: number;
	readonly click?: boolean;
}
