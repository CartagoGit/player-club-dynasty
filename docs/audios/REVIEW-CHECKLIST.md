# Audio review checklist

Estado actual: 0/65 slots staged. 65 sin generar.

**Cómo usar — elige el método que te funcione:**

**Método A — VS Code preview (Ctrl+Shift+V) — flujo recomendado:**

1. Abre una terminal (`Ctrl+\``) y déjala a la vista al lado del preview.
2. En el preview ves cada variant con su `<audio>` (la barra avanza, el visualizador se mueve, pero **NO sale sonido** — es el sandbox de VS Code en WSL).
3. Para oír un track, copia el comando `▶ listen` de su fila, pégalo en la terminal y dale Enter. Suena por WSLg → speakers Windows.
4. Marca OK / Bad / Re-stage en el checkbox del preview mientras escuchas.

**Método B — Solo terminal, sin preview:**

- `bun run game:audio:review` — reproduce cada .ogg vía WSLg + ffplay (suena por speakers Windows), escribes o/b/r/s, ejecuta al final con --commit.

Cuando termines un grupo, dime **"aprueba los OK en `<group>`"** y yo corro `audio_approve` por cada uno.

---

## 🎵 MÚSICA

### `music.menu` — ❌ none staged

> Retro late-1990s Spanish football management PC game menu music. Warm, slightly nostalgic, optimistic but never saccharine.

### `music.menu.menu_main` — ❌ NOT STAGED

> Tema de pantalla de inicio. Calmado, optimista, cálido.

- `bun run game:audio:stage music.menu.menu_main`

### `music.menu.menu_credits` — ❌ NOT STAGED

> Créditos finales. Melancólico pero resuelto.

- `bun run game:audio:stage music.menu.menu_credits`

### `music.menu.menu_loadsave` — ❌ NOT STAGED

> Pantalla de guardar/cargar. Suspendido, ambiental.

- `bun run game:audio:stage music.menu.menu_loadsave`

### `music.matchday` — ❌ none staged

> Retro late-1990s Spanish football management PC game matchday music. Driving, tense, stadium atmosphere. Every track is harmonically compatible with the menu group.

### `music.matchday.matchday_warmup` — ❌ NOT STAGED

> Pre-partido. Ambiente de estadio creciendo.

- `bun run game:audio:stage music.matchday.matchday_warmup`

### `music.matchday.matchday_kickoff` — ❌ NOT STAGED

> Momento del pitido inicial. Explosivo pero cálido.

- `bun run game:audio:stage music.matchday.matchday_kickoff`

### `music.matchday.matchday_goal` — ❌ NOT STAGED

> Stinger de gol. Motivo triunfal corto.

- `bun run game:audio:stage music.matchday.matchday_goal`

### `music.matchday.matchday_fulltime` — ❌ NOT STAGED

> Stinger de final. Exhalo aliviado.

- `bun run game:audio:stage music.matchday.matchday_fulltime`

### `music.matchday.matchday_victory` — ❌ NOT STAGED

> Stinger de victoria. Brass cálido.

- `bun run game:audio:stage music.matchday.matchday_victory`

### `music.matchday.matchday_defeat` — ❌ NOT STAGED

> Stinger de derrota. Acorde sostenido.

- `bun run game:audio:stage music.matchday.matchday_defeat`

### `music.workspace` — ❌ none staged

> Sub-views of the club management workspace (Overview / Market / Stadium / Standings). Same warm 1990s synthwave palette as music.menu but each sub-view has a slightly different mood so the manager feels the context shift.

### `music.workspace.workspace_hub_01` — ❌ NOT STAGED

> Workspace hub — variante 1: pizzicato ascendente.

- `bun run game:audio:stage music.workspace.workspace_hub_01`

### `music.workspace.workspace_hub_02` — ❌ NOT STAGED

> Workspace hub — variante 2: arpegio descendente.

- `bun run game:audio:stage music.workspace.workspace_hub_02`

### `music.workspace.workspace_hub_03` — ❌ NOT STAGED

> Workspace hub — variante 3: pad + Rhodes.

- `bun run game:audio:stage music.workspace.workspace_hub_03`

### `music.workspace.market_browsing_01` — ❌ NOT STAGED

> Mercado — variante 1: bajo caminando.

- `bun run game:audio:stage music.workspace.market_browsing_01`

### `music.workspace.market_browsing_02` — ❌ NOT STAGED

> Mercado — variante 2: contra-melodía Rhodes.

- `bun run game:audio:stage music.workspace.market_browsing_02`

### `music.workspace.market_browsing_03` — ❌ NOT STAGED

