/**
 * plugins/delendai_player-club-dynasty/src/lib/conventions/file-conventions.helper.ts
 *
 * Pure validators that depend on the FILE_CONVENTIONS map. Lives in its
 * own module so the constants file stays logic-free, matching the
 * convention the file-conventions.spec.ts enforces.
 */

import { FILE_CONVENTIONS, type FileKind } from './file-conventions.constants';

export interface IPathValidation {
	readonly ok: boolean;
	readonly kind: FileKind | 'unknown';
	readonly reason: string;
}

/**
 * Validate a workspace-relative path against the convention. Returns the
 * matching `kind` or `unknown` when the extension does not match any
 * known category. The function does **not** touch the filesystem.
 */
export const validatePath = (relativePath: string): IPathValidation => {
	const normalised = relativePath.replace(/^\.\//, '');
	for (const [kind, rule] of Object.entries(FILE_CONVENTIONS)) {
		if (normalised.endsWith(rule.extension)) {
			return {
				ok: true,
				kind: kind as FileKind,
				reason: `${rule.extension} matches the ${kind} convention.`,
			};
		}
	}
	return {
		ok: false,
		kind: 'unknown',
		reason: `Path "${relativePath}" does not end with a known extension. Use one of: ${Object.values(
			FILE_CONVENTIONS,
		)
			.map((rule) => rule.extension)
			.join(', ')}.`,
	};
};