# Audio review checklist

Estado actual: 65/65 slots staged. 0 sin generar.

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

### `music.menu` — ✅ all staged

> Retro late-1990s Spanish football management PC game menu music. Warm, slightly nostalgic, optimistic but never saccharine.

### `music.menu.menu_main`

> Tema de pantalla de inicio. Calmado, optimista, cálido.

#### `v_1787099553943`

<audio id="aud-v_1787099553943" controls preload="none" src="../../.generate/audios/staging/music.menu/menu_main/v_1787099553943.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099553943" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099553943" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.menu/menu_main/v_1787099553943.ogg" download="v_1787099553943.ogg">⬇ 764 KB</a> &nbsp; 
<small>ogg: 764 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099553943-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099553943-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099553943-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.menu/menu_main/v_1787099553943.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.menu/menu_main/v_1787099553943.ogg`
- approve:   `bun run game:audio:approve music.menu.menu_main --variant v_1787099553943`
- deprecate: `bun run game:audio:deprecate music.menu.menu_main --variant v_1787099553943`
- re-stage:  `bun run game:audio:stage music.menu.menu_main`

Notes:

```

---

#### `v_1787140280097`

<audio id="aud-v_1787140280097" controls preload="none" src="../../.generate/audios/staging/music.menu/menu_main/v_1787140280097.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787140280097" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787140280097" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.menu/menu_main/v_1787140280097.ogg" download="v_1787140280097.ogg">⬇ 880 KB</a> &nbsp; 
<small>ogg: 880 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787140280097-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787140280097-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787140280097-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.menu/menu_main/v_1787140280097.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.menu/menu_main/v_1787140280097.ogg`
- approve:   `bun run game:audio:approve music.menu.menu_main --variant v_1787140280097`
- deprecate: `bun run game:audio:deprecate music.menu.menu_main --variant v_1787140280097`
- re-stage:  `bun run game:audio:stage music.menu.menu_main`

Notes:

```

---

### `music.menu.menu_credits`

> Créditos finales. Melancólico pero resuelto.

#### `v_1787099383649`

<audio id="aud-v_1787099383649" controls preload="none" src="../../.generate/audios/staging/music.menu/menu_credits/v_1787099383649.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099383649" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099383649" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.menu/menu_credits/v_1787099383649.ogg" download="v_1787099383649.ogg">⬇ 1231 KB</a> &nbsp; 
<small>ogg: 1231 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099383649-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099383649-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099383649-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.menu/menu_credits/v_1787099383649.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.menu/menu_credits/v_1787099383649.ogg`
- approve:   `bun run game:audio:approve music.menu.menu_credits --variant v_1787099383649`
- deprecate: `bun run game:audio:deprecate music.menu.menu_credits --variant v_1787099383649`
- re-stage:  `bun run game:audio:stage music.menu.menu_credits`

Notes:

```

---

### `music.menu.menu_loadsave`

> Pantalla de guardar/cargar. Suspendido, ambiental.

#### `v_1787099383649`

<audio id="aud-v_1787099383649" controls preload="none" src="../../.generate/audios/staging/music.menu/menu_loadsave/v_1787099383649.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099383649" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099383649" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.menu/menu_loadsave/v_1787099383649.ogg" download="v_1787099383649.ogg">⬇ 1150 KB</a> &nbsp; 
<small>ogg: 1150 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099383649-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099383649-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099383649-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.menu/menu_loadsave/v_1787099383649.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.menu/menu_loadsave/v_1787099383649.ogg`
- approve:   `bun run game:audio:approve music.menu.menu_loadsave --variant v_1787099383649`
- deprecate: `bun run game:audio:deprecate music.menu.menu_loadsave --variant v_1787099383649`
- re-stage:  `bun run game:audio:stage music.menu.menu_loadsave`

Notes:

```

---

### `music.matchday` — ✅ all staged

> Retro late-1990s Spanish football management PC game matchday music. Driving, tense, stadium atmosphere. Every track is harmonically compatible with the menu group.

### `music.matchday.matchday_warmup`

> Pre-partido. Ambiente de estadio creciendo.

#### `v_1787099674023`

<audio id="aud-v_1787099674023" controls preload="none" src="../../.generate/audios/staging/music.matchday/matchday_warmup/v_1787099674023.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099674023" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099674023" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.matchday/matchday_warmup/v_1787099674023.ogg" download="v_1787099674023.ogg">⬇ 908 KB</a> &nbsp; 
<small>ogg: 908 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099674023-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099674023-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099674023-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.matchday/matchday_warmup/v_1787099674023.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.matchday/matchday_warmup/v_1787099674023.ogg`
- approve:   `bun run game:audio:approve music.matchday.matchday_warmup --variant v_1787099674023`
- deprecate: `bun run game:audio:deprecate music.matchday.matchday_warmup --variant v_1787099674023`
- re-stage:  `bun run game:audio:stage music.matchday.matchday_warmup`

Notes:

```

---

### `music.matchday.matchday_kickoff`

> Momento del pitido inicial. Explosivo pero cálido.

#### `v_1787099674024`

<audio id="aud-v_1787099674024" controls preload="none" src="../../.generate/audios/staging/music.matchday/matchday_kickoff/v_1787099674024.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099674024" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099674024" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.matchday/matchday_kickoff/v_1787099674024.ogg" download="v_1787099674024.ogg">⬇ 944 KB</a> &nbsp; 
<small>ogg: 944 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099674024-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099674024-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099674024-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.matchday/matchday_kickoff/v_1787099674024.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.matchday/matchday_kickoff/v_1787099674024.ogg`
- approve:   `bun run game:audio:approve music.matchday.matchday_kickoff --variant v_1787099674024`
- deprecate: `bun run game:audio:deprecate music.matchday.matchday_kickoff --variant v_1787099674024`
- re-stage:  `bun run game:audio:stage music.matchday.matchday_kickoff`

Notes:

```

---

### `music.matchday.matchday_goal`

> Stinger de gol. Motivo triunfal corto.

#### `v_1787099674023`

<audio id="aud-v_1787099674023" controls preload="none" src="../../.generate/audios/staging/music.matchday/matchday_goal/v_1787099674023.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099674023" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099674023" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.matchday/matchday_goal/v_1787099674023.ogg" download="v_1787099674023.ogg">⬇ 980 KB</a> &nbsp; 
<small>ogg: 980 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099674023-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099674023-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099674023-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.matchday/matchday_goal/v_1787099674023.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.matchday/matchday_goal/v_1787099674023.ogg`
- approve:   `bun run game:audio:approve music.matchday.matchday_goal --variant v_1787099674023`
- deprecate: `bun run game:audio:deprecate music.matchday.matchday_goal --variant v_1787099674023`
- re-stage:  `bun run game:audio:stage music.matchday.matchday_goal`

Notes:

```

---

### `music.matchday.matchday_fulltime`

> Stinger de final. Exhalo aliviado.

#### `v_1787099828673`

<audio id="aud-v_1787099828673" controls preload="none" src="../../.generate/audios/staging/music.matchday/matchday_fulltime/v_1787099828673.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099828673" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099828673" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.matchday/matchday_fulltime/v_1787099828673.ogg" download="v_1787099828673.ogg">⬇ 1070 KB</a> &nbsp; 
<small>ogg: 1070 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099828673-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099828673-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099828673-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.matchday/matchday_fulltime/v_1787099828673.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.matchday/matchday_fulltime/v_1787099828673.ogg`
- approve:   `bun run game:audio:approve music.matchday.matchday_fulltime --variant v_1787099828673`
- deprecate: `bun run game:audio:deprecate music.matchday.matchday_fulltime --variant v_1787099828673`
- re-stage:  `bun run game:audio:stage music.matchday.matchday_fulltime`

Notes:

```

---

### `music.matchday.matchday_victory`

> Stinger de victoria. Brass cálido.

#### `v_1787099828677`