> Mercado — variante 3: pad–Rhodes alternando.

- `bun run game:audio:stage music.workspace.market_browsing_03`

### `music.workspace.stadium_management_01` — ❌ NOT STAGED

> Estadio — variante 1: ostinato pizzicato.

- `bun run game:audio:stage music.workspace.stadium_management_01`

### `music.workspace.stadium_management_02` — ❌ NOT STAGED

> Estadio — variante 2: acordes cálidos.

- `bun run game:audio:stage music.workspace.stadium_management_02`

### `music.workspace.stadium_management_03` — ❌ NOT STAGED

> Estadio — variante 3: melodía legato.

- `bun run game:audio:stage music.workspace.stadium_management_03`

### `music.workspace.standings_review_01` — ❌ NOT STAGED

> Clasificación — variante 1: pad + Rhodes espaciado.

- `bun run game:audio:stage music.workspace.standings_review_01`

### `music.workspace.standings_review_02` — ❌ NOT STAGED

> Clasificación — variante 2: línea única Rhodes.

- `bun run game:audio:stage music.workspace.standings_review_02`

### `music.workspace.standings_review_03` — ❌ NOT STAGED

> Clasificación — variante 3: pad–Rhodes alternando.

- `bun run game:audio:stage music.workspace.standings_review_03`

### `music.editor` — ❌ none staged

> Pack editor / creative mode. Sparse and minimal so the player can think — no busy motifs, lots of air. Same warm 1990s synthwave palette as the menu group.

### `music.editor.editor_focus_01` — ❌ NOT STAGED

> Editor — variante 1: pad + nota Rhodes.

- `bun run game:audio:stage music.editor.editor_focus_01`

### `music.editor.editor_focus_02` — ❌ NOT STAGED

> Editor — variante 2: arpegio cada 8 compases.

- `bun run game:audio:stage music.editor.editor_focus_02`

### `music.editor.editor_focus_03` — ❌ NOT STAGED

> Editor — variante 3: acordes cada 4 compases.

- `bun run game:audio:stage music.editor.editor_focus_03`

### `music.trophy` — ❌ none staged

> Trophy / winners screen (league titles, promotions, manager of the month). Same warm 1990s synthwave palette but with a contained brass-like synth lead so it feels triumphant without being bombastic.

### `music.trophy.trophy_lift_01` — ❌ NOT STAGED

> Trofeo — variante 1: brass sobre pizzicato.

- `bun run game:audio:stage music.trophy.trophy_lift_01`

### `music.trophy.trophy_lift_02` — ❌ NOT STAGED

> Trofeo — variante 2: Rhodes melodía.

- `bun run game:audio:stage music.trophy.trophy_lift_02`

### `music.trophy.trophy_lift_03` — ❌ NOT STAGED

> Trofeo — variante 3: brass–Rhodes pregunta-respuesta.

- `bun run game:audio:stage music.trophy.trophy_lift_03`

## 🔊 SFX

### `sfx.ui` — ❌ none staged

> Retro 1990s PC speaker UI sound. Short, crisp, synthesized, dry. Should sound like they all came from the same sound chip.

### `sfx.ui.ui_click` — ❌ NOT STAGED

> Click UI.

- `bun run game:audio:stage sfx.ui.ui_click`

### `sfx.ui.ui_hover` — ❌ NOT STAGED

> Hover UI.

- `bun run game:audio:stage sfx.ui.ui_hover`

### `sfx.ui.ui_confirm` — ❌ NOT STAGED

> Confirmación UI.

- `bun run game:audio:stage sfx.ui.ui_confirm`

### `sfx.ui.ui_cancel` — ❌ NOT STAGED

> Cancelar UI.

- `bun run game:audio:stage sfx.ui.ui_cancel`

### `sfx.ui.ui_error` — ❌ NOT STAGED

> Error UI.

- `bun run game:audio:stage sfx.ui.ui_error`

### `sfx.ui.ui_notify` — ❌ NOT STAGED

> Notificación UI.

- `bun run game:audio:stage sfx.ui.ui_notify`

### `sfx.match` — ❌ none staged

> Retro 1990s football management PC game stadium sound effects. Should sound lo-fi but unmistakable — they are what the manager hears from the stands.

### `sfx.match.whistle_short` — ❌ NOT STAGED

> Silbido corto.

- `bun run game:audio:stage sfx.match.whistle_short`

### `sfx.match.whistle_long` — ❌ NOT STAGED

> Silbido largo.

- `bun run game:audio:stage sfx.match.whistle_long`

### `sfx.match.whistle_penalty` — ❌ NOT STAGED

> Silbido de penalti.

