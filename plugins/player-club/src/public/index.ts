/**
 * Public surface of `@player-club/plugin`. The default export (in
 * `../index.ts`) is the loadable `IMcpPlugin`; this barrel re-exports
 * the typed shapes so other plugins can reuse them.
 */
export { default } from '../index';

export type { IPlayer, IClub, IDataFile } from '../lib/types';