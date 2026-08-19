# 16 · Lectura de capturas: qué se ve exactamente, componente a componente

Este capítulo es distinto de [`14`](14-catalogo-visual.md). El 14 es el **índice**
de dónde está cada referencia. Éste es la **lectura**: qué hay en cada captura,
zona por zona, con el nivel de detalle que hace falta para construir el componente
sin volver a mirar la imagen.

Se escribió mirando las capturas una por una. Cada sección dice de qué fuente
salió, describe la pantalla, y termina en **qué componente exige** — con el
nombre que ese componente tiene (o tendrá) en
[`../pc-dynasty/mapa-de-equivalencias.md`](../pc-dynasty/mapa-de-equivalencias.md).

> Recordatorio de la política de [`14`](14-catalogo-visual.md) §1: aquí no entra
> ninguna imagen. Se enlaza la fuente, se describe, y el wireframe es nuestro.

---

## 1. PC Fútbol 6.0 — menú principal

Fuente: [`cdn.devuego.es/capturas/624_1.jpg`](https://cdn.devuego.es/capturas/624_1.jpg)
· ficha: <https://www.devuego.es/presskit/pc-futbol-60>

```
┌──────────────────────────────────────────────────────────────┐
│  PC FÚTBOL                ┌──────┐ ┌──────┐ ┌──────┐         │
│    6.0                    │ HIST │ │ BASE │ │GOLES │         │
│  TEMPORADA 97-98          │ORIA  │ │DATOS │ │LIGA  │         │
│                           └──────┘ └──────┘ └──────┘         │
│  ▣ dinamic                ┌──────┐ ┌──────┐ ┌──────┐         │
│    multimedia             │SEGUI-│ │PARTI-│ │DESA- │         │
│                           │MIENTO│ │ AMIS.│ │ FÍO  │         │
│  eXtensión 1  (gris)      └──────┘ └──────┘ └──────┘         │
│  eXtensión 2  (gris)      ┌──────┐ ┌──────┐ ┌──────┐         │
│                           │ LIGA │ │ LIGA │ │PROQUI│         │
│                           │MANAGER│ │PROMAN│ │NIELAS│        │
│  ┌───┐                    └──────┘ └──────┘ └──────┘         │
│  │LFP│                                                       │
│  └───┘   ⓘ Instrucciones  ⚽ Infofutbol  ▣ Créditos   ↩ SALIR │
│ F1.2                                                         │
└──────────────────────────────────────────────────────────────┘
```

Lo que se ve, y por qué importa:

- **Rejilla 3×3 de destinos.** Nueve, todos visibles, ninguno detrás de un
  submenú. Cada mosaico es una ilustración con **la etiqueta dentro**, abajo a la
  izquierda, en mayúsculas blancas con sombra, montada sobre la imagen — no
  debajo de ella.
- **Los destinos no instalados siguen ahí.** «eXtensión 1» y «eXtensión 2»
  aparecen en gris, en su sitio, en la columna izquierda. El menú es el **mapa
  del producto**, y un mapa con huecos es más honesto que un mapa que miente
  sobre su tamaño. Es la regla que adopta [`f00051` S3].
- **Raíl inferior de comandos pequeños**: tres a la izquierda del centro
  (Instrucciones, Infofútbol, Créditos), cada uno icono + etiqueta, y **SALIR
  separado, pegado al borde derecho**. Salir nunca comparte grupo con lo demás.
- **Identidad arriba a la izquierda**: logo del juego, temporada, y marca del
  editor debajo. El número de versión («F1.2») en la esquina inferior izquierda,
  en cuerpo mínimo.

→ Componentes: `DestinationTile`, `CommandRail`, `VersionStamp`.

---

## 2. PC Fútbol 5.0 — «MENÚ MANAGER», el eje de la carrera

Fuente: [`pc-futbol-5-0_2.png`](https://www.myabandonware.com/media/screenshots/p/pc-futbol-5-0-q6n/pc-futbol-5-0_2.png)
· ficha: <https://www.myabandonware.com/game/pc-futbol-5-0-gzl>

```
┌──────────────────────────────────────────────────────────────┐
│ [ffb        ]│         MENU MANAGER        │ Agosto │Pretemp.│
│ [Real Madrid]│                             │ 14 Mié │Prepara.│
├──────────────────────────────────────────────────────────────┤
│ ▤ RESULTADOS                    ╭─────────╮   ALINEACIÓN ▤   │
│ ▤ CLASIFICACIÓN                 │  CPU    │   TÁCTICAS   ▤   │
│ ▤ CALENDARIO                    │  Díaz   │   VER RIVAL  ▤   │
│════ SEGUIMIENTO ════            │  River  │  ═ ENTRENADOR ═  │
│ [↩ SALIR][▤ GRABAR LIGA]        │ R.Madrid│ [▤NOTICIAS][▶SEGUIR]│
│════ MERCADO ════════            │  ffb    │  ═ FINANZAS ══   │
│ ▤ FICHAR                        │  JUG 1  │   CAJA       ▤   │
│ ▤ PLANTILLA                     ╰─────────╯   DECISIONES ▤   │
│ ▤ EMPLEADOS                          ▼        ESTADIO    ▤   │
└──────────────────────────────────────────────────────────────┘
```

- **Cuatro áreas, tres destinos cada una.** SEGUIMIENTO · ENTRENADOR · MERCADO ·
  FINANZAS. Nueve destinos y cuatro comandos, todo en una pantalla.
- **El centro es el próximo partido**: escudos, quién controla cada banquillo
  (`CPU` frente a `JUG 1`), el entrenador rival, y una bandera de competición.
  El eje de la pantalla no es un menú, es *lo siguiente que va a pasar*.
- **Cada destino lleva su icono al lado exterior**, no dentro de la barra.
- **GRABAR LIGA está en la barra de comandos**, junto a SALIR. Guardar era una
  acción explícita y visible, no algo implícito. Es el argumento directo del
  guardarraíl de salida que ya tenemos.

→ Componentes: `AreaGroup`, `NextFixturePlate`, `CommandBar`.

---

## 3. PC Fútbol 6.0 — el mismo eje, un año después

Fuente: [`cdn.devuego.es/capturas/624_5.jpg`](https://cdn.devuego.es/capturas/624_5.jpg)

Misma pantalla, misma estructura, **dos cambios que enseñan cuál es la regla y
cuál el adorno**:

| | 5.0 | 6.0 |
|---|---|---|
| Nombre del área | banda horizontal en el centro | **raíl vertical en el borde exterior**, texto girado 90° |
| Color SEGUIMIENTO | verde oliva | rojo |
| Color ENTRENADOR | azul | verde |
| Color MERCADO | rojo | turquesa |
| Color FINANZAS | naranja | ámbar |
| Agrupación | 4 áreas × 3 destinos | **idéntica** |
| Comandos | SALIR · GRABAR LIGA · NOTICIAS · SEGUIR | **idénticos** |

> **Hallazgo normativo.** Los colores concretos **cambiaron entre versiones**; la
> agrupación en cuatro áreas y el reparto de destinos **no**. Luego el invariante
> de la gramática es *«cuatro áreas separadas por color, y el color se repite
> dentro de las pantallas que abre»*, no una paleta concreta. Esto es lo que
> legitima que PC Dynasty tenga temas alternativos: mientras la separación se
> mantenga, la gramática sobrevive al cambio de paleta — igual que se comprueba
> en la variante argentina ([`15`](15-variantes-argentina-y-selecciones.md)).

Añadidos de 6.0 sobre 5.0:

- **Cintillo de noticias** bajo la banda superior, con una frase corrida
  («baja para 2 semanas. Tiene gastroenteritis.»). Las noticias no esperan a que
  entres en NOTICIAS: la más reciente te alcanza en el eje.
- **Columnas laterales de retratos**: la directiva y el cuerpo técnico como caras,
  no como filas de tabla.
- **Placa de fecha propia** con día, mes y año en tres líneas, más el número de
  jornada al lado.

→ Componentes: `EdgeRail` (ya existe), `NewsTicker`, `DatePlate` (ya existe),
`StaffColumn`.

---

## 4. PC Fútbol 6.0 — alineación (la pantalla eje del juego)

Fuente: [`cdn.devuego.es/capturas/624_6.jpg`](https://cdn.devuego.es/capturas/624_6.jpg)

```
┌──────────────────────────────────────────────────────────────┐
│ Tottenham H.  │      ALINEACIÓN      │ Domingo │ Premier     │
│ Manchester Utd│                      │ 31 Ago  │ Semana 2    │
├────────────────────────────────────────────┬─────────────────┤
│ Nº JUGADOR    EN VE RE AG CA EF MO ME ROL D│ ▸PARÁMETROS     │
│ 17 Van der Sar 99 80 87 80 78 55 90 76  POR│  CALIFICACIÓN   │
│  2 Gary Neville…                        DEF│ ┌─────────────┐ │
│ …                                          │ │ MEDIA EQUIPO│ │
│ ╞══ JUGADORES CONVOCADOS ══════════════════╡ │  ★ ★ ★ ★    │ │
│ 11 Kuszczak    …                        POR│ └─────────────┘ │
│ …                                          │ PORTERO   PASE  │
│ ╞══ JUGADORES NO CONVOCADOS ═══════════════╡ REGATE   REMATE │
│ 12 Hargreaves  …                        MED│ ENTRADAS   TIRO │
│ …                                          │ ┌───────────┐   │
│                                            │ │  campo    │   │
│                                            │ └───────────┘   │
│                                            │ ENTRENAMIENTO   │
│                                            │ LESIONADOS      │
│                                            │ ESTADÍSTICAS    │
│                                            │ TÁCTICA │VOLVER │
└────────────────────────────────────────────┴─────────────────┘
```

Detalles que sólo se ven mirando:

- **Columnas exactas**: `EN VE RE AG CA EF MO ME ROL DEM`. Confirmado.
- **Tres bloques con cabecera propia**: el once (sin cabecera, arriba), luego
  `JUGADORES CONVOCADOS`, luego `JUGADORES NO CONVOCADOS`. Una sola tabla,
  cortada por barras de sección a todo el ancho.
- **La fila entera va teñida por demarcación** — portero, defensas, medios y
  delanteros tienen fondos distintos — *además* de llevar la columna `DEM` con
  el texto. El color no sustituye al dato: lo duplica, para poder leer la tabla
  de un vistazo y con precisión.
- **Cada celda de atributo va teñida por su valor**, verde alto → rojo bajo. La
  tabla es un mapa de calor, no una rejilla de números.
- **Conmutador de vista** `PARÁMETROS` / `CALIFICACIÓN` arriba del raíl derecho:
  dos pestañas sobre **la misma tabla**, no dos pantallas.
- **Tarjeta del equipo**: `MEDIA EQUIPO` con estrellas.
- **Seis destrezas nombradas en dos columnas**: PORTERO · PASE / REGATE ·
  REMATE / ENTRADAS · TIRO.
- **Diagrama de campo** con la disposición, debajo de las destrezas.
- **VOLVER siempre en la esquina inferior derecha.**

→ Componentes: `ViewSwitch`, `StarRating`, `SkillGrid`, `PitchDiagram`,
`RowTab`, `HeatCell`, `SectionRow`.

---

## 5. PC Fútbol 5.0 — ver rival (el mismo raíl, aplicado a otro club)

Fuente: [`pc-futbol-5-0_8.png`](https://www.myabandonware.com/media/screenshots/p/pc-futbol-5-0-q6n/pc-futbol-5-0_8.png)

La misma tabla y el mismo raíl que la alineación, apuntando al rival. Lo que
añade sobre la lectura anterior:

- **La pestaña activa lleva un marcador** — un triángulo rojo pegado a su borde
  izquierdo — *además* del cambio de color. Dos señales para un estado.
- **La placa de club del raíl dice quién lo controla**: bajo el escudo, el
  nombre y `MEDIA EQUIPO ★★★★ 76`, y debajo una barra con `ORDENADOR`. Saber si
  un club lo lleva la máquina o una persona es información de primera clase.
- **La tarjeta del jugador seleccionado vive abajo a la izquierda**, con su
  nombre en una barra de título y las seis destrezas en rejilla 2×3, cada una
  con **número y estrellas a la vez** (PASE 88 ★★★★, ENTRADAS 61 ★★★).
- **Placa de personal**: `ASISTENTE`, con retrato, nombre y sus propias
  estrellas. El cuerpo técnico se valora con la misma unidad que los jugadores.

→ Componentes: `PlayerCard`, `StaffPlate`, `ControlBadge`.

---

## 6. PC Fútbol 6.0 — ficha de club (base de datos)

Fuente: [`cdn.devuego.es/capturas/624_3.jpg`](https://cdn.devuego.es/capturas/624_3.jpg)

**La captura más importante para nuestra gramática**, porque contiene el patrón
que más se repite en toda la saga y que nosotros no tenemos:

```
   ┌────────────────────────────────┐
   │ ESTADIO                        │   ← barra clara, ETIQUETA en negrita
   ├────────────────────────────────┤
   │ Santiago Bernabéu              │   ← barra saturada, valor en blanco
   └────────────────────────────────┘
```

- **Ningún dato se escribe como «Etiqueta: valor».** Siempre son **dos barras
  apiladas**: arriba el nombre del campo, en mayúsculas oscuras sobre fondo
  claro; debajo el valor, en blanco sobre un color saturado.
- **El color de la barra de valor agrupa por familia de dato**: verde para los
  datos del estadio, azul para los del club, morado para nombre, patrocinador y
  equipador. El color no decora: clasifica.
- **Los pares se disponen en rejilla de dos columnas**, y un dato largo ocupa el
  ancho entero (NOMBRE, PATROCINADOR, EQUIPADOR).
- **Escudo enorme arriba a la derecha**, montado a caballo sobre la banda
  superior. La identidad del club domina su ficha.
- **Raíl derecho de comandos con icono encima**: unos guantes sobre `PLANTILLA`,
  un trofeo sobre `PALMARÉS`. El icono anota el comando en lugar de compartir
  línea con él.
- `IMPRIMIR` y `VOLVER` cierran el raíl; VOLVER en verde, siempre el último.

→ Componentes: **`FieldPair`** (el que falta y más se nota), `DossierGrid`,
`CrestBanner`, `IconCommand`.

---

## 7. PC Fútbol 6.0 — pretemporada Promanager

Fuente: [`cdn.devuego.es/capturas/624_2.jpg`](https://cdn.devuego.es/capturas/624_2.jpg)

- **Modo declarado permanentemente**: arriba a la izquierda, sobre un maletín,
  «Liga ProManager» en dos líneas. Siempre sabes bajo qué reglas juegas.
- **Título con el club dentro**: «Pretemporada para Plasencia». El título de
  pantalla nombra al club, no sólo a la pantalla.
- **Mapa con pestañas verticales** `EUROPA` / `SUDAMÉRICA` en su borde izquierdo,
  texto girado. Las banderas sobre el mapa son los controles.
- **Franja de lectura negra** bajo el mapa: muestra el nombre de lo que señalas.
  Un renglón de estado dedicado al hover.
- **Cinco ranuras fijas** para los amistosos, numeradas 1..5 en placas rojas al
  borde derecho. **Las cinco están dibujadas aunque estén vacías**: la pantalla
  no cambia de forma según cuánto hayas hecho.
- Cada ranura: **placa de calendario de taco** (día en rojo, mes, año, borde
  superior troquelado) + rival + estadio.
- Comandos abajo a la derecha, **codificados por intención**: `NO JUGAR`
  (contorno apagado), `BORRAR` (aspa roja), `SEGUIR` (círculo verde). Rojo
  destruye, verde avanza. Es exactamente nuestro `Intent`.

→ Componentes: `SlotList`, `DatePlate`, `HoverReadout`, `ModeBadge`,
`MapPicker`.

---

## 8. Lo que estas ocho capturas cambian en el plan

| Hallazgo | Dónde entra |
|---|---|
| `FieldPair` (etiqueta clara + valor saturado) es **el** patrón de datos de la saga | [`04`](04-gramatica-visual.md), y falta en PC Dynasty |
| La celda de atributo va teñida por valor; la fila, por demarcación | [`04`](04-gramatica-visual.md) — mapa de calor, no rejilla |
| Los colores de área cambiaron entre versiones; la agrupación no | [`00`](00-objetivo-y-alcance.md) §3 — legitima los temas alternativos |
| Guardar es un comando visible de la barra, junto a salir | ya aplicado en el guardarraíl de salida |
| Un destino no disponible se muestra en gris, no se oculta | [`f00051`] S3 |
| El eje de la carrera es el próximo partido, no un menú | [`f00051`], y el hub que aún no tenemos |
| El modo se declara permanentemente en pantalla | [`f00051`] S1 |
| Las noticias alcanzan al jugador en el eje, sin entrar a buscarlas | [`12`](12-mejoras-para-pc-dynasty.md) |

---

## 9. Fuentes de este capítulo

- DeVuego, presskit de PC Fútbol 6.0 — <https://www.devuego.es/presskit/pc-futbol-60>
  (capturas `624_1` … `624_6`).
- My Abandonware, ficha de PC Fútbol 5.0 — <https://www.myabandonware.com/game/pc-futbol-5-0-gzl>
  (30 capturas, `pc-futbol-5-0_1` … `_30`).
- My Abandonware, ficha de PC Fútbol 6.0 — <https://www.myabandonware.com/game/pc-futbol-6-0-gzu>
- Computer Emuzone, ficha de PC Fútbol 7 — <https://computeremuzone.com/ficha/253/pc-futbol-7-temporada-98-99>
- Wikipedia ES, PC Fútbol 6.0 — <https://es.wikipedia.org/wiki/PC_F%C3%BAtbol_6.0>

Ver también [`99-fuentes.md`](99-fuentes.md) para el resto del corpus.
