#!/usr/bin/env bun
// Re-export the working script. The `.script.ts` filename follows the
// mcp-vertex convention (so `bun run mcp-vertex:validate` etc. find it);
// this `.ts` shim keeps the older `scripts/mcp-vertex-root.ts` reference
// working for anyone who copy-pasted it before the rename.
export * from './mcp-vertex-root.script.ts';