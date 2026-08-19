#!/usr/bin/env bun
/**
 * serve-audio-review — Tiny local HTTP server that serves the same
 * REVIEW-CHECKLIST.md the build-audio-checklist script produces, but
 * accessed over http://localhost. The browser (Brave/Chrome/Firefox
 * native) gets a real audio device, so the <audio> tags actually play.
 *
 * Use this when you want a real-audio UI in a separate browser tab and
 * the VS Code preview sandbox is being difficult.
 *
 * Usage:
 *   bun tools/scripts/content/audio/serve-review.script.ts                # default port 7333
 *   bun tools/scripts/content/audio/serve-review.script.ts --port=8080
 *   bun tools/scripts/content/audio/serve-review.script.ts --group=sfx.ui # only one group
 *
 * The server:
 *   - lists .ogg files from .generate/audios/staging/
 *   - serves them at /audio/<group>/<entry>/<file>
 *   - serves the md at / (rebuilds on each request to pick up spec changes)
 *
 * Background it with `&` and visit http://localhost:7333 in your browser.
 */

import { existsSync, readdirSync, readFileSync, statSync } from "node:fs";
import { resolve, join } from "node:path";
import { spawn } from "node:child_process";

const ARGS = new Map<string, string>();
for (const arg of Bun.argv.slice(2)) {
	const [k, v] = arg.replace(/^--/, "").split("=");
	ARGS.set(k, v ?? "true");
}

if (ARGS.has("help") || ARGS.has("h")) {
	process.stdout.write(`Usage: bun ${import.meta.file} [options]

  --port=<n>      port (default 7333)
  --group=<id>    only serve one group
  --help, -h

Starts a tiny local HTTP server on http://localhost:<port>:
  /         → md rendered as HTML (auto-refreshed on every request)
 *  /audio/*  → the .ogg files in .generate/audios/staging/

Open the URL in any native browser (Brave/Chrome/Firefox) — the <audio>
tags will play because the browser has a real audio device, unlike the
VS Code preview sandbox.
`);
	process.exit(0);
}

const PORT = Number(ARGS.get("port") ?? "7333");
const GROUP_FILTER = ARGS.get("group");

const workspaceRoot = process.cwd();
const specPath = resolve(workspaceRoot, "game/content/audio/audio-spec.json");
const stagingRoot = resolve(workspaceRoot, ".generate/audios/staging");
const checklistPath = resolve(workspaceRoot, "docs/audios/REVIEW-CHECKLIST.md");

const descriptions: Record<string, string> = {
	"music.menu.menu_main": "Tema de pantalla de inicio. Calmado, optimista, cálido.",
	"music.menu.menu_credits": "Créditos finales. Melancólico pero resuelto.",
	"music.menu.menu_loadsave": "Pantalla de guardar/cargar. Suspendido, ambiental.",
	"music.matchday.matchday_warmup": "Pre-partido. Ambiente de estadio creciendo.",
	"music.matchday.matchday_kickoff": "Momento del pitido inicial. Explosivo pero cálido.",
	"music.matchday.matchday_goal": "Stinger de gol. Motivo triunfal corto.",
	"music.matchday.matchday_fulltime": "Stinger de final. Exhalo aliviado.",
	"music.matchday.matchday_victory": "Stinger de victoria. Brass cálido.",
	"music.matchday.matchday_defeat": "Stinger de derrota. Acorde sostenido.",
};

interface Spec { groups: Array<{ id: string; kind: string; outputDir: string; styleAnchor: string; entries: Array<{ id: string; filename: string }> }> }

function escape(s: string): string {
	return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
}

