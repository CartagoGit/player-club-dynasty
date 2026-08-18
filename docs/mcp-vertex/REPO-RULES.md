# Repo-level rules — Player Club Dynasty

> Read this **only** if the host you are running in reads a
> workspace-root `AGENTS.md` (see [`AGENT-BOOTSTRAP.md`](AGENT-BOOTSTRAP.md)
> §7). Hosts that don't read `AGENTS.md` never need this file —
> nothing in the universal bootstrap depends on it, so skip it and save
> the tokens.

## What this repo is

A **football-manager game** scaffolded for AI-assisted development with
`@mcp-vertex/core`. The actual game code lives under `src/`; the
project's own MCP plugin lives under `plugins/player-club/`. The
mcp-vertex core + every first-party plugin we use (proposals, memory,
search, git, docs, deps, rules, quality, notification, status-marker,
test-convention) come from the **local checkout** resolved by
`scripts/mcp-vertex-root.script.ts` (env var, `.mcp-vertex-root` pointer,
linked package, or a `mcp-vertex` sibling directory) — no npm publish
required and no machine-specific path is committed.

## Layout

| Path | What |
|---|---|
| `mcp-vertex.config.json` | The single source of truth for plugin config + plugin-specific options. |
| `.vscode/mcp.json` | The VS Code / Cursor / Antigravity launch shape (references the local mcp-vertex checkout). |
| `package.json` | Workspace scripts (`mcp-vertex:serve`, `mcp-vertex:doctor`, `mcp-vertex:overview`, …) + bun workspaces for the local plugins. |
| `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md`, `.cursor/rules/mcp-vertex.md`, `.aider.conf.yml`, `.continue/config.json`, `.claude/settings.json`, `.codex/config.toml` | Host instruction files. Every one of them is a **pointer** to [`docs/mcp-vertex/AGENT-BOOTSTRAP.md`](AGENT-BOOTSTRAP.md). |
| `docs/mcp-vertex/AGENT-BOOTSTRAP.md` | Vendored copy of the canonical universal bootstrap (refresh with `bun scripts/sync-bootstrap.script.ts`). |
| `docs/mcp-vertex/host-hints/agent-instructions.generated.md` | Auto-generated hint fragment, regenerated with `bun run mcp-vertex:hints`. |
| `docs/mcp-vertex/proposals/` | The proposals store (`blocked/`, `done/`, `in-progress/`, `paused/`, `ready/`, `retired/`, `review/`, `legacy/`). |
| `plugins/player-club/` | The project's own MCP plugin (`@player-club/plugin`). |
| `scripts/` | Workspace-local helpers (build, dev, sync-bootstrap, root resolver). |
| `.cache/mcp-vertex/` | Scratch root, per-plugin subdirectories created on first boot. |
| `.mcp-vertex-root` | Pointer file consumed by `scripts/mcp-vertex-root.script.ts`. Holds a path relative to the workspace; overridable via `MCP_VERTEX_ROOT`. |

## Commands (the only ones you need)

| Task | Command |
|---|---|
| Boot the MCP server (stdio, blocks) | `bun run mcp-vertex:serve` |
| Doctor — validate config + plugins, no serve | `bun run mcp-vertex:doctor` |
| Print the compact overview (JSON) | `bun run mcp-vertex:overview` |
| Print the agent catalog (compact mode) | `bun run mcp-vertex:catalog` |
| Regenerate typed tool-outputs SDK | `bun run mcp-vertex:types` |
| Regenerate host-hints fragment | `bun run mcp-vertex:hints` |
| Sync vendored AGENT-BOOTSTRAP.md from upstream | `bun scripts/sync-bootstrap.script.ts` |
| Build every plugin in `plugins/` | `bun run build` |
| Build a single plugin | `bun scripts/build-plugin.script.ts player-club` |
| Create a new plugin skeleton | `bun run plugin:create <name>` |
| Full gate (typecheck + lint + tests + drift guards) | `bun run validate` |

## Repo-level hard rules

1. **mcp-vertex stays local-only for now.** No npm publish, no `@mcp-vertex/*` from the npm registry. Every first-party package resolves from the local checkout via `scripts/mcp-vertex-root.script.ts`. Bump the `.mcp-vertex-root` pointer (or set `MCP_VERTEX_ROOT`) when upgrading.
2. **The plugin set is curated, not exhaustive.** The workspace loads `swarm` plus `player-club`, with `issues` / `web-fetch` / `external-mcps` excluded by default. Add new plugins deliberately; update both `mcp-vertex.config.json` and `.vscode/mcp.json`'s `--plugins=` flag together.
3. **Plugins live under `plugins/`.** Every project-scoped MCP capability is a plugin, never a stray tool added to a config. The `--plugins=<path>` CLI form (f00087) is the canonical way to point at a local plugin without polluting the host's MCP config.
4. **Host instruction files are pointers only.** `AGENTS.md`, `CLAUDE.md`, etc. must not enumerate tool / skill / proposal ids. The server is the source of truth. If you find yourself wanting to write a list, call `mcp-vertex_agent_catalog` instead.
5. **Bun workspaces point at our plugins only.** `package.json#workspaces` covers `plugins/*` and `scripts/*`. The local mcp-vertex packages are resolved at runtime via the host script, not via workspace links — keeps the two lockfiles separate.
6. **Every tool declares an `outputSchema`.** That includes the `player-club` plugin. Drift against the generated SDK fails `bun run mcp-vertex:types --check`.
7. **No `process.cwd()`.** Path resolution flows through `ctx.workspace` (for plugins) or `scripts/mcp-vertex-root.script.ts` (for scripts). The workspace root is the cwd the host is launched from, never assumed by the tool layer.
8. **Bash, never zsh.** The user keeps zsh (Powerlevel10k). Agent-driven shell calls — direct `run_in_terminal`, subagent shell, MCP tool handlers — must launch `bash -c '<cmd>'`. The p10k instant prompt opens the alternate screen buffer and silently breaks zsh-driven wrappers. The fallback ladder is in [`docs/mcp-vertex/skills/shell-fallback/SKILL.md`](skills/shell-fallback/SKILL.md) once that skill is vendored.

## Plugin ID prefixes

The proposals plugin assigns IDs to slices; the audit plugin (when
loaded) reads from `get_proposal_workflow`. Today this repo has zero
open proposals — the orchestrator will assign the first ID on the next
`proposals_auto_work` call. The standard mcp-vertex prefixes apply:

- `p00001-…` — feature / refactor proposal
- `f00001-…` — fix (cascades before `p`)
- `c00001-…` — chore / infra
- `q00001-…` — plan-of-plans
- `x00001-…` — eXperiment (spike / throwaway)
- `a00001-…` — audit (when `audit` plugin loaded)

## When the local checkout moves

```bash
# 1. Update the pointer (relative to this workspace, or absolute)
echo "../../mcp-vertex" > .mcp-vertex-root

# 2. Confirm
bun scripts/mcp-vertex-root.script.ts --check

# 3. Refresh the vendored bootstrap
bun scripts/sync-bootstrap.script.ts

# 4. Re-run doctor
bun run mcp-vertex:doctor
```