# Player Club Dynasty

A football-manager simulation scaffolded for AI-assisted development with
[`@mcp-vertex/core`](https://github.com/CartagoGit/mcp-vertex).

> **Local-only mode.** This workspace does **not** publish to npm. The
> mcp-vertex core + every first-party plugin we use is loaded from a
> **local checkout** discovered by `scripts/mcp-vertex-root.script.ts`, not from
> the npm registry. The pointer file is `.mcp-vertex-root` (override
> with `MCP_VERTEX_ROOT`).

---

## Quick start

```bash
# 1. Confirm the host can locate the local mcp-vertex checkout
bun scripts/mcp-vertex-root.script.ts --check

# 2. Validate the configuration + plugin surface (no serve)
bun run mcp-vertex:doctor

# 3. Build the project plugin
bun scripts/build-plugin.script.ts player-club

# 4. Boot the MCP server (stdio; blocks)
bun run mcp-vertex:serve
```

The `.vscode/mcp.json` shape boots the same server under Copilot /
Cursor / Antigravity. Claude Code and Codex read the equivalents in
`~/.claude.json` and `~/.codex/config.toml` (or the workspace-level
`.codex/config.toml`).

## Layout

```
player-club-dynasty/
├─ AGENTS.md                            pointer → docs/mcp-vertex/AGENT-BOOTSTRAP.md
├─ CLAUDE.md                            pointer → docs/mcp-vertex/AGENT-BOOTSTRAP.md
├─ .github/copilot-instructions.md      pointer → docs/mcp-vertex/AGENT-BOOTSTRAP.md
├─ .cursor/rules/mcp-vertex.md          pointer → docs/mcp-vertex/AGENT-BOOTSTRAP.md
├─ .aider.conf.yml                      pointer → docs/mcp-vertex/AGENT-BOOTSTRAP.md
├─ .continue/config.json                pointer → docs/mcp-vertex/AGENT-BOOTSTRAP.md
├─ .codex/config.toml                   MCP server launch (same args as .vscode/mcp.json)
├─ .vscode/mcp.json                     VS Code / Cursor / Antigravity MCP server launch
├─ .vscode/settings.json                editor settings (mcp-vertex path, formatter)
├─ mcp-vertex.config.json               single source of truth for plugin config
├─ package.json                         bun workspaces + npm scripts
├─ bunfig.toml                          bun install scopes → local mcp-vertex packages
├─ tsconfig.json                        @mcp-vertex/core/* via absolute paths
├─ .mcp-vertex-root                     pointer file → the mcp-vertex checkout (relative)
├─ .gitignore                           excludes .cache/, node_modules/, dist/, data/, saves/
├─ docs/mcp-vertex/
│  ├─ AGENT-BOOTSTRAP.md                vendored from local mcp-vertex checkout
│  ├─ REPO-RULES.md                     workspace-level repo rules
│  ├─ host-hints/agent-instructions.generated.md  auto-generated catalog fragment
│  └─ proposals/                        proposals store (8 status folders)
├─ plugins/player-club/                 @player-club/plugin (project-scoped MCP plugin)
├─ scripts/                             workspace-local helpers
└─ .cache/mcp-vertex/                   per-plugin scratch dirs (gitignored)
```

## Commands

| Task | Command |
|---|---|
| Boot the MCP server (stdio, blocks) | `bun run mcp-vertex:serve` |
| Doctor — validate config + plugins, no serve | `bun run mcp-vertex:doctor` |
| Print compact overview (JSON) | `bun run mcp-vertex:overview` |
| Print agent catalog (compact mode) | `bun run mcp-vertex:catalog` |
| Regenerate typed tool-outputs SDK | `bun run mcp-vertex:types` |
| Regenerate host-hints fragment | `bun run mcp-vertex:hints` |
| Sync vendored `AGENT-BOOTSTRAP.md` from upstream | `bun scripts/sync-bootstrap.script.ts` |
| Build every plugin in `plugins/` | `bun run build` |
| Build a single plugin | `bun scripts/build-plugin.script.ts <name>` |
| Create a new plugin skeleton | `bun run plugin:create <name>` |
| Full gate (typecheck + lint + tests + drift guards) | `bun run validate` |

## Plugins

This workspace loads the **swarm** preset plus `player-club`, with
`issues`, `web-fetch`, `external-mcps` excluded:

- **swarm base** — `git`, `search`, `memory`, `docs`, `rules`,
  `quality`, `deps`, `proposals`, `notification`, `status-marker`,
  `test-convention`.
- **player-club** — project-scoped MCP plugin (see
  [`plugins/player-club/`](plugins/player-club/README.md)) that exposes
  the football-manager dataset over MCP: `player_club_list_clubs`,
  `player_club_get_squad`, `player_club_current_season`.

To add another plugin:

1. Bump `.vscode/mcp.json`'s `--plugins=<name>` flag and the
   `package.json` script.
2. Add a `plugins.<name>.options` block to `mcp-vertex.config.json`.
3. (Optional) Pre-bake a per-plugin config preset so re-launching
   matches the canonical docs.

## Why local-only?

The mcp-vertex repo is in active development — the published versions
on npm trail the `develop` branch by days to weeks. Until the team is
ready to publish a stable `1.x`, this workspace bootstraps against a
sibling checkout (`../../_projects/mcp-vertex`) and treats the
pointer file as the canonical "where is the source of truth right
now". When the team is ready to ship to npm:

```bash
# 1. Remove the pointer file (or point at the published version)
rm .mcp-vertex-root

# 2. Switch .vscode/mcp.json to the canonical npm launch shape:
#    bunx --package @mcp-vertex/cli mcpv __serve \
#      --workspace . --preset swarm --plugins player-club
# 3. Same change in package.json#scripts and .codex/config.toml.
```

## Learn more

- [`docs/mcp-vertex/AGENT-BOOTSTRAP.md`](docs/mcp-vertex/AGENT-BOOTSTRAP.md) —
  the universal agent rules (orient, route, workflow loop, invariants,
  host appendices).
- [`docs/mcp-vertex/REPO-RULES.md`](docs/mcp-vertex/REPO-RULES.md) —
  the workspace-specific repo rules (commands, hard rules, plugin
  prefixes).
- Local mcp-vertex README — `$(bun run mcp-vertex:resolve-root)/README.md`
  — the upstream monorepo layout.