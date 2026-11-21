# `@player-club-dynasty/delendai-plugin`

The project-scoped MCP plugin for **Player Club Dynasty**. It exposes the
football-manager dataset (clubs, squads, season clock) over MCP so any agent
running through `delendai` can drive the simulation.

The plugin lives under `plugins/delendai_player-club-dynasty/` so the
folder name (`delendai_*`) is unmistakable as an delendai plugin,
not part of the game itself. The runtime plugin id (and the default tool
namespace) is `player-club-dynasty` — chosen to match the game's working
name, not the delendai infra prefix.

> Loaded automatically by the workspace's `.vscode/mcp.json` via
> `--plugins=delendai_player-club-dynasty` (the host resolves the
> folder to the plugin's `dist/index.js`). Disable by removing the
> `--plugins=` flag.

## Tools (namespaced `player-club-dynasty_*` by default)

| Tool | Purpose |
|---|---|
| `player-club-dynasty_list_clubs` | Enumerate every saved club with stadium, founded year, squad size, average rating. |
| `player-club-dynasty_get_squad` | Return a club's full squad, ordered by shirt number. |
| `player-club-dynasty_current_season` | Read the in-progress season number and the count of clubs known to the dataset. |

## Configure (`delendai.config.json`)

```jsonc
{
	"plugins": {
		"delendai_player-club-dynasty": {
			"options": {
				"dataDir": "data",
				"savesDir": "saves"
			}
		}
	}
}
```

The dataset lives at `<dataDir>/clubs.json` (default `data/clubs.json`) and
is auto-created with an empty fixture on first read.

## Develop

```bash
# Build the dist/ the delendai host imports at runtime
bun tools/scripts/builder/plugin.script.ts delendai_player-club-dynasty

# Typecheck + tests
cd plugins/delendai_player-club-dynasty && bun run typecheck && bun test
```

The plugin imports from `@delendai/core/public`; bun's `link:`
protocol (declared in `package.json#dependencies`) symlinks the local
delendai checkout into `node_modules/@delendai/core` so the import
resolves the way it would in production.
