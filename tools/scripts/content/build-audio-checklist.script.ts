#!/usr/bin/env bun
/**
 * Regenerates docs/audios/REVIEW-CHECKLIST.md from the spec + the on-disk
 * .generate/audios/staging/ state. Run with:
 *   bun tools/scripts/content/build-audio-checklist.script.ts
 *
 * Output format per variant:
 *   #### variant_id
 *   - [ ] OK | - [ ] Bad | - [ ] Re-stage      (VS Code task list - clickable)
 *   <audio controls src="../../.generate/audios/staging/...ogg">     (inline player)
 *   <small>ogg: NNN KB</small>
 *   Commands: bun run game:audio:approve <slot> --variant <id>
 *             bun run game:audio:deprecate ...
 *             xdg-open ...
 */
import { existsSync, readdirSync, statSync, writeFileSync } from "node:fs";

const spec = await Bun.file("game/content/audio/audio-spec.json").json();

const allSlots = spec.groups.flatMap((g) =>
	g.entries.map((e) => ({
		group: g.id,
		groupKind: g.kind,
		entry: e,
		dir: `.generate/audios/staging/${g.id}/${e.id}`,
	})),
);

const isStaged = (s) =>
	existsSync(s.dir) && readdirSync(s.dir).some((f) => f.endsWith(".ogg"));

const stagedSlots = allSlots.filter(isStaged);
const missingSlots = allSlots.filter((s) => !isStaged(s));

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
	"music.workspace.workspace_hub_01": "Workspace hub — variante 1: pizzicato ascendente.",
	"music.workspace.workspace_hub_02": "Workspace hub — variante 2: arpegio descendente.",
	"music.workspace.workspace_hub_03": "Workspace hub — variante 3: pad + Rhodes.",
	"music.workspace.market_browsing_01": "Mercado — variante 1: bajo caminando.",
	"music.workspace.market_browsing_02": "Mercado — variante 2: contra-melodía Rhodes.",
	"music.workspace.market_browsing_03": "Mercado — variante 3: pad–Rhodes alternando.",
	"music.workspace.stadium_management_01": "Estadio — variante 1: ostinato pizzicato.",
	"music.workspace.stadium_management_02": "Estadio — variante 2: acordes cálidos.",
	"music.workspace.stadium_management_03": "Estadio — variante 3: melodía legato.",
	"music.workspace.standings_review_01": "Clasificación — variante 1: pad + Rhodes espaciado.",
	"music.workspace.standings_review_02": "Clasificación — variante 2: línea única Rhodes.",
	"music.workspace.standings_review_03": "Clasificación — variante 3: pad–Rhodes alternando.",
	"music.editor.editor_focus_01": "Editor — variante 1: pad + nota Rhodes.",
	"music.editor.editor_focus_02": "Editor — variante 2: arpegio cada 8 compases.",
	"music.editor.editor_focus_03": "Editor — variante 3: acordes cada 4 compases.",
	"music.trophy.trophy_lift_01": "Trofeo — variante 1: brass sobre pizzicato.",
	"music.trophy.trophy_lift_02": "Trofeo — variante 2: Rhodes melodía.",
	"music.trophy.trophy_lift_03": "Trofeo — variante 3: brass–Rhodes pregunta-respuesta.",
	"sfx.workspace.section_change": "Sidebar — cambio de sección. Whoosh corto.",
	"sfx.workspace.section_hover": "Sidebar — hover. Tick subliminal.",
	"sfx.workspace.section_pin": "Sidebar — pineado. Ping de dos notas.",
	"sfx.editor.tile_place": "Editor — tile colocado. Snap crujiente.",
	"sfx.editor.tile_remove": "Editor — tile retirado. Click-pop.",
	"sfx.editor.tile_rotate": "Editor — tile rotado. Tick de madera.",
	"sfx.editor.canvas_save": "Editor — canvas guardado.",
	"sfx.match_extended.match_start": "Inicio del partido. Silbido + hinchada.",
	"sfx.match_extended.half_time": "Descanso. Dos silbidos.",
	"sfx.match_extended.yellow_card": "Tarjeta amarilla. Silbido + flick.",
	"sfx.match_extended.red_card": "Tarjeta roja. Silbido + ominosidad.",
	"sfx.match_extended.foul": "Falta. Silbido + ohh.",
	"sfx.match_extended.shot_on_target": "Tiro a puerta. Golpe + parada.",
	"sfx.match_extended.shot_off_target": "Tiro fuera. Golpe + thud.",
	"sfx.match_extended.substitution": "Cambio. Ping + murmullo.",
	"sfx.transfer.offer_received": "Oferta recibida. Chime entrante.",
	"sfx.transfer.scout_report": "Informe de ojeador. Papel + campana.",
	"sfx.transfer.transfer_complete": "Fichaje cerrado. Caja registradora.",
	"sfx.transfer.transfer_rejected": "Oferta rechazada. Descendente.",
	"sfx.achievement.manager_of_month": "Manager del mes. Brass 3 notas.",
	"sfx.achievement.season_complete": "Temporada completa. ding-ding-ding.",
	"sfx.achievement.promotion": "Ascenso. Brass brillante.",
	"sfx.achievement.relegation": "Descenso. Chime menor + groan.",
	"sfx.ui.ui_click": "Click UI.",
	"sfx.ui.ui_hover": "Hover UI.",
	"sfx.ui.ui_confirm": "Confirmación UI.",
	"sfx.ui.ui_cancel": "Cancelar UI.",
	"sfx.ui.ui_error": "Error UI.",
	"sfx.ui.ui_notify": "Notificación UI.",
	"sfx.match.whistle_short": "Silbido corto.",
	"sfx.match.whistle_long": "Silbido largo.",
	"sfx.match.whistle_penalty": "Silbido de penalti.",
	"sfx.match.crowd_cheer": "Hinchada. Rugido.",
	"sfx.match.crowd_groan": "Hinchada. Ohhh.",
	"sfx.match.crowd_chant": "Hinchada. Cántico.",
	"sfx.match.kick": "Tiro. Thud + squeak.",
	"sfx.match.ball_bounce": "Balón botando.",
	"sfx.match.post_hit": "Poste. Clang.",
};

