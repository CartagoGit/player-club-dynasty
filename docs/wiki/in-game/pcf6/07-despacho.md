# PC Fútbol 6.0 — el despacho: hub central de Liga Manager

`[jugado]` 2026-08-21. Tras nombre y equipo
([`05-liga-manager-nueva-partida.md`](05-liga-manager-nueva-partida.md)),
personalización
([`06-liga-manager-personalizacion.md`](06-liga-manager-personalizacion.md))
y una pantalla de pretemporada (amistosos, no documentada aparte — ver más
abajo), se llega aquí: el **despacho**, la pantalla desde la que se juega
toda la temporada. No es una pantalla más del flujo inicial — es el hub al
que se vuelve constantemente durante toda la carrera.

![El despacho: hub de Liga Manager](screenshots/liga-manager/06-despacho.png)

```
┌──────────────────────────────────────────────────────────────────┐
│[F.C.B][F.C. Barcelona / Van Gaal / CPU] [Jueves 14 Agosto 1997]   │
│                                    [Real Madrid C.F./dynasty/JUG 1][RMA]│
├────────┬─────────────────────────────┬─────────────────────────┬─┤
│S       │ (mensaje del asistente 1)  │ (mensaje del asistente 2)│E│
│E       │  RESULTADOS                │  ALINEACIÓN              │N│
│G       │  CLASIFICACIÓN              │  TÁCTICAS                │T│
│U       │  CALENDARIO                 │  VER RIVAL                │R│
│I       │      Pretemporada            │      Preparación          │E│
│M       │  FICHAR                     │  CAJA                     │N│
│I       │  EMPLEADOS                  │  DECISIONES                │A│
│E       │  PLANTILLA                  │  ESTADIO                  │D│
│N       │ (hint del asistente, línea) │                           │O│
│T       │                             │                           │ │
│O       ├─────────────────────────────┴─────────────────────────┴─┤
│(plant.)│ [SALIR] [GRABAR LIGA]      [NOTICIAS]        [SEGUIR]    │
└────────┴─────────────────────────────────────────────────────────┘
```

- **Cabecera de partido, no de menú.** No dice "Liga Manager" — dice quién
  juega contra quién en la próxima fecha (`F.C. Barcelona / Van Gaal / CPU`
  vs. `Real Madrid C.F. / dynasty / JUG 1`) y la fecha exacta (`Jueves 14
  Agosto 1997` — la misma que se fijó en la pantalla de amistosos). El
  despacho está siempre anclado al **próximo partido**, no a un estado
  neutro.
- **Columna izquierda de retratos**: una fila vertical de miniaturas con
  camiseta + un contador numérico con icono (visto: números junto a un
  icono de "+"y de alerta roja "!") junto a cada una — muy probablemente la
  plantilla o el cuerpo técnico en miniatura, con avisos (lesión, sanción,
  etc.) codificados por icono. Pendiente de confirmar clicando cada
  miniatura.
- **Ocho acciones organizadas en una rejilla 2×4 con eje temático**, no una
  lista plana: las etiquetas verticales de los bordes (`SEGUIMIENTO`,
  `MERCADO` a la izquierda; `ENTRENADOR`, `FINANZAS` a la derecha) agrupan
  las columnas, y dos subtítulos internos (`Pretemporada`, `Preparación`)
  agrupan las filas de abajo. Es una **matriz categoría×momento**, no solo
  una lista de botones:

  | | Columna izquierda | Columna derecha |
  |---|---|---|
  | Fila superior | RESULTADOS, CLASIFICACIÓN, CALENDARIO | ALINEACIÓN, TÁCTICAS, VER RIVAL |
  | Fila inferior | FICHAR, EMPLEADOS, PLANTILLA | CAJA, DECISIONES, ESTADIO |

- **Dos cajas de texto del asistente**, una encima de cada columna de
  botones — cambian de contenido según el último botón tocado/pasado por
  encima (mensajes vistos: *"Hola, soy tu asistente..."*, *"Puedes asignar
  los dorsales"*, *"...no tendrás más remedio que realizarlo antes de..."*)
  — un sistema de ayuda contextual **por hover**, coherente con el selector
  de verbosidad de la pantalla de personalización
  (`06-liga-manager-personalizacion.md`).
- **Barra inferior con cuatro acciones globales**: SALIR (volver al menú
  principal, se asume), GRABAR LIGA (guardar), NOTICIAS, SEGUIR (avanzar el
  día/la simulación — el mismo verbo que ya se usó para avanzar entre
  pantallas del asistente de creación, reutilizado aquí como "pasar tiempo").

## Confirmado navegable

- **CLASIFICACIÓN** → tabla de Liga completa, ver
  [`09-clasificacion.md`](09-clasificacion.md).
- **EMPLEADOS** → plantilla de personal técnico y de club, ver
  [`08-personal-del-club.md`](08-personal-del-club.md).

## Pendiente / con comportamiento raro

- **PLANTILLA y FICHAR no navegaron** pese a hover confirmado sobre el botón
  correcto (cursor visible encima, coordenada verificada). El mensaje del
  asistente en ese momento decía algo como *"...no tendrás más remedio que
  realizarlo antes de..."* — hipótesis: alguna acción (¿asignar dorsales?
  ¿revisar la plantilla desde otro punto?) puede ser un prerrequisito
  obligatorio antes de que esas pantallas se abran normalmente. No
  descartada una simple coordenada aún mal calibrada — recalibrar con el
  método del rectángulo de arrastre (`../como-esta-montado.md`) antes de
  concluir que es un bloqueo real de flujo.
- **RESULTADOS, CALENDARIO, ALINEACIÓN, TÁCTICAS, VER RIVAL, CAJA,
  DECISIONES, ESTADIO, SALIR, GRABAR LIGA, NOTICIAS, SEGUIR** — no
  probados todavía en esta sesión.
- **La pantalla de pretemporada** (calendario de amistosos, mapa
  Europa/Sudamérica, selección de rival por escudo con autocompletado de
  estadio) apareció entre "Equipos en competiciones"
  (`screenshots/liga-manager/04-pretemporada.png`) y este despacho, pero no
  tiene aún ficha propia — el patrón de interacción "elegir de una rejilla →
  se autocompleta y avanza al siguiente slot" ya se documentó en
  `05-liga-manager-nueva-partida.md` (incorporarlo también aquí en una
  próxima pasada).

## Qué exige

**El despacho es la escena central de PC Dynasty's career mode** — el
equivalente directo de lo que allí se llame "hub" o "oficina del mánager".
La matriz categoría×momento (Seguimiento/Mercado × Entrenador/Finanzas,
Pretemporada/Preparación) es un candidato fuerte para la organización real
del menú de navegación de esa escena, no una lista plana de botones. El
sistema de asistente-por-hover es una pieza de UX transferible tal cual:
texto contextual que cambia según qué se está mirando, no un tutorial fijo.
