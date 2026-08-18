/**
 * plugins/mcp-vertex_player-club-dynasty/src/lib/conventions/file-conventions.constants.ts
 *
 * The single source of truth for the **Player Club Dynasty** file and
 * folder conventions. The plugin exposes it through a tool, a prompt and
 * a skill so any agent (human or IA) can scaffold a new file under the
 * same rules.
 *
 * The convention is terse but strict:
 *
 *   - Directories describe a role; extensions describe the type inside.
 *   - `*.script.ts`       → executable entrypoint (shebang `#!/usr/bin/env bun`).
 *   - `*.helper.ts`       → reusable utility module (no side effects at import).
 *   - `*.interface.ts`    → TypeScript types/interfaces only.
 *   - `*.constants.ts`    → constants only, no logic.
 *   - `*.spec.ts`         → Vitest unit test.
 *   - `*.sh`              → only when a third-party tool requires it.
 *
 *   - `tools/contracts/constants/**` → string constants.
 *   - `tools/contracts/interfaces/**` → types.
 *   - `tools/helpers/**`              → helpers.
 *   - `tools/scripts/<area>/**`       → entrypoints grouped by domain.
 *   - `plugins/<name>/src/lib/**`     → plugin-internal helpers (grouped by domain).
 *
 * Logic that depends on this map lives in `file-conventions.helper.ts` —
 * constants files stay logic-free.
 */

export const FILE_CONVENTIONS = {
	script: {
		extension: '.script.ts',
		shebang: '#!/usr/bin/env bun',
		role: 'executable entrypoint that wraps a `run()` and exits',
		directories: ['tools/scripts/**'],
	},
	helper: {
		extension: '.helper.ts',
		role: 'reusable utility — no top-level side effects',
		directories: ['tools/helpers/**', 'plugins/*/src/lib/**'],
	},
	interface: {
		extension: '.interface.ts',
		role: 'types and interfaces only — no logic',
		directories: ['tools/contracts/interfaces/**', 'plugins/*/src/lib/**'],
	},
	constants: {
		extension: '.constants.ts',
		role: 'frozen values — no logic, no I/O',
		directories: ['tools/contracts/constants/**', 'plugins/*/src/lib/**'],
	},
	spec: {
		extension: '.spec.ts',
		role: 'Vitest unit test',
		directories: ['**/tests/**', '**/*.spec.ts'],
	},
} as const;

export type FileKind = keyof typeof FILE_CONVENTIONS;

export const SOLID_PRINCIPLES = {
	srp: 'Single Responsibility — one module, one reason to change.',
	ocp: 'Open/Closed — extend behaviour through new code, not by editing existing conditionals.',
	lsp: 'Liskov Substitution — subtypes must satisfy the contracts they inherit.',
	isp: 'Interface Segregation — narrow interfaces beat fat ones; never make a consumer implement what it does not need.',
	dip: 'Dependency Inversion — depend on abstractions (interfaces + constants), not on concrete implementations.',
} as const;

export const WORKSPACE_RULES = [
	'Scripts are thin orchestrators. Helpers own the filesystem and processes.',
	'Every script imports helpers via `tools/helpers/*.helper.ts`. They never import `node:fs` / `node:child_process` directly.',
	'Types live in `tools/contracts/interfaces/*.interface.ts`; constants live in `tools/contracts/constants/*.constants.ts`.',
	'No `process.cwd()` in helpers; paths come from `resolveProjectPaths(import.meta.dir)`.',
	'Async work uses `helpers/run.helper.ts`; sync work uses `process.helper.ts`.',
	'Every new file is paired with a spec when it owns logic.',
] as const;

/** Compact one-screen description of the convention, used by the skill. */
export const CONVENTION_BRIEF = `
# Player Club Dynasty — file & folder convention

## Roles
${Object.entries(FILE_CONVENTIONS)
	.map(([kind, rule]) => `- \`${rule.extension}\` → ${kind}: ${rule.role}`)
	.join('\n')}

## Folders
- \`tools/contracts/constants/**\` — string constants.
- \`tools/contracts/interfaces/**\` — types/interfaces, no logic.
- \`tools/helpers/**\` — reusable utilities (no top-level side effects).
- \`tools/scripts/<area>/**\` — entrypoints grouped by domain (\`game/\`, \`gate/\`, \`mcp-vertex/\`, \`build/\`, \`replays/\`, \`content/\`).
- \`plugins/<name>/src/lib/**\` — plugin-internal helpers.

## SOLID, by default
${Object.entries(SOLID_PRINCIPLES)
	.map(([letter, rule]) => `- **${letter.toUpperCase()}** — ${rule}`)
	.join('\n')}

## Workspace rules
${WORKSPACE_RULES.map((rule) => `- ${rule}`).join('\n')}
`.trim();