<audio id="aud-v_1787099828677" controls preload="none" src="../../.generate/audios/staging/music.matchday/matchday_victory/v_1787099828677.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099828677" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099828677" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.matchday/matchday_victory/v_1787099828677.ogg" download="v_1787099828677.ogg">⬇ 1280 KB</a> &nbsp; 
<small>ogg: 1280 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099828677-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099828677-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099828677-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.matchday/matchday_victory/v_1787099828677.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.matchday/matchday_victory/v_1787099828677.ogg`
- approve:   `bun run game:audio:approve music.matchday.matchday_victory --variant v_1787099828677`
- deprecate: `bun run game:audio:deprecate music.matchday.matchday_victory --variant v_1787099828677`
- re-stage:  `bun run game:audio:stage music.matchday.matchday_victory`

Notes:

```

---

### `music.matchday.matchday_defeat`

> Stinger de derrota. Acorde sostenido.

#### `v_1787099828674`

<audio id="aud-v_1787099828674" controls preload="none" src="../../.generate/audios/staging/music.matchday/matchday_defeat/v_1787099828674.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787099828674" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787099828674" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.matchday/matchday_defeat/v_1787099828674.ogg" download="v_1787099828674.ogg">⬇ 1118 KB</a> &nbsp; 
<small>ogg: 1118 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787099828674-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787099828674-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787099828674-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.matchday/matchday_defeat/v_1787099828674.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.matchday/matchday_defeat/v_1787099828674.ogg`
- approve:   `bun run game:audio:approve music.matchday.matchday_defeat --variant v_1787099828674`
- deprecate: `bun run game:audio:deprecate music.matchday.matchday_defeat --variant v_1787099828674`
- re-stage:  `bun run game:audio:stage music.matchday.matchday_defeat`

Notes:

```

---

### `music.workspace` — ✅ all staged

> Sub-views of the club management workspace (Overview / Market / Stadium / Standings). Same warm 1990s synthwave palette as music.menu but each sub-view has a slightly different mood so the manager feels the context shift.

### `music.workspace.workspace_hub_01`

> Workspace hub — variante 1: pizzicato ascendente.

#### `v_1787100006926`

<audio id="aud-v_1787100006926" controls preload="none" src="../../.generate/audios/staging/music.workspace/workspace_hub_01/v_1787100006926.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100006926" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100006926" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/workspace_hub_01/v_1787100006926.ogg" download="v_1787100006926.ogg">⬇ 806 KB</a> &nbsp; 
<small>ogg: 806 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100006926-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100006926-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100006926-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/workspace_hub_01/v_1787100006926.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/workspace_hub_01/v_1787100006926.ogg`
- approve:   `bun run game:audio:approve music.workspace.workspace_hub_01 --variant v_1787100006926`
- deprecate: `bun run game:audio:deprecate music.workspace.workspace_hub_01 --variant v_1787100006926`
- re-stage:  `bun run game:audio:stage music.workspace.workspace_hub_01`

Notes:

```

---

### `music.workspace.workspace_hub_02`

> Workspace hub — variante 2: arpegio descendente.

#### `v_1787100006926`

<audio id="aud-v_1787100006926" controls preload="none" src="../../.generate/audios/staging/music.workspace/workspace_hub_02/v_1787100006926.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100006926" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100006926" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/workspace_hub_02/v_1787100006926.ogg" download="v_1787100006926.ogg">⬇ 1097 KB</a> &nbsp; 
<small>ogg: 1097 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100006926-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100006926-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100006926-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/workspace_hub_02/v_1787100006926.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/workspace_hub_02/v_1787100006926.ogg`
- approve:   `bun run game:audio:approve music.workspace.workspace_hub_02 --variant v_1787100006926`
- deprecate: `bun run game:audio:deprecate music.workspace.workspace_hub_02 --variant v_1787100006926`
- re-stage:  `bun run game:audio:stage music.workspace.workspace_hub_02`

Notes:

```

---

### `music.workspace.workspace_hub_03`

> Workspace hub — variante 3: pad + Rhodes.

#### `v_1787100006926`

<audio id="aud-v_1787100006926" controls preload="none" src="../../.generate/audios/staging/music.workspace/workspace_hub_03/v_1787100006926.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100006926" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100006926" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/workspace_hub_03/v_1787100006926.ogg" download="v_1787100006926.ogg">⬇ 944 KB</a> &nbsp; 
<small>ogg: 944 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100006926-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100006926-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100006926-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/workspace_hub_03/v_1787100006926.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/workspace_hub_03/v_1787100006926.ogg`
- approve:   `bun run game:audio:approve music.workspace.workspace_hub_03 --variant v_1787100006926`
- deprecate: `bun run game:audio:deprecate music.workspace.workspace_hub_03 --variant v_1787100006926`
- re-stage:  `bun run game:audio:stage music.workspace.workspace_hub_03`

Notes:

```

---

### `music.workspace.market_browsing_01`

> Mercado — variante 1: bajo caminando.

#### `v_1787100170011`

<audio id="aud-v_1787100170011" controls preload="none" src="../../.generate/audios/staging/music.workspace/market_browsing_01/v_1787100170011.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100170011" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100170011" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/market_browsing_01/v_1787100170011.ogg" download="v_1787100170011.ogg">⬇ 526 KB</a> &nbsp; 
<small>ogg: 526 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100170011-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100170011-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100170011-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/market_browsing_01/v_1787100170011.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/market_browsing_01/v_1787100170011.ogg`
- approve:   `bun run game:audio:approve music.workspace.market_browsing_01 --variant v_1787100170011`
- deprecate: `bun run game:audio:deprecate music.workspace.market_browsing_01 --variant v_1787100170011`
- re-stage:  `bun run game:audio:stage music.workspace.market_browsing_01`

Notes:

```

---

### `music.workspace.market_browsing_02`

> Mercado — variante 2: contra-melodía Rhodes.

#### `v_1787100170010`

<audio id="aud-v_1787100170010" controls preload="none" src="../../.generate/audios/staging/music.workspace/market_browsing_02/v_1787100170010.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100170010" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100170010" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/market_browsing_02/v_1787100170010.ogg" download="v_1787100170010.ogg">⬇ 1226 KB</a> &nbsp; 
<small>ogg: 1226 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100170010-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100170010-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100170010-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/market_browsing_02/v_1787100170010.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/market_browsing_02/v_1787100170010.ogg`
- approve:   `bun run game:audio:approve music.workspace.market_browsing_02 --variant v_1787100170010`
- deprecate: `bun run game:audio:deprecate music.workspace.market_browsing_02 --variant v_1787100170010`
- re-stage:  `bun run game:audio:stage music.workspace.market_browsing_02`

Notes:

```

---

### `music.workspace.market_browsing_03`

> Mercado — variante 3: pad–Rhodes alternando.

#### `v_1787100170011`

<audio id="aud-v_1787100170011" controls preload="none" src="../../.generate/audios/staging/music.workspace/market_browsing_03/v_1787100170011.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100170011" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100170011" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/market_browsing_03/v_1787100170011.ogg" download="v_1787100170011.ogg">⬇ 1141 KB</a> &nbsp; 
<small>ogg: 1141 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100170011-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100170011-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100170011-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/market_browsing_03/v_1787100170011.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/market_browsing_03/v_1787100170011.ogg`
- approve:   `bun run game:audio:approve music.workspace.market_browsing_03 --variant v_1787100170011`
- deprecate: `bun run game:audio:deprecate music.workspace.market_browsing_03 --variant v_1787100170011`
- re-stage:  `bun run game:audio:stage music.workspace.market_browsing_03`

Notes:

```

---

### `music.workspace.stadium_management_01`

> Estadio — variante 1: ostinato pizzicato.

#### `v_1787100331920`

<audio id="aud-v_1787100331920" controls preload="none" src="../../.generate/audios/staging/music.workspace/stadium_management_01/v_1787100331920.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100331920" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100331920" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/stadium_management_01/v_1787100331920.ogg" download="v_1787100331920.ogg">⬇ 965 KB</a> &nbsp; 
<small>ogg: 965 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100331920-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100331920-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100331920-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/stadium_management_01/v_1787100331920.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/stadium_management_01/v_1787100331920.ogg`
- approve:   `bun run game:audio:approve music.workspace.stadium_management_01 --variant v_1787100331920`
- deprecate: `bun run game:audio:deprecate music.workspace.stadium_management_01 --variant v_1787100331920`
- re-stage:  `bun run game:audio:stage music.workspace.stadium_management_01`

Notes:

```

---

