# PC Fútbol 6.0 — CALENDARIO: mes navegable + próximos partidos

`[jugado]` 2026-08-21. Se llega desde el despacho pulsando **CALENDARIO**
(fila superior izquierda, bajo RESULTADOS/CLASIFICACIÓN — ver
[`07-despacho.md`](07-despacho.md)). Confirmado navegable, sin anomalía.

![Calendario de agosto/septiembre 1997](screenshots/calendario/01-calendario-agosto-septiembre.png)

## Qué se ve

- **Calendario mensual** (izquierda): cuadrícula de mes con marcadores de
  color por día — un código de color por competición, coherente con la
  leyenda que aparece en la franja superior del hub (mismo lenguaje visual
  que el resto del juego).
- **Columna central "HOY" + "PRÓXIMOS"**: tarjetas apiladas, una por
  partido, con fecha y rival — el partido de hoy resaltado, seguido de los
  próximos en orden cronológico. No es solo un calendario visual: es
  también una lista de agenda.
- **Botonera inferior**: `RESULTADOS`, `CLASIFICACIÓN`, `VISIONAR`,
  `VOLVER` — RESULTADOS y CLASIFICACIÓN saltan directamente a esas otras
  vistas (atajo cruzado entre las tres pantallas hermanas, ver
  [`13-resultados.md`](13-resultados.md)); VOLVER confirmado navegable sin
  anomalía.

## Qué exige

El patrón "agenda de próximos eventos + accesos directos a vistas
relacionadas" es más rico que un simple calendario de casillas — PC
Dynasty debería modelar el calendario como una cola de eventos navegable,
no solo una cuadrícula de fechas, con saltos directos a resultados y
clasificación desde el mismo sitio donde se consulta cuándo es el próximo
partido.

## Pendiente

- **VISIONAR no probado deliberadamente** — por el nombre, es candidato a
  disparar una previsualización/visionado del próximo partido, lo cual
  entra en conflicto con la instrucción explícita de esta sesión de nunca
  usar el modo "ver partido" (solo simulación por resultado). Queda
  pendiente de una sesión futura que confirme qué hace sin comprometer esa
  regla — puede que solo muestre una alineación probable sin simular nada,
  en cuyo caso sería seguro probarlo.
- No se ha navegado el calendario a otros meses (flechas de mes, si las
  hay) para confirmar que cubre toda la temporada.
