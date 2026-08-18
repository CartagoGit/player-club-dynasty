/**
 * tools/contracts/interfaces/run.interface.ts
 *
 * The shape every `runXxx()` helper returns. Centralising the contract
 * lets scripts chain operations without learning three different
 * `child_process` APIs.
 */

export interface IRunResult {
	/** Child exit code. `-1` means the process never started. */
	readonly exitCode: number;
	/** Captured stdout (empty when `stdio: 'inherit'`). */
	readonly stdout: string;
	/** Captured stderr (empty when `stdio: 'inherit'`). */
	readonly stderr: string;
	/** True when the process started, ran, and exited. */
	readonly succeeded: boolean;
}

/** Options accepted by every `runXxx()` helper. */
export interface IRunOptions {
	/** Arguments after the executable. Avoid passing user input untrusted. */
	readonly args?: readonly string[];
	/** Working directory. Defaults to the workspace root. */
	readonly cwd?: string;
	/** Extra env vars, merged on top of `process.env`. */
	readonly env?: Readonly<Record<string, string>>;
	/** When `true`, stdout/stderr stream to the parent's TTY. */
	readonly inherit?: boolean;
	/** Max number of milliseconds to wait before killing the child. */
	readonly timeoutMs?: number;
}