- `bun run game:audio:stage sfx.match.whistle_penalty`

### `sfx.match.crowd_cheer` — ❌ NOT STAGED

> Hinchada. Rugido.

- `bun run game:audio:stage sfx.match.crowd_cheer`

### `sfx.match.crowd_groan` — ❌ NOT STAGED

> Hinchada. Ohhh.

- `bun run game:audio:stage sfx.match.crowd_groan`

### `sfx.match.crowd_chant` — ❌ NOT STAGED

> Hinchada. Cántico.

- `bun run game:audio:stage sfx.match.crowd_chant`

### `sfx.match.kick` — ❌ NOT STAGED

> Tiro. Thud + squeak.

- `bun run game:audio:stage sfx.match.kick`

### `sfx.match.ball_bounce` — ❌ NOT STAGED

> Balón botando.

- `bun run game:audio:stage sfx.match.ball_bounce`

### `sfx.match.post_hit` — ❌ NOT STAGED

> Poste. Clang.

- `bun run game:audio:stage sfx.match.post_hit`

### `sfx.workspace` — ❌ none staged

> Section navigation in the workspace sidebar (Overview / Squad / Tactics / Competitions / Market / Finances / World). Should feel cohesive with sfx.ui but slightly more deliberate — these are larger transitions.

### `sfx.workspace.section_change` — ❌ NOT STAGED

> Sidebar — cambio de sección. Whoosh corto.

- `bun run game:audio:stage sfx.workspace.section_change`

### `sfx.workspace.section_hover` — ❌ NOT STAGED

> Sidebar — hover. Tick subliminal.

- `bun run game:audio:stage sfx.workspace.section_hover`

### `sfx.workspace.section_pin` — ❌ NOT STAGED

> Sidebar — pineado. Ping de dos notas.

- `bun run game:audio:stage sfx.workspace.section_pin`

### `sfx.editor` — ❌ none staged

> Pack editor — placing, rotating and removing tiles / cards / stickers on the canvas. Crispy, satisfying, retro-gamey.

### `sfx.editor.tile_place` — ❌ NOT STAGED

> Editor — tile colocado. Snap crujiente.

- `bun run game:audio:stage sfx.editor.tile_place`

### `sfx.editor.tile_remove` — ❌ NOT STAGED

> Editor — tile retirado. Click-pop.

- `bun run game:audio:stage sfx.editor.tile_remove`

### `sfx.editor.tile_rotate` — ❌ NOT STAGED

> Editor — tile rotado. Tick de madera.

- `bun run game:audio:stage sfx.editor.tile_rotate`

### `sfx.editor.canvas_save` — ❌ NOT STAGED

> Editor — canvas guardado.

- `bun run game:audio:stage sfx.editor.canvas_save`

### `sfx.match_extended` — ❌ none staged

> Extended match commentary stingers: cards, fouls, shots, saves, substitutions. Each event needs a short, identifiable sting that the manager associates with that exact play.

### `sfx.match_extended.match_start` — ❌ NOT STAGED

> Inicio del partido. Silbido + hinchada.

- `bun run game:audio:stage sfx.match_extended.match_start`

### `sfx.match_extended.half_time` — ❌ NOT STAGED

> Descanso. Dos silbidos.

- `bun run game:audio:stage sfx.match_extended.half_time`

### `sfx.match_extended.yellow_card` — ❌ NOT STAGED

> Tarjeta amarilla. Silbido + flick.

- `bun run game:audio:stage sfx.match_extended.yellow_card`

### `sfx.match_extended.red_card` — ❌ NOT STAGED

> Tarjeta roja. Silbido + ominosidad.

- `bun run game:audio:stage sfx.match_extended.red_card`

### `sfx.match_extended.foul` — ❌ NOT STAGED

> Falta. Silbido + ohh.

- `bun run game:audio:stage sfx.match_extended.foul`

### `sfx.match_extended.shot_on_target` — ❌ NOT STAGED

> Tiro a puerta. Golpe + parada.

- `bun run game:audio:stage sfx.match_extended.shot_on_target`

### `sfx.match_extended.shot_off_target` — ❌ NOT STAGED

> Tiro fuera. Golpe + thud.

- `bun run game:audio:stage sfx.match_extended.shot_off_target`

### `sfx.match_extended.substitution` — ❌ NOT STAGED

> Cambio. Ping + murmullo.

- `bun run game:audio:stage sfx.match_extended.substitution`

### `sfx.transfer` — ❌ none staged

> Transfer market stingers — incoming offer, scout report, completed deal. Lo-fi retro chime-ish to fit the 1990s chip aesthetic but with a cash-register undertone for deals.

