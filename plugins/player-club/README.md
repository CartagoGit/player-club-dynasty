# `@player-club/plugin`

The project-scoped MCP plugin for **Player Club Dynasty**. It exposes the
football-manager dataset (clubs, squads, season clock) over MCP so any agent
running through `mcp-vertex` can drive the simulation.

> Loaded automatically by the workspace's `.vscode/mcp.json` via
> `--plugins=player-club`. Disable by removing the flag.

## Tools (namespaced `player_club_*` by default)

| Tool | Purpose |
|---|---|
| `player_club_list_clubs` | Enumerate every saved club with stadium, founded year, squad size, average rating. |
| `player_club_get_squad` | Return a club's full squad, ordered by shirt number. |
| `player_club_current_season` | Read the in-progress season number and the count of clubs known to the dataset. |

## Configure (`mcp-vertex.config.json`)

```jsonc
{
	"plugins": {
		"player-club": {
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
# Typecheck
bun run --cwd plugins/player-club typecheck

# Tests
bun run --cwd plugins/player-club test

# Build the dist/ the mcp-vertex host imports at runtime
bun scripts/build-plugin.script.ts player-club
```

The plugin imports from `@mcp-vertex/core/public`; that resolves to
`/home/cartago/_projects/mcp-vertex/packages/core/src/public/index.ts` via
the `tsconfig.json#paths` map. Run `bun run mcp-vertex:doctor` to confirm
the host can resolve and load the plugin end-to-end.