### `music.workspace.stadium_management_02`

> Estadio — variante 2: acordes cálidos.

#### `v_1787100331923`

<audio id="aud-v_1787100331923" controls preload="none" src="../../.generate/audios/staging/music.workspace/stadium_management_02/v_1787100331923.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100331923" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100331923" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/stadium_management_02/v_1787100331923.ogg" download="v_1787100331923.ogg">⬇ 683 KB</a> &nbsp; 
<small>ogg: 683 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100331923-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100331923-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100331923-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/stadium_management_02/v_1787100331923.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/stadium_management_02/v_1787100331923.ogg`
- approve:   `bun run game:audio:approve music.workspace.stadium_management_02 --variant v_1787100331923`
- deprecate: `bun run game:audio:deprecate music.workspace.stadium_management_02 --variant v_1787100331923`
- re-stage:  `bun run game:audio:stage music.workspace.stadium_management_02`

Notes:

```

---

### `music.workspace.stadium_management_03`

> Estadio — variante 3: melodía legato.

#### `v_1787100331922`

<audio id="aud-v_1787100331922" controls preload="none" src="../../.generate/audios/staging/music.workspace/stadium_management_03/v_1787100331922.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100331922" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100331922" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/stadium_management_03/v_1787100331922.ogg" download="v_1787100331922.ogg">⬇ 1065 KB</a> &nbsp; 
<small>ogg: 1065 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100331922-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100331922-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100331922-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/stadium_management_03/v_1787100331922.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/stadium_management_03/v_1787100331922.ogg`
- approve:   `bun run game:audio:approve music.workspace.stadium_management_03 --variant v_1787100331922`
- deprecate: `bun run game:audio:deprecate music.workspace.stadium_management_03 --variant v_1787100331922`
- re-stage:  `bun run game:audio:stage music.workspace.stadium_management_03`

Notes:

```

---

### `music.workspace.standings_review_01`

> Clasificación — variante 1: pad + Rhodes espaciado.

#### `v_1787100488826`

<audio id="aud-v_1787100488826" controls preload="none" src="../../.generate/audios/staging/music.workspace/standings_review_01/v_1787100488826.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100488826" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100488826" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/standings_review_01/v_1787100488826.ogg" download="v_1787100488826.ogg">⬇ 1119 KB</a> &nbsp; 
<small>ogg: 1119 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100488826-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100488826-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100488826-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/standings_review_01/v_1787100488826.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/standings_review_01/v_1787100488826.ogg`
- approve:   `bun run game:audio:approve music.workspace.standings_review_01 --variant v_1787100488826`
- deprecate: `bun run game:audio:deprecate music.workspace.standings_review_01 --variant v_1787100488826`
- re-stage:  `bun run game:audio:stage music.workspace.standings_review_01`

Notes:

```

---

### `music.workspace.standings_review_02`

> Clasificación — variante 2: línea única Rhodes.

#### `v_1787100488827`

<audio id="aud-v_1787100488827" controls preload="none" src="../../.generate/audios/staging/music.workspace/standings_review_02/v_1787100488827.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100488827" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100488827" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/standings_review_02/v_1787100488827.ogg" download="v_1787100488827.ogg">⬇ 1069 KB</a> &nbsp; 
<small>ogg: 1069 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100488827-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100488827-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100488827-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/standings_review_02/v_1787100488827.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/standings_review_02/v_1787100488827.ogg`
- approve:   `bun run game:audio:approve music.workspace.standings_review_02 --variant v_1787100488827`
- deprecate: `bun run game:audio:deprecate music.workspace.standings_review_02 --variant v_1787100488827`
- re-stage:  `bun run game:audio:stage music.workspace.standings_review_02`

Notes:

```

---

### `music.workspace.standings_review_03`

> Clasificación — variante 3: pad–Rhodes alternando.

#### `v_1787100488828`

<audio id="aud-v_1787100488828" controls preload="none" src="../../.generate/audios/staging/music.workspace/standings_review_03/v_1787100488828.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100488828" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100488828" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.workspace/standings_review_03/v_1787100488828.ogg" download="v_1787100488828.ogg">⬇ 863 KB</a> &nbsp; 
<small>ogg: 863 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100488828-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100488828-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100488828-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.workspace/standings_review_03/v_1787100488828.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.workspace/standings_review_03/v_1787100488828.ogg`
- approve:   `bun run game:audio:approve music.workspace.standings_review_03 --variant v_1787100488828`
- deprecate: `bun run game:audio:deprecate music.workspace.standings_review_03 --variant v_1787100488828`
- re-stage:  `bun run game:audio:stage music.workspace.standings_review_03`

Notes:

```

---

### `music.editor` — ✅ all staged

> Pack editor / creative mode. Sparse and minimal so the player can think — no busy motifs, lots of air. Same warm 1990s synthwave palette as the menu group.

### `music.editor.editor_focus_01`

> Editor — variante 1: pad + nota Rhodes.

#### `v_1787100650393`

<audio id="aud-v_1787100650393" controls preload="none" src="../../.generate/audios/staging/music.editor/editor_focus_01/v_1787100650393.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100650393" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100650393" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.editor/editor_focus_01/v_1787100650393.ogg" download="v_1787100650393.ogg">⬇ 647 KB</a> &nbsp; 
<small>ogg: 647 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100650393-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100650393-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100650393-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.editor/editor_focus_01/v_1787100650393.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.editor/editor_focus_01/v_1787100650393.ogg`
- approve:   `bun run game:audio:approve music.editor.editor_focus_01 --variant v_1787100650393`
- deprecate: `bun run game:audio:deprecate music.editor.editor_focus_01 --variant v_1787100650393`
- re-stage:  `bun run game:audio:stage music.editor.editor_focus_01`

Notes:

```

---

### `music.editor.editor_focus_02`

> Editor — variante 2: arpegio cada 8 compases.

#### `v_1787100650393`

<audio id="aud-v_1787100650393" controls preload="none" src="../../.generate/audios/staging/music.editor/editor_focus_02/v_1787100650393.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100650393" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100650393" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.editor/editor_focus_02/v_1787100650393.ogg" download="v_1787100650393.ogg">⬇ 1575 KB</a> &nbsp; 
<small>ogg: 1575 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100650393-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100650393-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100650393-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.editor/editor_focus_02/v_1787100650393.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.editor/editor_focus_02/v_1787100650393.ogg`
- approve:   `bun run game:audio:approve music.editor.editor_focus_02 --variant v_1787100650393`
- deprecate: `bun run game:audio:deprecate music.editor.editor_focus_02 --variant v_1787100650393`
- re-stage:  `bun run game:audio:stage music.editor.editor_focus_02`

Notes:

```

---

### `music.editor.editor_focus_03`

> Editor — variante 3: acordes cada 4 compases.

#### `v_1787100650394`

<audio id="aud-v_1787100650394" controls preload="none" src="../../.generate/audios/staging/music.editor/editor_focus_03/v_1787100650394.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100650394" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100650394" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.editor/editor_focus_03/v_1787100650394.ogg" download="v_1787100650394.ogg">⬇ 1099 KB</a> &nbsp; 
<small>ogg: 1099 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100650394-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100650394-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100650394-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.editor/editor_focus_03/v_1787100650394.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.editor/editor_focus_03/v_1787100650394.ogg`
- approve:   `bun run game:audio:approve music.editor.editor_focus_03 --variant v_1787100650394`
- deprecate: `bun run game:audio:deprecate music.editor.editor_focus_03 --variant v_1787100650394`
- re-stage:  `bun run game:audio:stage music.editor.editor_focus_03`

Notes:

```

---

### `music.trophy` — ✅ all staged

> Trophy / winners screen (league titles, promotions, manager of the month). Same warm 1990s synthwave palette but with a contained brass-like synth lead so it feels triumphant without being bombastic.

### `music.trophy.trophy_lift_01`

> Trofeo — variante 1: brass sobre pizzicato.

#### `v_1787100904822`

