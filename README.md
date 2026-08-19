# Player Club Dynasty

A football-manager simulation scaffolded for AI-assisted development with
[`@mcp-vertex/core`](https://github.com/CartagoGit/mcp-vertex).

> **Local-only mode.** This workspace does **not** publish to npm. The
> mcp-vertex core + every first-party plugin we use is loaded from a
> **local checkout** discovered by `tools/scripts/workspace/root.script.ts`, not from
> the npm registry. The pointer file is `.mcp-vertex-root` (override
> with `MCP_VERTEX_ROOT`).

---

## Quick start

```bash
# 1. Confirm the host can locate the local mcp-vertex checkout
bun tools/scripts/workspace/root.script.ts --check

# 2. Validate the configuration + plugin surface (no serve)
bun run mcp-vertex:doctor

# 3. Build the project plugin
bun tools/scripts/builder/plugin.script.ts player-club

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
├─ docs/wiki/                           design knowledge base (classic-manager reference)
│  ├─ pcfutbol/00-objetivo-y-alcance.md NORMATIVE — the target is PC Fútbol 6.0/7.0
│  ├─ pcfutbol/04-gramatica-visual.md   NORMATIVE visual grammar — read before any UI change
│  ├─ pcfutbol/13-version-objetivo-*.md the target version, screen by screen, with wireframes
│  └─ pc-dynasty/mapa-de-equivalencias.md  our screens vs the classic, with the open gaps
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
| Sync vendored `AGENT-BOOTSTRAP.md` from upstream | `bun tools/scripts/workspace/sync-bootstrap.script.ts` |
| Build every plugin in `plugins/` | `bun run build` |
| Build a single plugin | `bun tools/scripts/builder/plugin.script.ts <name>` |
| Create a new plugin skeleton | `bun run plugin:create <name>` |
| Full gate (typecheck + lint + tests + drift guards) | `bun run validate` |
| Launch the game (add `--quit-after N` so it ends by itself) | `bun run game:play` |
| Walk a route and photograph every step | `bun run game:explore --tour` |
| Print the running node tree | `bun run game:explore '[{"dump":"*"}]'` |
| Fetch the reference games we study (6.0, 7.0, Basket 6.5) | `bun run download` |
| Fetch one of them by id | `bun run download --game=pcf7` |
| Check the reference registry against its source | `bun run download:catalog` |

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
- [`docs/wiki/`](docs/wiki/README.md) — the **design knowledge base**: how the
  classic desktop managers structured their screens, interactions and systems,
  and what PC Dynasty takes from them. Served to agents through the `docs`
  plugin, so `mcp-vertex_docs_search` finds it without crawling the repo.
  [`docs/wiki/pcfutbol/00-objetivo-y-alcance.md`](docs/wiki/pcfutbol/00-objetivo-y-alcance.md)
  (the target is PC Fútbol 6.0, with 7.0's improvements) and
  [`docs/wiki/pcfutbol/04-gramatica-visual.md`](docs/wiki/pcfutbol/04-gramatica-visual.md)
  are **normative** for every UI change.
- Local mcp-vertex README — `$(bun run mcp-vertex:resolve-root)/README.md`
  — the upstream monorepo layout.