### `sfx.transfer.offer_received` — ❌ NOT STAGED

> Oferta recibida. Chime entrante.

- `bun run game:audio:stage sfx.transfer.offer_received`

### `sfx.transfer.scout_report` — ❌ NOT STAGED

> Informe de ojeador. Papel + campana.

- `bun run game:audio:stage sfx.transfer.scout_report`

### `sfx.transfer.transfer_complete` — ❌ NOT STAGED

> Fichaje cerrado. Caja registradora.

- `bun run game:audio:stage sfx.transfer.transfer_complete`

### `sfx.transfer.transfer_rejected` — ❌ NOT STAGED

> Oferta rechazada. Descendente.

- `bun run game:audio:stage sfx.transfer.transfer_rejected`

### `sfx.achievement` — ❌ none staged

> Career achievement stingers — manager of the month, season complete, promotion, relegation, trophy lift. Each is a short fanfare that complements music.trophy.trophy_lift without overshadowing it.

### `sfx.achievement.manager_of_month` — ❌ NOT STAGED

> Manager del mes. Brass 3 notas.

- `bun run game:audio:stage sfx.achievement.manager_of_month`

### `sfx.achievement.season_complete` — ❌ NOT STAGED

> Temporada completa. ding-ding-ding.

- `bun run game:audio:stage sfx.achievement.season_complete`

### `sfx.achievement.promotion` — ❌ NOT STAGED

> Ascenso. Brass brillante.

- `bun run game:audio:stage sfx.achievement.promotion`

### `sfx.achievement.relegation` — ❌ NOT STAGED

> Descenso. Chime menor + groan.

- `bun run game:audio:stage sfx.achievement.relegation`

---

## Missing (65) — no staged yet

