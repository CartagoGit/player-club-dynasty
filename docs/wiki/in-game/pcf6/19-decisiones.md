# PC Fútbol 6.0 — DECISIONES: junta directiva, historial y TV/merchandising

`[jugado]` 2026-08-21. Se llega desde el despacho pulsando **DECISIONES**
(fila inferior derecha, bajo CAJA — ver [`07-despacho.md`](07-despacho.md)).
Título real de la pantalla: **"JUNTA DIRECTIVA"**. Confirmado navegable,
sin anomalía — y con dos sub-pantallas propias exploradas a fondo.

![Junta Directiva: confianza, créditos y primas](screenshots/decisiones/01-junta-directiva.png)

## La pantalla principal: cómo te perciben, y tus primas pactadas

- **`EVALUACIÓN COMO MANAGER`**, **`CONFIANZA DE LOS DIRECTIVOS`**,
  **`CONFIANZA DEL PÚBLICO`** — tres barras independientes (0-5 con
  segmentos rojo/marrón), no un único "prestigio": la junta, la afición y
  tu propia evaluación de gestión son ejes separados.
- **`CRÉDITOS`**: tabla de préstamos activos (cantidad, años, a pagar,
  semana) — vacía al inicio de esta partida.
- **`PRIMAS`**: `POR PARTIDO` y `POR TÍTULO`, cada una con selector
  numérico (flechas ◄►, en millones) y su propio botón `OK` — primas
  configurables por el propio manager, pactadas con la plantilla como
  incentivo, no fijas.
- **Botonera inferior**: `OFERTAS TV`, `MERCHANDISING` (ambas
  **deshabilitadas/atenuadas** en esta partida — ver por qué en
  `INFO TV/MERCHAN.` abajo), `INFO TV/MERCHAN.`, **`INFO MANAGER`**,
  `VOLVER` (navegable, sin anomalía).

## INFO MANAGER → "HISTORIAL DEL MANAGER"

![Historial del Manager: trayectoria + récord por competición](screenshots/decisiones/02-historial-manager.png)

- **Tabla de trayectoria por temporada/equipo**: división, posición
  objetivo (`SÍ`/`NO`), confianza de directiva y de afición de cada
  etapa — el historial completo de carrera del manager, con una fila por
  temporada/club.
- **Tabla de récord por competición** (`LIGA`, `COPA DEL REY`,
  `SUPERC.ESP.`, `U.E.F.A.`, `RECOPA`, `LIGA CAMP.`, `SUPERC.EUR.`,
  `C.INTERCONT.`): jugados/ganados/empatados/perdidos/goles a favor/goles
  en contra/posición final — en la sesión, Supercopa mostraba
  `1 0 1 0 0 0 Subcampeón` tras el 0-0 de ida (empate cuenta como "1 empate",
  posición aún no resuelta hasta la vuelta).
- `VOLVER` (navegable, sin anomalía).

## INFO TV/MERCHANDISING

![Info TV/Merchandising: contrato + seis líneas de producto](screenshots/decisiones/03-info-tv-merchandising.png)

- **`INFO CONTRATO TELEVISIÓN`**: mensaje *"Todavía no tienes ningún
  contrato por tus derechos de Televisión. Espera a recibir ofertas."* —
  explica por qué `OFERTAS TV` aparecía deshabilitado en la pantalla
  principal: no es un botón roto, es un estado "sin oferta todavía" que se
  resuelve solo con el tiempo (SEGUIR), no hay una acción directa para
  solicitar el contrato.
- **`INFO MERCHANDISING`**: seis líneas de producto con ficha propia —
  `CAMISETAS`, `GORRAS`, `PINS-LLAVEROS`, `BUFANDAS`, `BANDERAS`, `CD-ROM`
  — cada una con `U. FABRICADAS`, `U. VENDIDAS`, `GASTOS`, `INGRESOS`,
  `BENEFICIOS` por temporada actual, más una fila `TOTAL` agregada. Todo en
  cero al inicio de temporada.
- `VOLVER` (navegable, sin anomalía).

## Qué exige

El patrón de tres barras de confianza independientes (junta/afición/
autoevaluación), primas configurables con OK explícito, y un historial de
manager separado del historial de club, es la referencia de qué debería
rastrear la carrera de manager en PC Dynasty más allá de resultados
deportivos: reputación con múltiples stakeholders, no un único número. El
desglose de merchandising por línea de producto (con fabricación vs. venta
vs. beneficio) es igualmente aplicable a cualquier sistema de ingresos
comerciales que se quiera modelar.

## Pendiente

- `OFERTAS TV` y `MERCHANDISING` seguían deshabilitados toda la sesión —
  no se ha llegado al punto en que el club recibe su primera oferta;
  repetir tras avanzar varias semanas con SEGUIR para ver el flujo
  completo de aceptar una oferta.
- Los dos `OK` de `PRIMAS` no se han pulsado (para no alterar el estado de
  la partida sin necesidad) — confirmar que aceptan un valor y qué pasa si
  se deja en 0.
