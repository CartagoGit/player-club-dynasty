/**
 * tools/contracts/interfaces/system/lock.interface.ts
 *
 * The contract every `--lock` helper exposes. Scripts receive an
 * `ILockHandle` so they can release on `finally` without knowing whether
 * the lock is a PID file, a flock() fd, or in-memory.
 */

export interface ILockHandle {
	/** Path of the lock file on disk. */
	readonly path: string;
	/**
	 * Releases the lock. Idempotent: a second call is a no-op. Implementations
	 * may clean up a stale PID file written by a previous owner.
	 */
	release(): Promise<void> | void;
}

/** Result of `acquireLock`. `false` means another live process already holds it. */
export type LockAcquired = ILockHandle | false;