<audio id="aud-v_1787100904822" controls preload="none" src="../../.generate/audios/staging/music.trophy/trophy_lift_01/v_1787100904822.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100904822" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100904822" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.trophy/trophy_lift_01/v_1787100904822.ogg" download="v_1787100904822.ogg">⬇ 913 KB</a> &nbsp; 
<small>ogg: 913 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100904822-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100904822-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100904822-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.trophy/trophy_lift_01/v_1787100904822.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.trophy/trophy_lift_01/v_1787100904822.ogg`
- approve:   `bun run game:audio:approve music.trophy.trophy_lift_01 --variant v_1787100904822`
- deprecate: `bun run game:audio:deprecate music.trophy.trophy_lift_01 --variant v_1787100904822`
- re-stage:  `bun run game:audio:stage music.trophy.trophy_lift_01`

Notes:

```

---

#### `v_1787100854985`

<audio id="aud-v_1787100854985" controls preload="none" src="../../.generate/audios/staging/music.trophy/trophy_lift_01/v_1787100854985.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100854985" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100854985" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.trophy/trophy_lift_01/v_1787100854985.ogg" download="v_1787100854985.ogg">⬇ 514 KB</a> &nbsp; 
<small>ogg: 514 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100854985-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100854985-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100854985-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.trophy/trophy_lift_01/v_1787100854985.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.trophy/trophy_lift_01/v_1787100854985.ogg`
- approve:   `bun run game:audio:approve music.trophy.trophy_lift_01 --variant v_1787100854985`
- deprecate: `bun run game:audio:deprecate music.trophy.trophy_lift_01 --variant v_1787100854985`
- re-stage:  `bun run game:audio:stage music.trophy.trophy_lift_01`

Notes:

```

---

### `music.trophy.trophy_lift_02`

> Trofeo — variante 2: Rhodes melodía.

#### `v_1787100854985`

<audio id="aud-v_1787100854985" controls preload="none" src="../../.generate/audios/staging/music.trophy/trophy_lift_02/v_1787100854985.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100854985" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100854985" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.trophy/trophy_lift_02/v_1787100854985.ogg" download="v_1787100854985.ogg">⬇ 779 KB</a> &nbsp; 
<small>ogg: 779 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100854985-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100854985-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100854985-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.trophy/trophy_lift_02/v_1787100854985.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.trophy/trophy_lift_02/v_1787100854985.ogg`
- approve:   `bun run game:audio:approve music.trophy.trophy_lift_02 --variant v_1787100854985`
- deprecate: `bun run game:audio:deprecate music.trophy.trophy_lift_02 --variant v_1787100854985`
- re-stage:  `bun run game:audio:stage music.trophy.trophy_lift_02`

Notes:

```

---

### `music.trophy.trophy_lift_03`

> Trofeo — variante 3: brass–Rhodes pregunta-respuesta.

#### `v_1787100904821`

<audio id="aud-v_1787100904821" controls preload="none" src="../../.generate/audios/staging/music.trophy/trophy_lift_03/v_1787100904821.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100904821" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100904821" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.trophy/trophy_lift_03/v_1787100904821.ogg" download="v_1787100904821.ogg">⬇ 1485 KB</a> &nbsp; 
<small>ogg: 1485 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100904821-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100904821-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100904821-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.trophy/trophy_lift_03/v_1787100904821.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.trophy/trophy_lift_03/v_1787100904821.ogg`
- approve:   `bun run game:audio:approve music.trophy.trophy_lift_03 --variant v_1787100904821`
- deprecate: `bun run game:audio:deprecate music.trophy.trophy_lift_03 --variant v_1787100904821`
- re-stage:  `bun run game:audio:stage music.trophy.trophy_lift_03`

Notes:

```

---

#### `v_1787100854984`

<audio id="aud-v_1787100854984" controls preload="none" src="../../.generate/audios/staging/music.trophy/trophy_lift_03/v_1787100854984.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787100854984" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787100854984" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/music.trophy/trophy_lift_03/v_1787100854984.ogg" download="v_1787100854984.ogg">⬇ 1039 KB</a> &nbsp; 
<small>ogg: 1039 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787100854984-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787100854984-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787100854984-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/music.trophy/trophy_lift_03/v_1787100854984.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/music.trophy/trophy_lift_03/v_1787100854984.ogg`
- approve:   `bun run game:audio:approve music.trophy.trophy_lift_03 --variant v_1787100854984`
- deprecate: `bun run game:audio:deprecate music.trophy.trophy_lift_03 --variant v_1787100854984`
- re-stage:  `bun run game:audio:stage music.trophy.trophy_lift_03`

Notes:

```

---

## 🔊 SFX

### `sfx.ui` — ✅ all staged

> Retro 1990s PC speaker UI sound. Short, crisp, synthesized, dry. Should sound like they all came from the same sound chip.

### `sfx.ui.ui_click`

> Click UI.

#### `v_1787096672270`

<audio id="aud-v_1787096672270" controls preload="none" src="../../.generate/audios/staging/sfx.ui/ui_click/v_1787096672270.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096672270" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096672270" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.ui/ui_click/v_1787096672270.ogg" download="v_1787096672270.ogg">⬇ 515 KB</a> &nbsp; 
<small>ogg: 515 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096672270-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096672270-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096672270-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.ui/ui_click/v_1787096672270.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_click/v_1787096672270.ogg`
- approve:   `bun run game:audio:approve sfx.ui.ui_click --variant v_1787096672270`
- deprecate: `bun run game:audio:deprecate sfx.ui.ui_click --variant v_1787096672270`
- re-stage:  `bun run game:audio:stage sfx.ui.ui_click`

Notes:

```

---

### `sfx.ui.ui_hover`

> Hover UI.

#### `v_1787096672275`

<audio id="aud-v_1787096672275" controls preload="none" src="../../.generate/audios/staging/sfx.ui/ui_hover/v_1787096672275.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096672275" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096672275" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.ui/ui_hover/v_1787096672275.ogg" download="v_1787096672275.ogg">⬇ 84 KB</a> &nbsp; 
<small>ogg: 84 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096672275-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096672275-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096672275-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.ui/ui_hover/v_1787096672275.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_hover/v_1787096672275.ogg`
- approve:   `bun run game:audio:approve sfx.ui.ui_hover --variant v_1787096672275`
- deprecate: `bun run game:audio:deprecate sfx.ui.ui_hover --variant v_1787096672275`
- re-stage:  `bun run game:audio:stage sfx.ui.ui_hover`

Notes:

```

---

### `sfx.ui.ui_confirm`

> Confirmación UI.

#### `v_1787096672274`

<audio id="aud-v_1787096672274" controls preload="none" src="../../.generate/audios/staging/sfx.ui/ui_confirm/v_1787096672274.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096672274" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096672274" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.ui/ui_confirm/v_1787096672274.ogg" download="v_1787096672274.ogg">⬇ 673 KB</a> &nbsp; 
<small>ogg: 673 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096672274-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096672274-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096672274-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.ui/ui_confirm/v_1787096672274.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_confirm/v_1787096672274.ogg`
- approve:   `bun run game:audio:approve sfx.ui.ui_confirm --variant v_1787096672274`
- deprecate: `bun run game:audio:deprecate sfx.ui.ui_confirm --variant v_1787096672274`
- re-stage:  `bun run game:audio:stage sfx.ui.ui_confirm`

Notes:

```

---

### `sfx.ui.ui_cancel`

> Cancelar UI.

#### `v_1787097583086`

<audio id="aud-v_1787097583086" controls preload="none" src="../../.generate/audios/staging/sfx.ui/ui_cancel/v_1787097583086.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097583086" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097583086" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.ui/ui_cancel/v_1787097583086.ogg" download="v_1787097583086.ogg">⬇ 524 KB</a> &nbsp; 
<small>ogg: 524 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097583086-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097583086-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097583086-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.ui/ui_cancel/v_1787097583086.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_cancel/v_1787097583086.ogg`
- approve:   `bun run game:audio:approve sfx.ui.ui_cancel --variant v_1787097583086`
- deprecate: `bun run game:audio:deprecate sfx.ui.ui_cancel --variant v_1787097583086`
- re-stage:  `bun run game:audio:stage sfx.ui.ui_cancel`

Notes:

```

---

### `sfx.ui.ui_error`

> Error UI.

#### `v_1787096871691`

