/**
 * tools/contracts/constants/reference/vbox.constants.ts
 *
 * Where the reference-game VirtualBox setup lives on the Windows host, and
 * the fixed names inside it. See
 * `docs/wiki/in-game/como-esta-montado.md` for how this was built and why
 * it lives outside the WSL filesystem.
 */

/** VBoxManage.exe, reached through WSL's automatic Windows-drive mounts. */
export const VBOXMANAGE_WINDOWS_PATHS: readonly string[] = [
	'/mnt/c/Program Files/Oracle/VirtualBox/VBoxManage.exe',
];

/**
 * Windows-side folder holding every reference VM, its disks and any
 * captures taken during a session. Deliberately outside `C:\`: these are
 * multi-gigabyte VDIs, and `C:\` on the host this was built against had
 * single-digit GB free.
 */
export const VBOX_GAMES_ROOT_WINDOWS = 'V:\\pcd-reference-games';

/** One VM per reference title, named after its catalogue id (see the download registry). */
export const VBOX_VM_NAMES: Readonly<Record<string, string>> = {
	pcf6: 'PCF6Test',
};

/**
 * Windows PowerShell, reached through WSL's automatic Windows-drive mounts.
 * Needed only for mouse injection: VirtualBox's `IMouse` COM interface has
 * no `VBoxManage` CLI equivalent and no Bun/Node binding.
 */
export const POWERSHELL_WINDOWS_PATHS: readonly string[] = [
	'/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe',
];
