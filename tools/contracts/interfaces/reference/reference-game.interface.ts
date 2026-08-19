/**
 * tools/contracts/interfaces/reference/reference-game.interface.ts
 *
 * The shape of the reference-game registry: the classic titles we study
 * to build PC Dynasty's interface grammar.
 *
 * These are **references, not dependencies**. Nothing here is shipped,
 * nothing lands in the repository, and no build step needs them. They
 * exist so a question about how the original behaved can be answered by
 * running it instead of by guessing — see
 * `docs/wiki/pc-dynasty/como-mirar-el-juego.md`.
 */

/** Which slot a disk image occupies when the game is booted. */
export type ReferenceDiskSlot = 'hda' | 'hdb' | 'cdrom';

export interface IReferenceDisk {
	/** Absolute URL. May carry a `?v=` cache-buster that the filename drops. */
	readonly url: string;
	/** Declared size in bytes, used to verify and to resume. `null` when unknown. */
	readonly size: number | null;
}

export interface IReferenceGame {
	/** Human title, as the archive spells it. */
	readonly name: string;
	/** Release year, as the archive records it. */
	readonly year: string;
	/**
	 * The disks the title needs. `hda` is a bootable system image with the
	 * game already installed; `hdb` is its data volume; `cdrom` is the
	 * original disc. A title with no `hdb` keeps everything on `hda`.
	 */
	readonly disks: Readonly<Partial<Record<ReferenceDiskSlot, IReferenceDisk>>>;
}

/** One planned transfer, resolved against the local download directory. */
export interface IReferenceDownload {
	readonly gameId: string;
	readonly slot: ReferenceDiskSlot;
	readonly url: string;
	readonly targetPath: string;
	readonly expectedBytes: number | null;
}

/** What happened to one transfer. */
export interface IReferenceDownloadResult {
	readonly download: IReferenceDownload;
	readonly ok: boolean;
	/** `true` when the file was already complete and nothing was fetched. */
	readonly skipped: boolean;
	readonly bytes: number;
	readonly error: string | null;
}