const BT = "`";
const workspaceRoot = process.cwd();

function rowFor(s: { group: string; entry: { id: string; prompt?: string }; dir: string }) {
	const descKey = `${s.group}.${s.entry.id}`;
	const desc = descriptions[descKey] ?? s.entry.prompt?.slice(0, 80) ?? "(sin descripción)";
	const dir = s.dir;
	const block: string[] = [];

	const oggs = existsSync(dir) ? readdirSync(dir).filter((f) => f.endsWith(".ogg")) : [];
	if (!oggs.length) {
		block.push(`### ${BT}${s.group}.${s.entry.id}${BT} — ❌ NOT STAGED`);
		block.push("");
		block.push(`> ${desc}`);
		block.push("");
		block.push(`- ${BT}bun run game:audio:stage ${s.group}.${s.entry.id}${BT}`);
		block.push("");
		return block.join("\n");
	}

	block.push(`### ${BT}${s.group}.${s.entry.id}${BT}`);
	block.push("");
	block.push(`> ${desc}`);
	block.push("");

	for (const o of oggs) {
		const vid = o.replace(/\.ogg$/, "");
		const oggPath = `${dir}/${o}`;
		// The md lives at docs/audios/REVIEW-CHECKLIST.md, so audio srcs must
		// be resolved from THAT location. Going up two levels lands at the
		// workspace root, where .generate/audios/staging/ actually lives.
		const oggRelFromMd = `../../.generate/audios/staging/${s.group}/${s.entry.id}/${o}`;
		// xdg-open runs from the workspace root, so this stays as-is.
		const oggRel = `.generate/audios/staging/${s.group}/${s.entry.id}/${o}`;
		const oggSize = `${(statSync(oggPath).size / 1024).toFixed(0)} KB`;

		block.push(`#### ${BT}${vid}${BT}`);
		block.push("");
		// Inline <audio> in VS Code preview on WSL has no sound (Chromium
		// webview sandbox blocks audio output). We keep the tag because
		// (1) it shows progress & duration visually, and (2) it works in
		// any real browser (Ctrl+Shift+L "Open in browser"). The visualizer
		// below is the workaround for WSL preview: it scrolls a bar tied
		// to the audio's currentTime, so the user knows the file is playing
		// even when no sound comes out of the preview.
		block.push(`<audio id="aud-${vid}" controls preload="none" src="${oggRelFromMd}" style="width:100%;max-width:600px;height:32px;"></audio>`);
		block.push("");
		block.push(`<div id="viz-${vid}" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-${vid}" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>`);
		block.push("");
		block.push(`<a href="${oggRelFromMd}" download="${vid}.ogg">⬇ ${oggSize}</a> &nbsp; `);
		block.push(`<small>ogg: ${oggSize} &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>`);
		block.push("");
		block.push("");
		block.push(`<form>`);
		block.push(`<label><input type="checkbox" name="${vid}-ok"> OK</label> &nbsp;`);
		block.push(`<label><input type="checkbox" name="${vid}-bad"> Bad</label> &nbsp;`);
		block.push(`<label><input type="checkbox" name="${vid}-restage"> Re-stage</label>`);
		block.push(`</form>`);
		block.push("");
		block.push("Commands (copy-paste into terminal):");
		block.push("");
		// Get the current WSL IP so the link works from the Windows browser
		// (Brave/Chrome). The user starts the server with
		// `bun run game:audio:review:ui` — that command auto-opens the
		// page in their default browser. From any variant row in this md,
		// they can click "listen (browser)" to open that single .ogg in a
		// tab with audio.
		const wslIp = (() => {
			try {
				// hostname -I returns the IPs of this WSL instance. The
				// first one is the eth0 address reachable from Windows.
				const r = Bun.spawnSync(["sh", "-c", "hostname -I | awk '{print $1}'"], { stdout: "pipe" });
				const ip = new TextDecoder().decode(r.stdout).trim();
				if (ip) return ip;
			} catch {}
			return "localhost";
		})();
		block.push(`- ▶ listen (browser, recomendado): abre [esta URL](http://${wslIp}:7333/track/${s.group}/${s.entry.id}/${vid}.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con ${BT}bun run game:audio:review:ui${BT}`);
		block.push(`- ▶ listen (terminal):  ${BT}PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit ${oggRel}${BT}`);
		block.push(`- approve:   ${BT}bun run game:audio:approve ${s.group}.${s.entry.id} --variant ${vid}${BT}`);
		block.push(`- deprecate: ${BT}bun run game:audio:deprecate ${s.group}.${s.entry.id} --variant ${vid}${BT}`);
		block.push(`- re-stage:  ${BT}bun run game:audio:stage ${s.group}.${s.entry.id}${BT}`);
		block.push("");
		block.push(`Notes:`);
		block.push("");
		block.push("```");
		block.push("");
		block.push("---");
		block.push("");
	}
	return block.join("\n");
}