- `music.menu.menu_main` — run `bun run game:audio:stage music.menu.menu_main`
- `music.menu.menu_credits` — run `bun run game:audio:stage music.menu.menu_credits`
- `music.menu.menu_loadsave` — run `bun run game:audio:stage music.menu.menu_loadsave`
- `music.matchday.matchday_warmup` — run `bun run game:audio:stage music.matchday.matchday_warmup`
- `music.matchday.matchday_kickoff` — run `bun run game:audio:stage music.matchday.matchday_kickoff`
- `music.matchday.matchday_goal` — run `bun run game:audio:stage music.matchday.matchday_goal`
- `music.matchday.matchday_fulltime` — run `bun run game:audio:stage music.matchday.matchday_fulltime`
- `music.matchday.matchday_victory` — run `bun run game:audio:stage music.matchday.matchday_victory`
- `music.matchday.matchday_defeat` — run `bun run game:audio:stage music.matchday.matchday_defeat`
- `sfx.ui.ui_click` — run `bun run game:audio:stage sfx.ui.ui_click`
- `sfx.ui.ui_hover` — run `bun run game:audio:stage sfx.ui.ui_hover`
- `sfx.ui.ui_confirm` — run `bun run game:audio:stage sfx.ui.ui_confirm`
- `sfx.ui.ui_cancel` — run `bun run game:audio:stage sfx.ui.ui_cancel`
- `sfx.ui.ui_error` — run `bun run game:audio:stage sfx.ui.ui_error`
- `sfx.ui.ui_notify` — run `bun run game:audio:stage sfx.ui.ui_notify`
- `sfx.match.whistle_short` — run `bun run game:audio:stage sfx.match.whistle_short`
- `sfx.match.whistle_long` — run `bun run game:audio:stage sfx.match.whistle_long`
- `sfx.match.whistle_penalty` — run `bun run game:audio:stage sfx.match.whistle_penalty`
- `sfx.match.crowd_cheer` — run `bun run game:audio:stage sfx.match.crowd_cheer`
- `sfx.match.crowd_groan` — run `bun run game:audio:stage sfx.match.crowd_groan`
- `sfx.match.crowd_chant` — run `bun run game:audio:stage sfx.match.crowd_chant`
- `sfx.match.kick` — run `bun run game:audio:stage sfx.match.kick`
- `sfx.match.ball_bounce` — run `bun run game:audio:stage sfx.match.ball_bounce`
- `sfx.match.post_hit` — run `bun run game:audio:stage sfx.match.post_hit`
- `music.workspace.workspace_hub_01` — run `bun run game:audio:stage music.workspace.workspace_hub_01`
- `music.workspace.workspace_hub_02` — run `bun run game:audio:stage music.workspace.workspace_hub_02`
- `music.workspace.workspace_hub_03` — run `bun run game:audio:stage music.workspace.workspace_hub_03`
- `music.workspace.market_browsing_01` — run `bun run game:audio:stage music.workspace.market_browsing_01`
- `music.workspace.market_browsing_02` — run `bun run game:audio:stage music.workspace.market_browsing_02`
- `music.workspace.market_browsing_03` — run `bun run game:audio:stage music.workspace.market_browsing_03`
- `music.workspace.stadium_management_01` — run `bun run game:audio:stage music.workspace.stadium_management_01`
- `music.workspace.stadium_management_02` — run `bun run game:audio:stage music.workspace.stadium_management_02`
- `music.workspace.stadium_management_03` — run `bun run game:audio:stage music.workspace.stadium_management_03`
- `music.workspace.standings_review_01` — run `bun run game:audio:stage music.workspace.standings_review_01`
- `music.workspace.standings_review_02` — run `bun run game:audio:stage music.workspace.standings_review_02`
- `music.workspace.standings_review_03` — run `bun run game:audio:stage music.workspace.standings_review_03`
- `music.editor.editor_focus_01` — run `bun run game:audio:stage music.editor.editor_focus_01`
- `music.editor.editor_focus_02` — run `bun run game:audio:stage music.editor.editor_focus_02`
- `music.editor.editor_focus_03` — run `bun run game:audio:stage music.editor.editor_focus_03`
- `music.trophy.trophy_lift_01` — run `bun run game:audio:stage music.trophy.trophy_lift_01`
- `music.trophy.trophy_lift_02` — run `bun run game:audio:stage music.trophy.trophy_lift_02`
- `music.trophy.trophy_lift_03` — run `bun run game:audio:stage music.trophy.trophy_lift_03`
- `sfx.workspace.section_change` — run `bun run game:audio:stage sfx.workspace.section_change`
- `sfx.workspace.section_hover` — run `bun run game:audio:stage sfx.workspace.section_hover`
- `sfx.workspace.section_pin` — run `bun run game:audio:stage sfx.workspace.section_pin`
- `sfx.editor.tile_place` — run `bun run game:audio:stage sfx.editor.tile_place`
- `sfx.editor.tile_remove` — run `bun run game:audio:stage sfx.editor.tile_remove`
- `sfx.editor.tile_rotate` — run `bun run game:audio:stage sfx.editor.tile_rotate`
- `sfx.editor.canvas_save` — run `bun run game:audio:stage sfx.editor.canvas_save`
- `sfx.match_extended.match_start` — run `bun run game:audio:stage sfx.match_extended.match_start`
- `sfx.match_extended.half_time` — run `bun run game:audio:stage sfx.match_extended.half_time`
- `sfx.match_extended.yellow_card` — run `bun run game:audio:stage sfx.match_extended.yellow_card`
- `sfx.match_extended.red_card` — run `bun run game:audio:stage sfx.match_extended.red_card`
- `sfx.match_extended.foul` — run `bun run game:audio:stage sfx.match_extended.foul`
- `sfx.match_extended.shot_on_target` — run `bun run game:audio:stage sfx.match_extended.shot_on_target`
- `sfx.match_extended.shot_off_target` — run `bun run game:audio:stage sfx.match_extended.shot_off_target`
- `sfx.match_extended.substitution` — run `bun run game:audio:stage sfx.match_extended.substitution`
- `sfx.transfer.offer_received` — run `bun run game:audio:stage sfx.transfer.offer_received`
- `sfx.transfer.scout_report` — run `bun run game:audio:stage sfx.transfer.scout_report`
- `sfx.transfer.transfer_complete` — run `bun run game:audio:stage sfx.transfer.transfer_complete`
- `sfx.transfer.transfer_rejected` — run `bun run game:audio:stage sfx.transfer.transfer_rejected`
- `sfx.achievement.manager_of_month` — run `bun run game:audio:stage sfx.achievement.manager_of_month`
- `sfx.achievement.season_complete` — run `bun run game:audio:stage sfx.achievement.season_complete`
- `sfx.achievement.promotion` — run `bun run game:audio:stage sfx.achievement.promotion`
- `sfx.achievement.relegation` — run `bun run game:audio:stage sfx.achievement.relegation`

<script>
document.querySelectorAll('audio[id^="aud-"]').forEach(a => {
  const id = a.id.replace('aud-', '');
  const bar = document.getElementById('bar-' + id);
  if (!bar) return;
  a.addEventListener('timeupdate', () => {
    const pct = a.duration ? (a.currentTime / a.duration) * 100 : 0;
    bar.style.width = pct + '%';
  });
  a.addEventListener('ended', () => { bar.style.width = '100%'; });
  a.addEventListener('pause', () => {});
});
</script>