<audio id="aud-v_1787096871691" controls preload="none" src="../../.generate/audios/staging/sfx.ui/ui_error/v_1787096871691.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096871691" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096871691" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.ui/ui_error/v_1787096871691.ogg" download="v_1787096871691.ogg">⬇ 537 KB</a> &nbsp; 
<small>ogg: 537 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096871691-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096871691-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096871691-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.ui/ui_error/v_1787096871691.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_error/v_1787096871691.ogg`
- approve:   `bun run game:audio:approve sfx.ui.ui_error --variant v_1787096871691`
- deprecate: `bun run game:audio:deprecate sfx.ui.ui_error --variant v_1787096871691`
- re-stage:  `bun run game:audio:stage sfx.ui.ui_error`

Notes:

```

---

### `sfx.ui.ui_notify`

> Notificación UI.

#### `v_1787096871687`

<audio id="aud-v_1787096871687" controls preload="none" src="../../.generate/audios/staging/sfx.ui/ui_notify/v_1787096871687.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096871687" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096871687" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.ui/ui_notify/v_1787096871687.ogg" download="v_1787096871687.ogg">⬇ 951 KB</a> &nbsp; 
<small>ogg: 951 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096871687-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096871687-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096871687-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.ui/ui_notify/v_1787096871687.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_notify/v_1787096871687.ogg`
- approve:   `bun run game:audio:approve sfx.ui.ui_notify --variant v_1787096871687`
- deprecate: `bun run game:audio:deprecate sfx.ui.ui_notify --variant v_1787096871687`
- re-stage:  `bun run game:audio:stage sfx.ui.ui_notify`

Notes:

```

---

### `sfx.match` — ✅ all staged

> Retro 1990s football management PC game stadium sound effects. Should sound lo-fi but unmistakable — they are what the manager hears from the stands.

### `sfx.match.whistle_short`

> Silbido corto.

#### `v_1787096871692`

<audio id="aud-v_1787096871692" controls preload="none" src="../../.generate/audios/staging/sfx.match/whistle_short/v_1787096871692.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096871692" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096871692" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/whistle_short/v_1787096871692.ogg" download="v_1787096871692.ogg">⬇ 484 KB</a> &nbsp; 
<small>ogg: 484 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096871692-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096871692-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096871692-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/whistle_short/v_1787096871692.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/whistle_short/v_1787096871692.ogg`
- approve:   `bun run game:audio:approve sfx.match.whistle_short --variant v_1787096871692`
- deprecate: `bun run game:audio:deprecate sfx.match.whistle_short --variant v_1787096871692`
- re-stage:  `bun run game:audio:stage sfx.match.whistle_short`

Notes:

```

---

### `sfx.match.whistle_long`

> Silbido largo.

#### `v_1787096871695`

<audio id="aud-v_1787096871695" controls preload="none" src="../../.generate/audios/staging/sfx.match/whistle_long/v_1787096871695.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096871695" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096871695" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/whistle_long/v_1787096871695.ogg" download="v_1787096871695.ogg">⬇ 222 KB</a> &nbsp; 
<small>ogg: 222 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096871695-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096871695-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096871695-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/whistle_long/v_1787096871695.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/whistle_long/v_1787096871695.ogg`
- approve:   `bun run game:audio:approve sfx.match.whistle_long --variant v_1787096871695`
- deprecate: `bun run game:audio:deprecate sfx.match.whistle_long --variant v_1787096871695`
- re-stage:  `bun run game:audio:stage sfx.match.whistle_long`

Notes:

```

---

### `sfx.match.whistle_penalty`

> Silbido de penalti.

#### `v_1787096871691`

<audio id="aud-v_1787096871691" controls preload="none" src="../../.generate/audios/staging/sfx.match/whistle_penalty/v_1787096871691.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787096871691" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787096871691" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/whistle_penalty/v_1787096871691.ogg" download="v_1787096871691.ogg">⬇ 66 KB</a> &nbsp; 
<small>ogg: 66 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787096871691-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787096871691-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787096871691-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/whistle_penalty/v_1787096871691.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/whistle_penalty/v_1787096871691.ogg`
- approve:   `bun run game:audio:approve sfx.match.whistle_penalty --variant v_1787096871691`
- deprecate: `bun run game:audio:deprecate sfx.match.whistle_penalty --variant v_1787096871691`
- re-stage:  `bun run game:audio:stage sfx.match.whistle_penalty`

Notes:

```

---

### `sfx.match.crowd_cheer`

> Hinchada. Rugido.

#### `v_1787097075010`

<audio id="aud-v_1787097075010" controls preload="none" src="../../.generate/audios/staging/sfx.match/crowd_cheer/v_1787097075010.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097075010" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097075010" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/crowd_cheer/v_1787097075010.ogg" download="v_1787097075010.ogg">⬇ 372 KB</a> &nbsp; 
<small>ogg: 372 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097075010-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097075010-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097075010-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/crowd_cheer/v_1787097075010.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/crowd_cheer/v_1787097075010.ogg`
- approve:   `bun run game:audio:approve sfx.match.crowd_cheer --variant v_1787097075010`
- deprecate: `bun run game:audio:deprecate sfx.match.crowd_cheer --variant v_1787097075010`
- re-stage:  `bun run game:audio:stage sfx.match.crowd_cheer`

Notes:

```

---

### `sfx.match.crowd_groan`

> Hinchada. Ohhh.

#### `v_1787097075017`

<audio id="aud-v_1787097075017" controls preload="none" src="../../.generate/audios/staging/sfx.match/crowd_groan/v_1787097075017.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097075017" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097075017" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/crowd_groan/v_1787097075017.ogg" download="v_1787097075017.ogg">⬇ 891 KB</a> &nbsp; 
<small>ogg: 891 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097075017-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097075017-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097075017-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/crowd_groan/v_1787097075017.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/crowd_groan/v_1787097075017.ogg`
- approve:   `bun run game:audio:approve sfx.match.crowd_groan --variant v_1787097075017`
- deprecate: `bun run game:audio:deprecate sfx.match.crowd_groan --variant v_1787097075017`
- re-stage:  `bun run game:audio:stage sfx.match.crowd_groan`

Notes:

```

---

### `sfx.match.crowd_chant`

> Hinchada. Cántico.

#### `v_1787097075011`

<audio id="aud-v_1787097075011" controls preload="none" src="../../.generate/audios/staging/sfx.match/crowd_chant/v_1787097075011.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097075011" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097075011" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/crowd_chant/v_1787097075011.ogg" download="v_1787097075011.ogg">⬇ 774 KB</a> &nbsp; 
<small>ogg: 774 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097075011-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097075011-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097075011-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/crowd_chant/v_1787097075011.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/crowd_chant/v_1787097075011.ogg`
- approve:   `bun run game:audio:approve sfx.match.crowd_chant --variant v_1787097075011`
- deprecate: `bun run game:audio:deprecate sfx.match.crowd_chant --variant v_1787097075011`
- re-stage:  `bun run game:audio:stage sfx.match.crowd_chant`

Notes:

```

---

### `sfx.match.kick`

> Tiro. Thud + squeak.

#### `v_1787097075023`

<audio id="aud-v_1787097075023" controls preload="none" src="../../.generate/audios/staging/sfx.match/kick/v_1787097075023.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097075023" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097075023" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/kick/v_1787097075023.ogg" download="v_1787097075023.ogg">⬇ 198 KB</a> &nbsp; 
<small>ogg: 198 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097075023-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097075023-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097075023-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/kick/v_1787097075023.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/kick/v_1787097075023.ogg`
- approve:   `bun run game:audio:approve sfx.match.kick --variant v_1787097075023`
- deprecate: `bun run game:audio:deprecate sfx.match.kick --variant v_1787097075023`
- re-stage:  `bun run game:audio:stage sfx.match.kick`

Notes:

```

---

### `sfx.match.ball_bounce`

> Balón botando.

#### `v_1787097075012`

<audio id="aud-v_1787097075012" controls preload="none" src="../../.generate/audios/staging/sfx.match/ball_bounce/v_1787097075012.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097075012" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097075012" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/ball_bounce/v_1787097075012.ogg" download="v_1787097075012.ogg">⬇ 883 KB</a> &nbsp; 
<small>ogg: 883 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097075012-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097075012-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097075012-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/ball_bounce/v_1787097075012.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/ball_bounce/v_1787097075012.ogg`
- approve:   `bun run game:audio:approve sfx.match.ball_bounce --variant v_1787097075012`
- deprecate: `bun run game:audio:deprecate sfx.match.ball_bounce --variant v_1787097075012`
- re-stage:  `bun run game:audio:stage sfx.match.ball_bounce`

Notes:

```

