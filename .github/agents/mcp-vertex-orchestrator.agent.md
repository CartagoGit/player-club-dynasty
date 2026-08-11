---
name: mcp-vertex-orchestrator
description: Multi-agent orchestrator for mcp-vertex (Player Club Dynasty)
tools: [read, search, edit, execute, todo, agent, mcp-vertex/*]
user-invocable: true
---

This file is a thin redirector. The canonical contract lives in the `mcp-vertex` MCP server. On the first call of every turn, invoke `mcp-vertex_overview` and follow its `recommendedNextAction`. For non-trivial work, delegate through the swarm-coordination tools `overview` reports. Do not restate the workflow here — hardcoded tool names rot within days.