function buildHtml(): string {
	const spec: Spec = JSON.parse(readFileSync(specPath, "utf8"));
	const rows: string[] = [];
	for (const g of spec.groups) {
		if (GROUP_FILTER && g.id !== GROUP_FILTER) continue;
		for (const e of g.entries) {
			const slotDir = join(stagingRoot, g.id, e.id);
			if (!existsSync(slotDir)) continue;
			const oggs = readdirSync(slotDir).filter((f) => f.endsWith(".ogg"));
			for (const o of oggs) {
				const vid = o.replace(/\.ogg$/, "");
				const size = `${(statSync(join(slotDir, o)).size / 1024).toFixed(0)} KB`;
				const url = `/audio/${g.id}/${e.id}/${o}`;
				rows.push(`
<div class="card">
  <h3>${escape(`${g.id}.${e.id}`)} <span class="vid">${escape(vid)}</span></h3>
  <p class="size">${size}</p>
  <audio controls preload="none" src="${url}"></audio>
  <p class="cmds">
    <a class="listenbtn" target="_blank" href="/track/${escape(g.id)}/${escape(e.id)}/${escape(vid)}.ogg">▶ Open in new tab (focused player)</a>
  </p>
  <p class="cmds">
    <code>bun run game:audio:approve ${g.id}.${e.id} --variant ${vid}</code>
    <code>bun run game:audio:deprecate ${g.id}.${e.id} --variant ${vid}</code>
  </p>
</div>`);
			}
		}
	}
	return `<!doctype html>
<html><head><meta charset="utf-8">
<title>PC Dynasty audio review</title>
<style>
  body { font-family: -apple-system, BlinkMacSystemFont, sans-serif; max-width: 900px; margin: 0 auto; padding: 20px; background: #1a1a1a; color: #eee; }
  h1 { border-bottom: 1px solid #444; padding-bottom: 8px; }
  .card { background: #2a2a2a; border-radius: 8px; padding: 16px; margin: 16px 0; }
  .card h3 { margin: 0 0 4px; font-size: 14px; }
  .vid { color: #4caf50; font-family: monospace; font-size: 12px; }
  .size { color: #888; font-size: 12px; margin: 4px 0; }
  audio { width: 100%; margin-top: 8px; }
  .cmds { font-size: 11px; color: #888; margin-top: 8px; }
  .cmds code { display: block; padding: 2px 4px; background: #111; border-radius: 3px; margin: 2px 0; }
  .listenbtn { display: inline-block; padding: 6px 12px; background: #4caf50; color: #000; text-decoration: none; border-radius: 4px; font-weight: 600; font-size: 12px; }
  .listenbtn:hover { background: #66bb6a; }
</style>
</head><body>
<h1>🎵 PC Dynasty audio review (${rows.length} tracks)</h1>
<p>Each card has a ▶ button that opens a focused single-track player in a new tab — easier than scrolling through 69 inline players.</p>
${rows.join("\n")}
</body></html>`;
}