---

### `sfx.match.post_hit`

> Poste. Clang.

#### `v_1787097075013`

<audio id="aud-v_1787097075013" controls preload="none" src="../../.generate/audios/staging/sfx.match/post_hit/v_1787097075013.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787097075013" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787097075013" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match/post_hit/v_1787097075013.ogg" download="v_1787097075013.ogg">⬇ 495 KB</a> &nbsp; 
<small>ogg: 495 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787097075013-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787097075013-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787097075013-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match/post_hit/v_1787097075013.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match/post_hit/v_1787097075013.ogg`
- approve:   `bun run game:audio:approve sfx.match.post_hit --variant v_1787097075013`
- deprecate: `bun run game:audio:deprecate sfx.match.post_hit --variant v_1787097075013`
- re-stage:  `bun run game:audio:stage sfx.match.post_hit`

Notes:

```

---

### `sfx.workspace` — ✅ all staged

> Section navigation in the workspace sidebar (Overview / Squad / Tactics / Competitions / Market / Finances / World). Should feel cohesive with sfx.ui but slightly more deliberate — these are larger transitions.

### `sfx.workspace.section_change`

> Sidebar — cambio de sección. Whoosh corto.

#### `v_1787094058314`

<audio id="aud-v_1787094058314" controls preload="none" src="../../.generate/audios/staging/sfx.workspace/section_change/v_1787094058314.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094058314" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094058314" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.workspace/section_change/v_1787094058314.ogg" download="v_1787094058314.ogg">⬇ 496 KB</a> &nbsp; 
<small>ogg: 496 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094058314-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094058314-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094058314-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.workspace/section_change/v_1787094058314.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.workspace/section_change/v_1787094058314.ogg`
- approve:   `bun run game:audio:approve sfx.workspace.section_change --variant v_1787094058314`
- deprecate: `bun run game:audio:deprecate sfx.workspace.section_change --variant v_1787094058314`
- re-stage:  `bun run game:audio:stage sfx.workspace.section_change`

Notes:

```

---

#### `v_1787094127955`

<audio id="aud-v_1787094127955" controls preload="none" src="../../.generate/audios/staging/sfx.workspace/section_change/v_1787094127955.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094127955" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094127955" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.workspace/section_change/v_1787094127955.ogg" download="v_1787094127955.ogg">⬇ 471 KB</a> &nbsp; 
<small>ogg: 471 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094127955-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094127955-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094127955-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.workspace/section_change/v_1787094127955.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.workspace/section_change/v_1787094127955.ogg`
- approve:   `bun run game:audio:approve sfx.workspace.section_change --variant v_1787094127955`
- deprecate: `bun run game:audio:deprecate sfx.workspace.section_change --variant v_1787094127955`
- re-stage:  `bun run game:audio:stage sfx.workspace.section_change`

Notes:

```

---

### `sfx.workspace.section_hover`

> Sidebar — hover. Tick subliminal.

#### `v_1787094226566`

<audio id="aud-v_1787094226566" controls preload="none" src="../../.generate/audios/staging/sfx.workspace/section_hover/v_1787094226566.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094226566" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094226566" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.workspace/section_hover/v_1787094226566.ogg" download="v_1787094226566.ogg">⬇ 616 KB</a> &nbsp; 
<small>ogg: 616 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094226566-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094226566-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094226566-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.workspace/section_hover/v_1787094226566.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.workspace/section_hover/v_1787094226566.ogg`
- approve:   `bun run game:audio:approve sfx.workspace.section_hover --variant v_1787094226566`
- deprecate: `bun run game:audio:deprecate sfx.workspace.section_hover --variant v_1787094226566`
- re-stage:  `bun run game:audio:stage sfx.workspace.section_hover`

Notes:

```

---

### `sfx.workspace.section_pin`

> Sidebar — pineado. Ping de dos notas.

#### `v_1787094226562`

<audio id="aud-v_1787094226562" controls preload="none" src="../../.generate/audios/staging/sfx.workspace/section_pin/v_1787094226562.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094226562" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094226562" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.workspace/section_pin/v_1787094226562.ogg" download="v_1787094226562.ogg">⬇ 114 KB</a> &nbsp; 
<small>ogg: 114 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094226562-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094226562-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094226562-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.workspace/section_pin/v_1787094226562.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.workspace/section_pin/v_1787094226562.ogg`
- approve:   `bun run game:audio:approve sfx.workspace.section_pin --variant v_1787094226562`
- deprecate: `bun run game:audio:deprecate sfx.workspace.section_pin --variant v_1787094226562`
- re-stage:  `bun run game:audio:stage sfx.workspace.section_pin`

Notes:

```

---

### `sfx.editor` — ✅ all staged

> Pack editor — placing, rotating and removing tiles / cards / stickers on the canvas. Crispy, satisfying, retro-gamey.

### `sfx.editor.tile_place`

> Editor — tile colocado. Snap crujiente.

#### `v_1787094226565`

<audio id="aud-v_1787094226565" controls preload="none" src="../../.generate/audios/staging/sfx.editor/tile_place/v_1787094226565.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094226565" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094226565" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.editor/tile_place/v_1787094226565.ogg" download="v_1787094226565.ogg">⬇ 753 KB</a> &nbsp; 
<small>ogg: 753 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094226565-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094226565-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094226565-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.editor/tile_place/v_1787094226565.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.editor/tile_place/v_1787094226565.ogg`
- approve:   `bun run game:audio:approve sfx.editor.tile_place --variant v_1787094226565`
- deprecate: `bun run game:audio:deprecate sfx.editor.tile_place --variant v_1787094226565`
- re-stage:  `bun run game:audio:stage sfx.editor.tile_place`

Notes:

```

---

### `sfx.editor.tile_remove`

> Editor — tile retirado. Click-pop.

#### `v_1787094386565`

<audio id="aud-v_1787094386565" controls preload="none" src="../../.generate/audios/staging/sfx.editor/tile_remove/v_1787094386565.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094386565" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094386565" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.editor/tile_remove/v_1787094386565.ogg" download="v_1787094386565.ogg">⬇ 639 KB</a> &nbsp; 
<small>ogg: 639 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094386565-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094386565-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094386565-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.editor/tile_remove/v_1787094386565.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.editor/tile_remove/v_1787094386565.ogg`
- approve:   `bun run game:audio:approve sfx.editor.tile_remove --variant v_1787094386565`
- deprecate: `bun run game:audio:deprecate sfx.editor.tile_remove --variant v_1787094386565`
- re-stage:  `bun run game:audio:stage sfx.editor.tile_remove`

Notes:

```

---

### `sfx.editor.tile_rotate`

> Editor — tile rotado. Tick de madera.

#### `v_1787094386564`

<audio id="aud-v_1787094386564" controls preload="none" src="../../.generate/audios/staging/sfx.editor/tile_rotate/v_1787094386564.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094386564" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094386564" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.editor/tile_rotate/v_1787094386564.ogg" download="v_1787094386564.ogg">⬇ 595 KB</a> &nbsp; 
<small>ogg: 595 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094386564-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094386564-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094386564-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.editor/tile_rotate/v_1787094386564.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.editor/tile_rotate/v_1787094386564.ogg`
- approve:   `bun run game:audio:approve sfx.editor.tile_rotate --variant v_1787094386564`
- deprecate: `bun run game:audio:deprecate sfx.editor.tile_rotate --variant v_1787094386564`
- re-stage:  `bun run game:audio:stage sfx.editor.tile_rotate`

Notes:

```

---

### `sfx.editor.canvas_save`

> Editor — canvas guardado.

#### `v_1787094386566`

