# Qué se repite entre PC Fútbol 6.0, PC Fútbol 7 y PC Basket 6.5

`[jugado]` 2026-08-20, las tres sesiones de esta carpeta comparadas.

| | PC Fútbol 6.0 | PC Fútbol 7 | PC Basket 6.5 |
|---|---|---|---|
| Disposición del menú | Rejilla 3×3 | Rejilla 3×3 (idéntica) | Hexágono radial |
| Nº de categorías principales | 9 | 9 | 6 |
| Riel de marca | Lateral izquierdo | Lateral izquierdo | Inferior |
| Licencia oficial | LFP | LFP | ACB |
| Meta-acciones (barra inferior) | 4 | 5 | 4 |
| Arranca solo al insertar el disco | Sí | No (atajo en Inicio → Programas) | Sí |

## La lectura

**Lo que es del "manager Dinamic" en general:** el vocabulario de loseta
(icono + rótulo en mayúsculas), el patrón de submenú modal con lista
numerada+icono+párrafo (visto en 6.0, sección [`pcf6/01-submenu-y-ayuda.md`](pcf6/01-submenu-y-ayuda.md)),
y el conjunto conceptual de categorías: base de datos, seguimiento/
estadísticas, partido amistoso, un desafío/evento especial, liga manager,
liga promanager.

**Lo que es específico de la línea de fútbol:** la disposición en rejilla.
Baloncesto resuelve las mismas seis categorías con una constelación radial,
no con casillas. La rejilla no es "la forma en que Dinamic hace menús": es
la forma en que **esta saga de fútbol** los hace, estable de 6.0 a 7.0.

**Lo que cambia incluso dentro de la misma saga (6.0 → 7):** qué entra en la
rejilla y qué se degrada a meta-acción. Proquinielas es loseta en 6.0 y
meta-acción en 7. Guía Rápida aparece en 7 sin precedente en 6.0. La frontera
entre "modo" y "utilidad" se movió una vez entre dos versiones consecutivas —
no es una frontera fija ni siquiera dentro del mismo juego.

## Para PC Dynasty

Confirma sin matices el diseño ya construido en `DestinationBoard`/
`DestinationTile` (f00051 S3): rejilla 3×3, riel lateral, barra de
meta-acciones — es exactamente la forma de PC Fútbol 6.0/7.0, el objetivo
declarado. El hallazgo de Basket 6.5 es una advertencia hacia el futuro, no
una corrección del presente: si algún día se explora un modo o deporte
distinto, no asumir que la rejilla viaja con él — sólo el vocabulario de
categorías y el patrón de loseta lo hacen.
