#!/usr/bin/env bun
/**
 * tools/scripts/workspace/sync-bootstrap.script.ts
 *
 * Sync the workspace's vendored AGENT-BOOTSTRAP.md from the local
 * delendai checkout. Re-run after upgrading the upstream.
 *
 * Usage:
 *   bun tools/scripts/workspace/sync-bootstrap.script.ts
 *   bun tools/scripts/workspace/sync-bootstrap.script.ts --check
 */

import { copyFileSync, existsSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

const args = new Set(process.argv.slice(2));
const wantCheck = args.has('--check');

const rootScript = resolve(import.meta.dirname, 'root.script.ts');
const rootResult = spawnSync('bun', [rootScript], { encoding: 'utf8' });
if (rootResult.status !== 0) {
	process.stderr.write(rootResult.stderr);
	process.exit(rootResult.status ?? 1);
}
const delendaiRoot = rootResult.stdout.trim();

const upstream = `${delendaiRoot}/docs/delendai/AGENT-BOOTSTRAP.md`;
const local = resolve(import.meta.dirname, '../../../docs/delendai/AGENT-BOOTSTRAP.md');

if (!existsSync(upstream)) {
	process.stderr.write(`Upstream bootstrap not found at ${upstream}\n`);
	process.exit(1);
}

const upstreamText = readFileSync(upstream, 'utf8');
const localText = existsSync(local) ? readFileSync(local, 'utf8') : '';

if (upstreamText === localText) {
	process.stdout.write('AGENT-BOOTSTRAP.md is up to date.\n');
	process.exit(0);
}

if (wantCheck) {
	process.stderr.write('AGENT-BOOTSTRAP.md is drifted from upstream. Run `bun tools/scripts/workspace/sync-bootstrap.script.ts`.\n');
	process.exit(1);
}

const preamble = `# Universal agent bootstrap — Player Club Dynasty

> **Source of truth:** this file is a vendored copy of the canonical
> \`docs/delendai/AGENT-BOOTSTRAP.md\` shipped by the local delendai
> checkout at \`${delendaiRoot}\`. When that file changes upstream, re-run
> \`bun tools/scripts/workspace/sync-bootstrap.script.ts\` to refresh this
> copy. Until then, treat this copy as authoritative — it is what host
> instruction files (\`AGENTS.md\`, \`CLAUDE.md\`,
> \`.github/copilot-instructions.md\`, \`.cursor/rules/delendai.md\`,
> \`.codex/config.toml\`, \`.aider.conf.yml\`, \`.continue/config.json\`)
> all point at.
>
> This file is the **only** place agent rules live. Every host instruction
> file in this workspace is a **pointer** to this file.

The server (\`delendai_overview\`, \`delendai_agent_catalog\`,
\`delendai_agent_bootstrap\` prompt) is the **only** source of truth for
what is loaded. The agent must **always** ask the server instead of
guessing from a list, hardcoded id, or copy-pasted previous session.

---

## Table of contents

1. [Orient first — one cheap call](#1-orient-first--one-cheap-call)
2. [Route work — ask the server](#2-route-work--ask-the-server)
3. [Bootstrap prompt — insert when the host supports it](#3-bootstrap-prompt--insert-when-the-host-supports-it)
4. [Workflow loop](#4-workflow-loop)
5. [Definition of done](#5-definition-of-done)
6. [Invariants you must not break](#6-invariants-you-must-not-break)
7. [Repo-level rules (only when the host reads \`AGENTS.md\`)](#7-repo-level-rules--only-when-the-host-reads-agentsmd)
8. [Host appendices](#8-host-appendices)

The remainder of this file is byte-identical to the upstream body. The
auto-generated preamble above is the only divergence.
`;

const upstreamBody = upstreamText.replace(/^# Universal agent bootstrap[\s\S]*?---/u, '').trimStart();
const wrapped = `${preamble}\n${upstreamBody}`;
writeFileSync(local, wrapped);
process.stdout.write(`Synced ${local} from ${upstream}\n`);