<audio id="aud-v_1787094386566" controls preload="none" src="../../.generate/audios/staging/sfx.editor/canvas_save/v_1787094386566.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094386566" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094386566" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.editor/canvas_save/v_1787094386566.ogg" download="v_1787094386566.ogg">⬇ 636 KB</a> &nbsp; 
<small>ogg: 636 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094386566-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094386566-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094386566-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.editor/canvas_save/v_1787094386566.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.editor/canvas_save/v_1787094386566.ogg`
- approve:   `bun run game:audio:approve sfx.editor.canvas_save --variant v_1787094386566`
- deprecate: `bun run game:audio:deprecate sfx.editor.canvas_save --variant v_1787094386566`
- re-stage:  `bun run game:audio:stage sfx.editor.canvas_save`

Notes:

```

---

### `sfx.match_extended` — ✅ all staged

> Extended match commentary stingers: cards, fouls, shots, saves, substitutions. Each event needs a short, identifiable sting that the manager associates with that exact play.

### `sfx.match_extended.match_start`

> Inicio del partido. Silbido + hinchada.

#### `v_1787094513248`

<audio id="aud-v_1787094513248" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/match_start/v_1787094513248.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094513248" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094513248" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/match_start/v_1787094513248.ogg" download="v_1787094513248.ogg">⬇ 914 KB</a> &nbsp; 
<small>ogg: 914 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094513248-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094513248-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094513248-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/match_start/v_1787094513248.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/match_start/v_1787094513248.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.match_start --variant v_1787094513248`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.match_start --variant v_1787094513248`
- re-stage:  `bun run game:audio:stage sfx.match_extended.match_start`

Notes:

```

---

### `sfx.match_extended.half_time`

> Descanso. Dos silbidos.

#### `v_1787094513233`

<audio id="aud-v_1787094513233" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/half_time/v_1787094513233.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094513233" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094513233" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/half_time/v_1787094513233.ogg" download="v_1787094513233.ogg">⬇ 256 KB</a> &nbsp; 
<small>ogg: 256 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094513233-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094513233-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094513233-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/half_time/v_1787094513233.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/half_time/v_1787094513233.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.half_time --variant v_1787094513233`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.half_time --variant v_1787094513233`
- re-stage:  `bun run game:audio:stage sfx.match_extended.half_time`

Notes:

```

---

### `sfx.match_extended.yellow_card`

> Tarjeta amarilla. Silbido + flick.

#### `v_1787094513252`

<audio id="aud-v_1787094513252" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/yellow_card/v_1787094513252.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094513252" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094513252" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/yellow_card/v_1787094513252.ogg" download="v_1787094513252.ogg">⬇ 279 KB</a> &nbsp; 
<small>ogg: 279 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094513252-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094513252-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094513252-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/yellow_card/v_1787094513252.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/yellow_card/v_1787094513252.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.yellow_card --variant v_1787094513252`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.yellow_card --variant v_1787094513252`
- re-stage:  `bun run game:audio:stage sfx.match_extended.yellow_card`

Notes:

```

---

### `sfx.match_extended.red_card`

> Tarjeta roja. Silbido + ominosidad.

#### `v_1787094685518`

<audio id="aud-v_1787094685518" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/red_card/v_1787094685518.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094685518" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094685518" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/red_card/v_1787094685518.ogg" download="v_1787094685518.ogg">⬇ 219 KB</a> &nbsp; 
<small>ogg: 219 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094685518-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094685518-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094685518-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/red_card/v_1787094685518.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/red_card/v_1787094685518.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.red_card --variant v_1787094685518`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.red_card --variant v_1787094685518`
- re-stage:  `bun run game:audio:stage sfx.match_extended.red_card`

Notes:

```

---

### `sfx.match_extended.foul`

> Falta. Silbido + ohh.

#### `v_1787094685521`

<audio id="aud-v_1787094685521" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/foul/v_1787094685521.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094685521" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094685521" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/foul/v_1787094685521.ogg" download="v_1787094685521.ogg">⬇ 677 KB</a> &nbsp; 
<small>ogg: 677 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094685521-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094685521-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094685521-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/foul/v_1787094685521.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/foul/v_1787094685521.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.foul --variant v_1787094685521`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.foul --variant v_1787094685521`
- re-stage:  `bun run game:audio:stage sfx.match_extended.foul`

Notes:

```

---

### `sfx.match_extended.shot_on_target`

> Tiro a puerta. Golpe + parada.

#### `v_1787094685529`

<audio id="aud-v_1787094685529" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/shot_on_target/v_1787094685529.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094685529" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094685529" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/shot_on_target/v_1787094685529.ogg" download="v_1787094685529.ogg">⬇ 616 KB</a> &nbsp; 
<small>ogg: 616 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094685529-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094685529-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094685529-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/shot_on_target/v_1787094685529.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/shot_on_target/v_1787094685529.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.shot_on_target --variant v_1787094685529`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.shot_on_target --variant v_1787094685529`
- re-stage:  `bun run game:audio:stage sfx.match_extended.shot_on_target`

Notes:

```

---

### `sfx.match_extended.shot_off_target`

> Tiro fuera. Golpe + thud.

#### `v_1787094819663`

<audio id="aud-v_1787094819663" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/shot_off_target/v_1787094819663.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094819663" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094819663" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/shot_off_target/v_1787094819663.ogg" download="v_1787094819663.ogg">⬇ 640 KB</a> &nbsp; 
<small>ogg: 640 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094819663-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094819663-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094819663-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/shot_off_target/v_1787094819663.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/shot_off_target/v_1787094819663.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.shot_off_target --variant v_1787094819663`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.shot_off_target --variant v_1787094819663`
- re-stage:  `bun run game:audio:stage sfx.match_extended.shot_off_target`

Notes:

```

---

### `sfx.match_extended.substitution`

> Cambio. Ping + murmullo.

#### `v_1787094819661`

<audio id="aud-v_1787094819661" controls preload="none" src="../../.generate/audios/staging/sfx.match_extended/substitution/v_1787094819661.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094819661" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094819661" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.match_extended/substitution/v_1787094819661.ogg" download="v_1787094819661.ogg">⬇ 714 KB</a> &nbsp; 
<small>ogg: 714 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094819661-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094819661-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094819661-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.match_extended/substitution/v_1787094819661.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.match_extended/substitution/v_1787094819661.ogg`
- approve:   `bun run game:audio:approve sfx.match_extended.substitution --variant v_1787094819661`
- deprecate: `bun run game:audio:deprecate sfx.match_extended.substitution --variant v_1787094819661`
- re-stage:  `bun run game:audio:stage sfx.match_extended.substitution`

Notes:

```

---

### `sfx.transfer` — ✅ all staged

> Transfer market stingers — incoming offer, scout report, completed deal. Lo-fi retro chime-ish to fit the 1990s chip aesthetic but with a cash-register undertone for deals.

### `sfx.transfer.offer_received`

> Oferta recibida. Chime entrante.

#### `v_1787094819660`

<audio id="aud-v_1787094819660" controls preload="none" src="../../.generate/audios/staging/sfx.transfer/offer_received/v_1787094819660.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094819660" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094819660" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.transfer/offer_received/v_1787094819660.ogg" download="v_1787094819660.ogg">⬇ 13 KB</a> &nbsp; 
<small>ogg: 13 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094819660-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094819660-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094819660-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.transfer/offer_received/v_1787094819660.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.transfer/offer_received/v_1787094819660.ogg`
- approve:   `bun run game:audio:approve sfx.transfer.offer_received --variant v_1787094819660`
- deprecate: `bun run game:audio:deprecate sfx.transfer.offer_received --variant v_1787094819660`
- re-stage:  `bun run game:audio:stage sfx.transfer.offer_received`

Notes:

```

---

### `sfx.transfer.scout_report`

> Informe de ojeador. Papel + campana.

#### `v_1787094951114`

<audio id="aud-v_1787094951114" controls preload="none" src="../../.generate/audios/staging/sfx.transfer/scout_report/v_1787094951114.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094951114" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094951114" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.transfer/scout_report/v_1787094951114.ogg" download="v_1787094951114.ogg">⬇ 688 KB</a> &nbsp; 
<small>ogg: 688 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094951114-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094951114-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094951114-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.transfer/scout_report/v_1787094951114.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.transfer/scout_report/v_1787094951114.ogg`
- approve:   `bun run game:audio:approve sfx.transfer.scout_report --variant v_1787094951114`
- deprecate: `bun run game:audio:deprecate sfx.transfer.scout_report --variant v_1787094951114`
- re-stage:  `bun run game:audio:stage sfx.transfer.scout_report`

