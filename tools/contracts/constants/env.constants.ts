/**
 * tools/contracts/constants/env.constants.ts
 *
 * Environment variables shared across the tool layer. The helpers
 * normalise reads (`getEnv`) so each script never forks its own
 * `process.env` handling.
 */

/** Standard XDG runtime dir (Linux). Fallback is `/tmp` when the env var is missing. */
export const XDG_RUNTIME_DIR_ENV_VAR = 'XDG_RUNTIME_DIR' as const;

/** Path-list separator. */
export const PATH_LIST_SEPARATOR = ':' as const;

/** Common Linux home directory fallback. */
export const HOME_ENV_VAR = 'HOME' as const;

/** PATH env var name. */
export const PATH_ENV_VAR = 'PATH' as const;