const server = Bun.serve({
	port: PORT,
	hostname: "0.0.0.0",
	async fetch(req) {
		const url = new URL(req.url);
		if (url.pathname === "/") {
			return new Response(buildHtml(), { headers: { "content-type": "text/html; charset=utf-8" } });
		}
		if (url.pathname.startsWith("/audio/")) {
			// url.pathname is like "/audio/sfx.ui/ui_click/v_xxx.ogg". Strip
			// the leading "/audio/" and resolve under stagingRoot.
			const rel = url.pathname.slice("/audio/".length);
			const filePath = join(stagingRoot, rel);
			// Refuse to escape stagingRoot (basic path traversal guard).
			if (!filePath.startsWith(stagingRoot)) {
				return new Response("forbidden", { status: 403 });
			}
			if (!existsSync(filePath)) return new Response("not found: " + filePath, { status: 404 });
			const file = Bun.file(filePath);
			return new Response(file, { headers: { "content-type": "audio/ogg" } });
		}
		// /track/<group>/<entry>/<file>.ogg → a minimal one-track HTML page
		// that auto-plays the .ogg. This is the URL the "listen (browser)"
		// link in REVIEW-CHECKLIST.md points to. It only renders a single
		// <audio> so the browser doesn't have to load 69 tracks.
		if (url.pathname.startsWith("/track/")) {
			const rel = url.pathname.slice("/track/".length);
			// Strip any ".ogg" suffix or query (we'll let the browser hit
			// the audio URL with the .ogg extension to get the bytes).
			const relNoQuery = rel.split("?")[0];
			const filePath = join(stagingRoot, relNoQuery);
			if (!filePath.startsWith(stagingRoot)) {
				return new Response("forbidden", { status: 403 });
			}
			if (!existsSync(filePath)) return new Response("not found: " + filePath, { status: 404 });
			const audioUrl = `/audio/${relNoQuery}`;
			const title = relNoQuery.replace(/\.ogg$/, "");
			const html = `<!doctype html>
<html><head><meta charset="utf-8"><title>${escape(title)}</title>
<style>
  body { font-family: -apple-system, BlinkMacSystemFont, sans-serif; max-width: 700px; margin: 40px auto; padding: 20px; background: #1a1a1a; color: #eee; text-align: center; }
  h1 { font-size: 18px; word-break: break-all; }
  audio { width: 100%; margin: 20px 0; }
  .cmd { font-size: 11px; color: #888; margin-top: 30px; padding: 10px; background: #111; border-radius: 4px; }
</style></head>
<body>
<h1>${escape(title)}</h1>
<audio controls autoplay preload="metadata" src="${escape(audioUrl)}"></audio>
<p><a href="/" style="color:#4caf50">← back to all 69 tracks</a></p>
<div class="cmd">
To approve: <code>bun run game:audio:approve ${escape(relNoQuery.replace(/\/[^/]+$/, "").replace(/\//g, "."))} --variant ${escape(title.split("/").pop() ?? "")}</code>
</div>
</body></html>`;
			return new Response(html, { headers: { "content-type": "text/html; charset=utf-8" } });
		}
		return new Response("not found", { status: 404 });
	},
});

// WSL2: `localhost` inside WSL ≠ `localhost` in Windows. Find the WSL IP
// so the user can open the server from a Windows-native browser. We need
// the IP of THIS machine's eth0 (not the default gateway — that's the
// Windows host, which won't have our server listening).
function getWslIp(): string {
	try {
		// `hostname -I` returns the IPs assigned to this WSL instance, in
		// the form "172.21.118.36 172.17.0.1 ...". The first one is the
		// eth0 address that Windows can reach.
		const r = Bun.spawnSync(["sh", "-c", "hostname -I | awk '{print $1}'"], { stdout: "pipe" });
		const ip = new TextDecoder().decode(r.stdout).trim();
		if (ip) return ip;
	} catch {}
	try {
		// Fallback: parse the inet line from eth0.
		const r = Bun.spawnSync(["sh", "-c", "ip -4 addr show eth0 | awk '/inet/ {print $2}' | cut -d/ -f1"], { stdout: "pipe" });
		return new TextDecoder().decode(r.stdout).trim();
	} catch {
		return "";
	}
}

const WSL_IP = getWslIp();
process.stdout.write(`✓ Audio review server:\n`);
process.stdout.write(`    WSL:    http://localhost:${server.port}\n`);
if (WSL_IP) {
	process.stdout.write(`    Windows (Brave/Chrome/Firefox nativo): http://${WSL_IP}:${server.port}\n`);
}
process.stdout.write(`  Audio plays because the browser has a real audio device.\n`);
process.stdout.write(`  Press Ctrl+C to stop.\n`);

// Optionally open the browser automatically. Prefer the WSL IP so Windows
// browsers (Brave, Chrome) can reach it.
if (ARGS.has("open")) {
	const url = WSL_IP ? `http://${WSL_IP}:${server.port}` : `http://localhost:${server.port}`;
	const proc = spawn("cmd.exe", ["/c", "start", "", url], {
		detached: true,
		stdio: "ignore",
	});
	proc.unref();
	process.stdout.write(`  Opened ${url} in default browser.\n`);
}