Notes:

```

---

### `sfx.transfer.transfer_complete`

> Fichaje cerrado. Caja registradora.

#### `v_1787095279647`

<audio id="aud-v_1787095279647" controls preload="none" src="../../.generate/audios/staging/sfx.transfer/transfer_complete/v_1787095279647.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787095279647" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787095279647" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.transfer/transfer_complete/v_1787095279647.ogg" download="v_1787095279647.ogg">⬇ 391 KB</a> &nbsp; 
<small>ogg: 391 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787095279647-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787095279647-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787095279647-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.transfer/transfer_complete/v_1787095279647.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.transfer/transfer_complete/v_1787095279647.ogg`
- approve:   `bun run game:audio:approve sfx.transfer.transfer_complete --variant v_1787095279647`
- deprecate: `bun run game:audio:deprecate sfx.transfer.transfer_complete --variant v_1787095279647`
- re-stage:  `bun run game:audio:stage sfx.transfer.transfer_complete`

Notes:

```

---

### `sfx.transfer.transfer_rejected`

> Oferta rechazada. Descendente.

#### `v_1787094951110`

<audio id="aud-v_1787094951110" controls preload="none" src="../../.generate/audios/staging/sfx.transfer/transfer_rejected/v_1787094951110.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787094951110" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787094951110" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.transfer/transfer_rejected/v_1787094951110.ogg" download="v_1787094951110.ogg">⬇ 733 KB</a> &nbsp; 
<small>ogg: 733 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787094951110-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787094951110-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787094951110-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.transfer/transfer_rejected/v_1787094951110.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.transfer/transfer_rejected/v_1787094951110.ogg`
- approve:   `bun run game:audio:approve sfx.transfer.transfer_rejected --variant v_1787094951110`
- deprecate: `bun run game:audio:deprecate sfx.transfer.transfer_rejected --variant v_1787094951110`
- re-stage:  `bun run game:audio:stage sfx.transfer.transfer_rejected`

Notes:

```

---

### `sfx.achievement` — ✅ all staged

> Career achievement stingers — manager of the month, season complete, promotion, relegation, trophy lift. Each is a short fanfare that complements music.trophy.trophy_lift without overshadowing it.

### `sfx.achievement.manager_of_month`

> Manager del mes. Brass 3 notas.

#### `v_1787095137928`

<audio id="aud-v_1787095137928" controls preload="none" src="../../.generate/audios/staging/sfx.achievement/manager_of_month/v_1787095137928.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787095137928" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787095137928" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.achievement/manager_of_month/v_1787095137928.ogg" download="v_1787095137928.ogg">⬇ 668 KB</a> &nbsp; 
<small>ogg: 668 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787095137928-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787095137928-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787095137928-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.achievement/manager_of_month/v_1787095137928.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.achievement/manager_of_month/v_1787095137928.ogg`
- approve:   `bun run game:audio:approve sfx.achievement.manager_of_month --variant v_1787095137928`
- deprecate: `bun run game:audio:deprecate sfx.achievement.manager_of_month --variant v_1787095137928`
- re-stage:  `bun run game:audio:stage sfx.achievement.manager_of_month`

Notes:

```

---

### `sfx.achievement.season_complete`

> Temporada completa. ding-ding-ding.

#### `v_1787095137927`

<audio id="aud-v_1787095137927" controls preload="none" src="../../.generate/audios/staging/sfx.achievement/season_complete/v_1787095137927.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787095137927" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787095137927" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.achievement/season_complete/v_1787095137927.ogg" download="v_1787095137927.ogg">⬇ 681 KB</a> &nbsp; 
<small>ogg: 681 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787095137927-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787095137927-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787095137927-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.achievement/season_complete/v_1787095137927.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.achievement/season_complete/v_1787095137927.ogg`
- approve:   `bun run game:audio:approve sfx.achievement.season_complete --variant v_1787095137927`
- deprecate: `bun run game:audio:deprecate sfx.achievement.season_complete --variant v_1787095137927`
- re-stage:  `bun run game:audio:stage sfx.achievement.season_complete`

Notes:

```

---

### `sfx.achievement.promotion`

> Ascenso. Brass brillante.

#### `v_1787095137927`

<audio id="aud-v_1787095137927" controls preload="none" src="../../.generate/audios/staging/sfx.achievement/promotion/v_1787095137927.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787095137927" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787095137927" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.achievement/promotion/v_1787095137927.ogg" download="v_1787095137927.ogg">⬇ 720 KB</a> &nbsp; 
<small>ogg: 720 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787095137927-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787095137927-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787095137927-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.achievement/promotion/v_1787095137927.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.achievement/promotion/v_1787095137927.ogg`
- approve:   `bun run game:audio:approve sfx.achievement.promotion --variant v_1787095137927`
- deprecate: `bun run game:audio:deprecate sfx.achievement.promotion --variant v_1787095137927`
- re-stage:  `bun run game:audio:stage sfx.achievement.promotion`

Notes:

```

---

### `sfx.achievement.relegation`

> Descenso. Chime menor + groan.

#### `v_1787095507980`

<audio id="aud-v_1787095507980" controls preload="none" src="../../.generate/audios/staging/sfx.achievement/relegation/v_1787095507980.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787095507980" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787095507980" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.achievement/relegation/v_1787095507980.ogg" download="v_1787095507980.ogg">⬇ 757 KB</a> &nbsp; 
<small>ogg: 757 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787095507980-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787095507980-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787095507980-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.achievement/relegation/v_1787095507980.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.achievement/relegation/v_1787095507980.ogg`
- approve:   `bun run game:audio:approve sfx.achievement.relegation --variant v_1787095507980`
- deprecate: `bun run game:audio:deprecate sfx.achievement.relegation --variant v_1787095507980`
- re-stage:  `bun run game:audio:stage sfx.achievement.relegation`

Notes:

```

---

#### `v_1787095363924`

<audio id="aud-v_1787095363924" controls preload="none" src="../../.generate/audios/staging/sfx.achievement/relegation/v_1787095363924.ogg" style="width:100%;max-width:600px;height:32px;"></audio>

<div id="viz-v_1787095363924" style="height:6px;background:#222;border-radius:3px;margin:4px 0 8px;width:100%;max-width:600px;overflow:hidden"><div id="bar-v_1787095363924" style="height:100%;width:0%;background:linear-gradient(90deg,#4caf50,#8bc34a);transition:width 0.1s linear"></div></div>

<a href="../../.generate/audios/staging/sfx.achievement/relegation/v_1787095363924.ogg" download="v_1787095363924.ogg">⬇ 1286 KB</a> &nbsp; 
<small>ogg: 1286 KB &nbsp;·&nbsp; <strong>Para oír:</strong> copia el comando <code>listen</code> de abajo y ejecútalo en una terminal (sale por WSLg → speakers Windows). El preview muestra el progreso pero no emite audio (limitación del sandbox de VS Code en WSL).</small>


<form>
<label><input type="checkbox" name="v_1787095363924-ok"> OK</label> &nbsp;
<label><input type="checkbox" name="v_1787095363924-bad"> Bad</label> &nbsp;
<label><input type="checkbox" name="v_1787095363924-restage"> Re-stage</label>
</form>

Commands (copy-paste into terminal):

- ▶ listen (browser, recomendado): abre [esta URL](http://172.21.118.36:7333/track/sfx.achievement/relegation/v_1787095363924.ogg) en Brave/Chrome/Edge — necesita que el server esté corriendo con `bun run game:audio:review:ui`
- ▶ listen (terminal):  `PULSE_SERVER=unix:/mnt/wslg/PulseServer ffplay -nodisp -autoexit .generate/audios/staging/sfx.achievement/relegation/v_1787095363924.ogg`
- approve:   `bun run game:audio:approve sfx.achievement.relegation --variant v_1787095363924`
- deprecate: `bun run game:audio:deprecate sfx.achievement.relegation --variant v_1787095363924`
- re-stage:  `bun run game:audio:stage sfx.achievement.relegation`

Notes:

```

---

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