const out: string[] = [];
out.push("# Audio review checklist");
out.push("");
out.push(`Estado actual: ${stagedSlots.length}/${allSlots.length} slots staged. ${missingSlots.length} sin generar.`);
out.push("");
out.push("**Cómo usar — elige el método que te funcione:**");
out.push("");
out.push("**Método A — VS Code preview (Ctrl+Shift+V) — flujo recomendado:**");
out.push("");
out.push("1. Abre una terminal (`Ctrl+\\\``) y déjala a la vista al lado del preview.");
out.push("2. En el preview ves cada variant con su `<audio>` (la barra avanza, el visualizador se mueve, pero **NO sale sonido** — es el sandbox de VS Code en WSL).");
out.push("3. Para oír un track, copia el comando `▶ listen` de su fila, pégalo en la terminal y dale Enter. Suena por WSLg → speakers Windows.");
out.push("4. Marca OK / Bad / Re-stage en el checkbox del preview mientras escuchas.");
out.push("");
out.push("**Método B — Solo terminal, sin preview:**");
out.push("");
out.push("- `bun run game:audio:review` — reproduce cada .ogg vía WSLg + ffplay (suena por speakers Windows), escribes o/b/r/s, ejecuta al final con --commit.");
out.push("");
out.push("Cuando termines un grupo, dime **\"aprueba los OK en `<group>`\"** y yo corro `audio_approve` por cada uno.");
out.push("");
out.push("---");
out.push("");

for (const kind of ["music", "sfx"]) {
	const groupsOfKind = spec.groups.filter((g: { kind: string }) => g.kind === kind);
	if (!groupsOfKind.length) continue;
	out.push(`## ${kind === "music" ? "🎵 MÚSICA" : "🔊 SFX"}`);
	out.push("");

	for (const g of groupsOfKind) {
		const slotsInGroup = allSlots.filter((s) => s.group === g.id);
		const staged = slotsInGroup.filter(isStaged).length;
		const total = slotsInGroup.length;
		const badge = staged === total ? "✅ all staged" :
			staged === 0 ? "❌ none staged" :
			`� ${staged}/${total} staged`;
		out.push(`### ${BT}${g.id}${BT} — ${badge}`);
		out.push("");
		out.push(`> ${g.styleAnchor}`);
		out.push("");
		for (const s of slotsInGroup) {
			out.push(rowFor(s));
		}
	}
}

if (missingSlots.length) {
	out.push("---");
	out.push("");
	out.push(`## Missing (${missingSlots.length}) — no staged yet`);
	out.push("");
	for (const s of missingSlots) {
		out.push(`- ${BT}${s.group}.${s.entry.id}${BT} — run ${BT}bun run game:audio:stage ${s.group}.${s.entry.id}${BT}`);
	}
	out.push("");
}

// Global script: drive all the per-variant visualizer bars from their <audio>
// elements. In WSL preview the <audio> tags don't produce sound, but the
// currentTime still ticks when the user clicks play (the webview downloads
// and decodes the file). We surface that progress as a green bar so the
// reviewer can tell the file is being processed.
out.push("<script>");
out.push("document.querySelectorAll('audio[id^=\"aud-\"]').forEach(a => {");
out.push("  const id = a.id.replace('aud-', '');");
out.push("  const bar = document.getElementById('bar-' + id);");
out.push("  if (!bar) return;");
out.push("  a.addEventListener('timeupdate', () => {");
out.push("    const pct = a.duration ? (a.currentTime / a.duration) * 100 : 0;");
out.push("    bar.style.width = pct + '%';");
out.push("  });");
out.push("  a.addEventListener('ended', () => { bar.style.width = '100%'; });");
out.push("  a.addEventListener('pause', () => {});");
out.push("});");
out.push("</script>");

writeFileSync("docs/audios/REVIEW-CHECKLIST.md", out.join("\n") + "\n");
console.log(`OK REVIEW-CHECKLIST.md`);
console.log(`  ${stagedSlots.length} staged · ${missingSlots.length} missing`);
