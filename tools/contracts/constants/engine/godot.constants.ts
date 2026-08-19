/**
 * tools/contracts/constants/engine/godot.constants.ts
 *
 * Strings that appear in the path where Godot lives, env vars that
 * override the lookup, and the lock file that prevents two simultaneous
 * launches. The playwright-style helpers read these once and propagate.
 */

/** Environment variable that hard-pins the Godot executable. */
export const GODOT_BIN_ENV_VAR = 'GODOT_BIN' as const;

/** Environment variable that turns the symlink update into a writable target. */
export const PCDYNASTY_TOOLS_DIR_ENV_VAR = 'PCDYNASTY_TOOLS_DIR' as const;

/** File name of the cached, properly-resolved Godot binary symlink. */
export const GODOT_CURRENT_SYMLINK = 'current' as const;

/** Filename of the version pin used by the smoke script and provisioner. */
export const GODOT_VERSION_FILENAME = 'godot.version' as const;

/** Lock file used by `play.script.ts` to prevent duplicate launches. */
export const PCDYNASTY_GAME_LOCK_FILE = 'pcdynasty-game.lock' as const;

/** Env var that bypasses the per-user lock for parallel CI / smoke runs. */
export const PCDYNASTY_ALLOW_MULTIPLE_ENV_VAR = 'PCDYNASTY_ALLOW_MULTIPLE' as const;

/** Substring that must appear in the binary name for it to be the C# flavour. */
export const GODOT_MONO_FLAVOUR_MARKER = 'mono' as const;

/** Pinned Godot version, baked into provisioning URLs and cache names. */
export const GODOT_VERSION = '4.7.1-stable' as const;

/** Pattern that catches `SCRIPT ERROR`, `ERROR:`, `USER ERROR` in smoke logs. */
export const GODOT_SMOKE_ERROR_REGEX: RegExp = /(SCRIPT ERROR|ERROR:|USER ERROR)/;
