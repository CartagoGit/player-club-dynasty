/**
 * plugins/mcp-vertex_player-club-dynasty/src/lib/index.ts
 *
 * Barrel for the plugin's internal helpers and types. Grouped by
 * domain so each entrypoint (or skill) can pick exactly what it needs:
 *
 *   - `domain/audio/*`    — zod schemas + loaders for the audio manifest
 *   - `domain/game-data/*` — types for the football-manager dataset
 *   - `conventions/*`     — file & folder convention the plugin enforces
 *
 * Keeping this barrel flat (no re-exports) means every consumer imports
 * from the actual path: `from './lib/domain/audio/audio-manifest.helper'`.
 * The barrel exists for documentation + future tooling.
 */

export type { AudioSpec, AudioGroup, AudioEntryDef, AudioSlotStatus, AudioVariantRecord, AudioVariantState } from './domain/audio/audio-spec.types';
export { assertMusicAnchor, findSlot, loadAudioManifest } from './domain/audio/audio-manifest.helper';
export type { IDataFile, IClub, IPlayer } from './domain/game-data/game-data.types';
export { FILE_CONVENTIONS, SOLID_PRINCIPLES, WORKSPACE_RULES, CONVENTION_BRIEF } from './conventions/file-conventions.constant';
export type { FileKind } from './conventions/file-conventions.constant';
export { validatePath } from './conventions/file-conventions.helper';
export type { IPathValidation } from './conventions/file-conventions.helper';
export { renderTemplate, templateFor } from './conventions/file-assistant.helper';
export type { IFileTemplate } from './conventions/file-assistant.helper';