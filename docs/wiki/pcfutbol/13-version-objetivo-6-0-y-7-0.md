# 13 · La versión objetivo: PC Fútbol 6.0 y 7.0

Capítulo de referencia de la entrega que PC Dynasty toma como modelo. Lo de aquí
tiene **prioridad** sobre cualquier otra versión descrita en esta wiki; ver
[`00-objetivo-y-alcance.md`](00-objetivo-y-alcance.md).

Origen: `[capturas]` análisis de pantallas originales a resolución nativa,
`[wiki]` enciclopedias, `[prensa]` análisis de la época, `[manual 4.0]` para las
reglas que 6.0 heredó sin cambios. Lo marcado **inferencia** es deducción propia.

---

## 1. Identidad de la entrega

| | PC Fútbol 6.0 | PC Fútbol 7 |
|---|---|---|
| Temporada | 97/98 | 98/99 |
| Salida | Noviembre 1997 | 1998 |
| Precio | 2.995 pesetas | 2.995 pesetas |
| Resolución | 640×480 | 640×480 |
| Ventas | ~600.000 | ~360.000 |
| Ligas jugables | 1ª, 2ª y 2ªB españolas | + Francia, Inglaterra, Alemania, Italia |
| Valoración de la época | Micromanía 89, PCManía 90, Meristation 8/10 | Micromanía 79, PCManía 90 |

6.0 tuvo **eXtensión 1**, **eXtensión 2** (actualizaciones de plantillas y datos)
y una **Edición de Oro** en DVD que recopilaba toda la saga hasta la fecha.
7.0 tuvo **7.5**, que actualizó el mercado y añadió la 3ª división italiana. `[wiki]`

---

## 2. El menú principal: un tablero de destinos

Nueve mosaicos ilustrados en rejilla 3×3, cada uno con su imagen y su rótulo
debajo, más una fila inferior de iconos pequeños. `[capturas]`

```
┌──────────────────────────────────────────────────────────────────┐
│  ╔═══════════╗    ┌────────┐   ┌────────┐   ┌────────┐           │
│  ║ LOGOTIPO  ║    │  img   │   │  img   │   │  img   │           │
│  ║  GRANDE   ║    └────────┘   └────────┘   └────────┘           │
│  ║ temporada ║     HISTORIA   BASE DE DATOS  LOS GOLES           │
│  ╚═══════════╝                               DE LA LIGA          │
│                   ┌────────┐   ┌────────┐   ┌────────┐           │
│  ╔═══════════╗    │  img   │   │  img   │   │  img   │           │
│  ║  editora  ║    └────────┘   └────────┘   └────────┘           │
│  ╚═══════════╝    SEGUIMIENTO   PARTIDO      DESAFÍO             │
│                                AMISTOSO   AMÉRICA-EUROPA         │
│   eXtensión 1     ┌────────┐   ┌────────┐   ┌────────┐           │
│   eXtensión 2     │  img   │   │  img   │   │  img   │           │
│   (grisadas si    └────────┘   └────────┘   └────────┘           │
│    no instaladas) LIGA MANAGER LIGA        PROQUINIELAS 2.0      │
│                                PROMANAGER                        │
│  ┌────┐                                                          │
│  │lic.│      ⓘ          ⚽           ⬡            ↩             │
│  └────┘  Instrucciones Infofútbol  Créditos      SALIR           │
│ vN.N                                                             │
└──────────────────────────────────────────────────────────────────┘
```

Detalles que importan: `[capturas]`

- Las extensiones instaladas aparecen **listadas en la columna izquierda**; las
  no instaladas se ven en gris. El menú informa de qué contiene la instalación.
- Los productos hermanos de la casa (PC Calcio, PC Premier…) se listan también
  ahí, en gris: publicidad interna que además sitúa el producto en una familia.
- El **número de versión** vive discreto en la esquina inferior izquierda.
- **`SALIR` está abajo a la derecha**, con una flecha de retorno. La misma
  posición que tendrá en todas las demás pantallas.
- Los nueve destinos están **visibles a la vez**. No hay submenús.

> **Consecuencia para nosotros.** El menú principal no es "empezar partida"; es
> el índice de un producto con varias secciones. Ver
> [`12-mejoras-para-pc-dynasty.md`](12-mejoras-para-pc-dynasty.md) C11/C13.

---

## 3. La banda superior: el esqueleto de toda pantalla de gestión

Todas las pantallas de Liga Manager y Liga Promanager comparten la misma banda.
`[capturas]`

```
┌─────────┬──────────────────────────┬──────────┬──────────────┐
│ ▣ modo  │   NOMBRE DE LA PANTALLA  │ calendario│  contexto ▣  │
│ (2 líneas)│  versales espaciadas   │  Domingo  │  2ªB         │
│ + escudo │                         │    31     │  Jornada 1   │
│          │                         │  Agosto   │  + trofeo    │
│          │                         │   1997    │              │
└─────────┴──────────────────────────┴──────────┴──────────────┘
```

- **Izquierda**: una placa de dos líneas con el modo (`Liga / Manager`,
  `Liga / ProManager`, `Base / de datos`) y, en las pantallas de club, el escudo
  y el nombre del club gestionado.
- **Centro**: el nombre de la pantalla, en versales blancas muy espaciadas sobre
  una banda cromada. Es el elemento que más identifica a la saga.
- **Derecha**: la **placa de calendario** con día de la semana, día, mes y año en
  cuatro líneas, y a su lado una placa de contexto con la categoría y la jornada
  (con icono de trofeo cuando hay competición en juego).

En pantallas de la base de datos, la izquierda lleva el icono de sección y el
centro el nombre del protagonista consultado, con su escudo a la derecha.

---

## 4. Liga Manager: el recorrido completo

### 4.1 Introduce nombre y equipo

```
┌────────────────────────── INTRODUCE NOMBRE Y EQUIPO ─────────────┐
│      ◀  [ JUGADOR 1 ]  [ nombre tecleado________ ]  ▶            │
│  ┌───┬──────────────┐            ┌───┬──────────────┐            │
│  │ 1 │ ███ activo   │            │11 │              │            │
│  │ 2 │              │            │12 │              │            │
│  │ …ranuras 1..10…  │            │ …ranuras 11..20… │            │
│  └───┴──────────────┘            └───┴──────────────┘            │
│ ┌──────────┐  ┌────────────────────────────┐  ┌──────────┐       │
│ │1ª división│  │  ▣▣▣▣▣▣▣▣▣  rejilla de     │  │ 2ªB II   │       │
│ │2ª división│  │  ▣▣▣▣▣▣▣▣▣  escudos       │  │ 2ªB III  │       │
│ │2ªB I     │  │      Nombre del club       │  │ 2ªB IV   │       │
│ └──────────┘  └────────────────────────────┘  └──────────┘       │
│  [VOLVER]  [CARGAR LIGA]              [BORRAR]  [SEGUIR ▶]       │
└──────────────────────────────────────────────────────────────────┘
```

- Hasta **20 managers** en la misma liga, cada uno en su ranura numerada.
- Las categorías son **botones apilados a ambos lados** de la rejilla de escudos;
  elegir categoría cambia la rejilla.
- El club se elige **pinchando su escudo**, no en una lista. `[capturas]`
- Barra de mando de cuatro: volver, cargar, borrar y seguir.

### 4.2 Pretemporada

```
┌── Liga ──┬──────── PRETEMPORADA ────────────────────────┬─ ⚽ ──┐
│ Manager  │                                                       │
├──────────┴───────────────────────────────┬───────────┬───────────┤
│ ┌─ Primera división ──────────────────┐  │  ESPAÑA   │  PAÍSES   │
│ │  ▣ ▣ ▣ ▣ ▣ ▣ ▣ ▣ ▣ ▣               │  │ ┌───────┐ │ ▤▤▤▤▤▤▤   │
│ │  ▣ ▣ ▣ ▣ ▣ ▣ ▣ ▣ ▣ ▣               │  │ │PRIMERA│ │ ▤▤▤▤▤▤▤   │
│ │ [NO JUGAR] [ campo de texto      ] │  │ │SEGUNDA│ │ ▤▤▤▤▤▤▤   │
│ └─────────────────────────────────────┘  │ │2ªB I  │ │ (banderas)│
│                                          │ └───────┘ │           │
├──────────────────────────────────────────┴───────────┴───────────┤
│ ▐ RIVALES PARA │        Club A        │        Club B       │ 🏆 │
├────┬───────────┬──────────────┬────────────────┬─────────────────┤
│ 1  │Preparación│  Rival       │  Estadio       │  14-8-1996      │
│ 2  │Preparación│  Rival       │  Estadio       │  17-8-1996      │
│ 5  │Presentación│ Rival       │  Estadio       │  28-8-1996      │
│    │           │ PRECIO: 15 millones            │                 │
├──────────────────────────────────────────────────────────────────┤
│                                    [✖ BORRAR]  [● SEGUIR]        │
└──────────────────────────────────────────────────────────────────┘
```

- Se montan los **amistosos de pretemporada** eligiendo rival por escudo o por
  país (dos placas con barra de rótulo de colores distintos: una para las
  categorías del país, otra para la bandera del país).
- La tabla de amistosos lleva **columna índice roja numerada**, filas alternas y
  una fila de otro color para el **partido de presentación**, con su precio
  embebido en la propia fila.
- `NO JUGAR` permite dejar una fecha vacía.
- El partido de presentación **cuesta dinero**: la pretemporada ya es una
  decisión económica.

### 4.3 Alineación — el centro de gravedad

En 6.0/7.0 **no hay una pantalla-despacho aparte**: la alineación es el eje, y
sus destinos cuelgan de un raíl a la derecha. `[capturas]` **inferencia**

```
┌─ rival1 ─┬──────── ALINEACIÓN ────────┬ calendario ┬ Pretemp. ─┐
│  rival2  │                            │            │Preparación│
├──────────┴────────────────────────────┴────────────┴───────────┤
│ Nº JUGADOR      EN VE RE AG CA EF MO ME  ROL DEM │ [PARÁMETROS]│
│  1 Portero      99 79 88 76 78 43 94 76  ▣  POR  │[CALIFICACIÓN]│
│  4 Defensa      99 81 80 78 78 42 92 75  ▣  DEF  │ ┌──────────┐│
│ …                                                │ │MEDIA     ││
│ ══════════ JUGADORES CONVOCADOS ══════════       │ │EQUIPO    ││
│ 20 Suplente     99 80 70 73 75 43 91 72  ▣  MED  │ │ ▣ ★★★½ 75││
│ ══════════ JUGADORES NO CONVOCADOS ═══════       │ └──────────┘│
│ 12 Lesionado    ✚ │ 2 SEMANAS │ 50 90 73  ▣ DEF  │ ┌──────────┐│
│                                                  │ │ FICHA DEL││
│                                                  │ │ JUGADOR  ││
│                                                  │ │ PORTERO 20││
│                                                  │ │ PASE  ★★★││
│                                                  │ └──────────┘│
│                                                  │ ┌──────────┐│
│                                                  │ │  CAMPO   ││
│                                                  │ └──────────┘│
│                                                  │[ENTRENAMIENTO]│
│                                                  │ [LESIONADOS] │
│                                                  │[ESTADÍSTICAS]│
│                                                  │[TÁCTICA][VOLVER]│
└─────────────────────────────────────────────────────────────────┘
```

**Las columnas de atributos** son las mismas en toda la aplicación —
alineación, tácticas, ver rival, comprar jugador, estado de las ofertas:

| Abrev. | Significado |
|---|---|
| `EN` | Energía |
| `VE` | Velocidad |
| `RE` | Resistencia |
| `AG` | Agresividad |
| `CA` | Calidad |
| `EF` | Estado de forma |
| `MO` | Moral |
| `ME` | Media |
| `ROL` | Rol (icono de campo; pulsación mantenida abre la lista de roles) |
| `DEM` | Demarcación (POR/DEF/MED/DEL) |

Reglas de la pantalla: `[capturas]`

1. **Una sola tabla, dos vistas.** `PARÁMETROS` muestra los ocho números;
   `CALIFICACIÓN` los sustituye por **estrellas + la demarcación escrita**
   (`LATERAL DER.`, `MEDIA P. CENTR.`, `VOLANTE TAPÓN`, `ARIETE`) y una media
   destacada. **El botón de la vista activa se pinta de otro color** — es el
   único elemento resaltado del raíl, así que la pantalla dice en todo momento
   qué está enseñando. No abre otra pantalla ni cambia el orden de las filas.
2. **El raíl reacciona a la fila señalada.** Señalar un jugador actualiza la
   ficha lateral y el diagrama de campo. Sin navegar.
3. **La fila seleccionada se marca con un contorno**, no con un cambio de fondo:
   la alternancia de la tabla se conserva intacta debajo.
4. **Filas de sección dentro de la tabla**: «JUGADORES CONVOCADOS», «JUGADORES NO
   CONVOCADOS», centradas y con icono de balón.
5. **El estado sustituye al dato**: un lesionado muestra `2 SEMANAS` sobre fondo
   rojo en lugar de sus números de rendimiento.
6. **Cada fila lleva un asidero a la izquierda** (un icono de carpeta) para
   arrastrarla entre bloques: convocar y desconvocar es mover, no rellenar un
   formulario.
7. **Flechas de desplazamiento chunky** en el borde del bloque que se desborda,
   no una barra de scroll fina.
8. El raíl termina en la **barra de mando local**: `TÁCTICA` y `VOLVER`.

**La ficha lateral del jugador** es el modelo de nuestra `StarRating`: seis
habilidades en dos columnas, cada una con estrellas **y** su cifra al lado, bajo
el nombre del jugador señalado:

```
┌──────── Nombre ────────┐
│ PORTERO ★☆☆☆☆ 29 │ PASE     ★★★★☆ 76 │
│ REGATE  ★★★★☆ 98 │ REMATE   ★★★★☆ 79 │
│ ENTRADAS★★★☆☆ 68 │ TIRO     ★★★★☆ 77 │
└────────────────────────┘
```

Y encima, la placa `MEDIA EQUIPO`: escudo, estrellas y la media numérica. Una
cifra de conjunto siempre visible mientras se toca la alineación.

### 4.4 Destinos que cuelgan de la alineación

| Destino | Qué hace |
|---|---|
| `ENTRENAMIENTO` | Entrenamiento general o específico por línea/puesto. Sólo rinde si concuerda con la táctica planteada `[manual 4.0]` |
| `LESIONADOS` | Parte médico con semanas de convalecencia |
| `ESTADÍSTICAS` | Rendimiento acumulado de la plantilla |
| `TÁCTICA` | Campo manipulable: dorsales, círculo defensivo, flecha ofensiva, área de actuación, líneas `D`/`M` |
| `VER RIVAL` | Los dos onces enfrentados con sus parámetros y la media de cada equipo |

### 4.5 Estado de las ofertas

Una de las pantallas mejor resueltas del original. `[capturas]`

```
┌─ club ▣ ─┬──── ESTADO DE LAS OFERTAS ────┬ calendario ┬ 2ªB ────┐
│ Isla C.  │                               │  Domingo   │Jornada 1│
├──────────┴───────────────────────────────┴────────────┴─────────┤
│ ┌───────────────────────────────────────────────────────┐  ┌──┐ │
│ │ NOMBRE            EN VE RE AG CA EF MO ME ROL DEM     │  │O │ │
│ │ ▣ Jugador  ★★½    99 49 41 36 43 47 85 51  ▣  POR    │  │F │ │
│ │   Mensajero   CLÁUSULA 3 mill.   FICHA 1 mill.        │  │E │ │
│ │ ┌───────────────────────────────────────────────┐ ▲   │  │R │ │
│ │ │ (31-08-1997) Has ofrecido al jugador un       │ │   │  │T │ │
│ │ │ contrato por 4 años con una ficha anual de 1  │ ▼   │  │A │ │
│ │ └───────────────────────────────────────────────┘     │  └──┘ │
│ │ ▭▭▭▭▭▭▭▭▭  (ranura vacía, conserva la forma)          │  ┌──┐ │
│ │ ▭▭▭▭▭▭▭▭▭                                             │  │O │ │
│ │ …cinco ranuras en total…                              │  └──┘ │
│ └───────────────────────────────────────────────────────┘       │
│ ┌──────────────────────┐                                        │
│ │ ▣ SECRETARIO TÉCNICO │                            [◀ VOLVER]  │
│ │   P. Castellanos   ▸ │                                        │
│ └──────────────────────┘                                        │
└─────────────────────────────────────────────────────────────────┘
```

Cuatro ideas que merecen copiarse:

1. **El empleado que gobierna la pantalla aparece en ella.** Una placa abajo a la
   izquierda con el retrato y el nombre del secretario técnico. El personal del
   club no es una lista en un submenú: está donde trabaja.
2. **Las ranuras vacías conservan su forma.** Cinco huecos siempre; los libres se
   dibujan como barras vacías. La pantalla nunca cambia de tamaño ni «salta».
3. **Cada ranura lleva su acción en una pestaña vertical** en el borde derecho
   (`OFERTA`), rotada — la misma idea que el raíl de área, aplicada por fila.
4. **La negociación se cuenta como un diario fechado**, en prosa, dentro de un
   panelito con su propio desplazamiento: *"(31-08-1997) Has ofrecido al jugador
   un contrato por 4 años…"*. El estado del trato es una **historia**, no un
   campo.

### 4.6 Ficha de jugador (base de datos)

Doble juego de pestañas. `[capturas]`

```
┌─ ▣ BASE ─┬───── Nombre completo, APODO ─────────────┬ escudo ─┐
│ DE DATOS │                                          │         │
├──────────┴──────────────────────────────────────────┴─────────┤
│ ⌐C.TÉCNICAS╥PALMARÉS╥TRAYECTORIA╥INTERNAC.╥ANECDOTARIO╥TEMP.ANT.│
│ ┌── DATOS PERSONALES ─────────────────────────────┐ ▮▮▮▯▯▯     │
│ │ ┌────────┐  ┌─LUGAR DE NACIMIENTO─┐ ┌─FECHA──┐  │            │
│ │ │ retrato│  │ Santos (Brasil)  ▤  │ │9/12/70 │  │            │
│ │ │        │  └─────────────────────┘ └────────┘  │            │
│ │ │        │  ┌EDAD┐┌NACIONALIDAD─┐┌INTERNACIONAL┐│            │
│ │ └────────┘  │ 49 ││ BRASIL   ▤  ││  Brasil     ││            │
│ │   MEDIO     └────┘└─────────────┘└─────────────┘│            │
│ │             ┌─ PROCEDENCIA ───────────────────┐ │            │
│ │             │ Palmeiras, BRA (97)             │ │            │
│ │             └─────────────────────────────────┘ │            │
│ │             ┌─ ALTURA ──────┬─ PESO ──────────┐ │            │
│ │             │ 1,78 m.       │ 69 kg.          │ │            │
│ └─────────────────────────────────────────────────┘            │
│ ⌐DATOS╥NOTAS╥SEGUIMIENTO                                       │
│                        [NOTAS]  [▤ IMPRIMIR]  [◀ VOLVER]       │
└────────────────────────────────────────────────────────────────┘
```

- **Pestañas arriba** = faceta del dossier (técnicas, palmarés, trayectoria,
  internacionalidades, anecdotario, temporada anterior).
- **Pestañas abajo** = modo de trabajo (datos, notas propias, seguimiento).
- Los pares etiqueta/valor son **fichas de color**: cada bloque semántico tiene
  su tono (identidad, procedencia, físico).
- Un **indicador segmentado** arriba a la derecha de la barra de rótulo dice
  cuántas fichas hay y en cuál estás.
- La barra de mando de la pantalla, abajo a la derecha: `NOTAS`, `IMPRIMIR`,
  `VOLVER`.

> Que exista un **ANECDOTARIO** en la ficha de un jugador dice mucho del
> producto: parte del valor era literario, no numérico.

### 4.7 Comparativa histórica

```
┌─ ▣ BASE ─┬──── LAS ÚLTIMAS 10 LIGAS ────────────────────────────┐
│ DE DATOS │                                                      │
├──────────┴───────────┬──────────────────────────┬───────────────┤
│ ┌─────────────────┐  │      gráfica de          │ Club A     ▣ │
│ │ 🏆 1ª DIVISIÓN  │  │      posiciones          │ [ COMPARA ]  │
│ ├─────────────────┤  │      por temporada,      │ Club B     ▣ │
│ │ 🌿 2ª DIVISIÓN  │  │      una serie de color   │ [ COMPARA ]  │
│ ├─────────────────┤  │      por club, con el     │ Club C     ▣ │
│ │ 🌿 2ª B         │  │      puesto etiquetado    │ [ COMPARA ]  │
│ ├─────────────────┤  │      en cada punto        │ Club D     ▣ │
│ │ 🌿 3ª DIVISIÓN  │  │                           │ [ COMPARA ]  │
│ └─────────────────┘  │ 87-88 … 96-97 (eje X)     │ [IMPRIMIR]   │
│                      │                           │ [◀ VOLVER]   │
└──────────────────────┴──────────────────────────┴───────────────┘
```

- **Cuatro clubes como máximo**, cada uno con su color y su botón `COMPARA`
  propio en un raíl derecho, con el escudo al lado.
- El raíl izquierdo elige la categoría, con icono distinto por división.
- Cada punto de la gráfica lleva **el puesto escrito**: la forma dice la
  tendencia, el número dice el dato exacto. Doble codificación.

### 4.8 Ficha de club (base de datos)

```
┌─ ▣ BASE ─┬───── Nombre completo del club ──────┬──── escudo ────┐
│ DE DATOS │                                     │   (grande,     │
│          │                                     │  sobresale)    │
├──────────┴─────────────────────────────────────┴────────────────┤
│ ┌─────────────────┬──────────────────────────┐                  │
│ │                 │ ┌─ ESTADIO ────────────┐ │                  │
│ │  foto aérea     │ │ Nombre del estadio   │ │                  │
│ │  del estadio    │ ├─ AFORO ──────────────┤ │                  │
│ │                 │ │ 104.779 espectadores │ │                  │
│ │                 │ ├─ FUNDACIÓN ──┬ DIMEN.┤ │                  │
│ └─────────────────┴─┴──────────────┴───────┴─┘                  │
│ ┌───────────────────────────────────────────┐   ┌─────────────┐ │
│ │ NOMBRE                                    │   │  👕         │ │
│ │ ┌─ PRESIDENTE ─────┬─ FUNDACIÓN ────────┐ │   │ [PLANTILLA] │ │
│ │ ├─ PRESUPUESTO ────┼─ SOCIOS ───────────┤ │   │  🏆         │ │
│ │ ├─ PATROCINADOR ───────────────────────┤ │   │ [PALMARÉS]  │ │
│ │ ├─ EQUIPADOR ──────────────────────────┤ │   │             │ │
│ └───────────────────────────────────────────┘   │ [IMPRIMIR]  │ │
│                                                 │ [◀ VOLVER]  │ │
└─────────────────────────────────────────────────┴─────────────┘
```

- **El color agrupa por familia semántica**: identidad y estadio en un tono,
  personas y dinero en otro, acuerdos comerciales en un tercero. No hay etiquetas
  de grupo: el color hace ese trabajo.
- El **escudo grande sobresale de la banda superior**, como un membrete.
- Raíl derecho con **icono sobre el botón** (camiseta → plantilla, trofeo →
  palmarés), y debajo la barra de mando de la pantalla.

### 4.9 Salida al campo

```
┌ ▣ ─ CLUB LOCAL ────────────────── CLUB VISITANTE ─ ▣ ┐
│      entrenador                     entrenador       │
│ ┌──┐ 17 Jugador       ┌──────────────┐  Jugador 1 ┌──┐│
│ │▣ │ 14 Jugador       │              │  Jugador 2 │▣ ││
│ │▣ │ 21 Jugador       │  foto aérea  │  Jugador 3 │▣ ││
│ │▣ │  5 Jugador       │  del estadio │  Jugador 4 │▣ ││
│ │▣ │ …once completo…  │              │  …once…    │▣ ││
│ └──┘                  └──────────────┘            └──┘│
│                        Nombre del estadio             │
└───────────────────────────────────────────────────────┘
```

Retrato por jugador a ambos lados, dorsal, y el estadio en el centro. Es una
pantalla **ceremonial**: no se decide nada en ella, y por eso puede permitirse
respirar. Compárese con [`03`](03-mapa-de-pantallas.md) §R.

---

## 5. Liga Promanager en 6.0/7.0

Mismo motor, distinta entrada:

- **Selección de ofertas** con nombre y contraseña (ver [`02`](02-modos-de-juego.md)).
- **Pretemporada sobre un mapa**: en vez de rejilla de escudos, un **mapa de
  Europa con banderas pinchables**, raíles verticales `EUROPA` / `SUDAMÉRICA` en
  el borde izquierdo, y la lista de rivales a la derecha en **tarjetas numeradas
  con pestaña roja**. Debajo del mapa, un panel con el país seleccionado y sus
  clubes. `[capturas]`
- La misma barra de mando: `NO JUGAR`, `BORRAR`, `SEGUIR`.

> **Idea a robar**: cuando el dominio es geográfico, la interfaz es un mapa.
> Nuestro mundo es ficticio, así que el mapa es **nuestro** y no plantea ningún
> problema de licencia.

---

## 6. Qué añade PC Fútbol 7 sobre 6.0

| Novedad | Detalle `[wiki]` |
|---|---|
| **Euro PC Fútbol** | Liga Manager con España, Francia, Inglaterra, Alemania e Italia en el mismo paquete |
| **Desafío Mundial** | Torneo configurable con cualquier club, sin despidos ni fichajes |
| **Reglas de destitución explícitas** | Despido por 3+ semanas consecutivas en déficit **o** por bajar de 16 jugadores en plantilla |
| **7.5** | Actualización de mercado y 3ª división italiana |

Lo que **no** cambió: la gramática de pantalla. 6.0 y 7 se navegan igual. Por eso
esta wiki los trata como una sola versión objetivo.

---

## 7. Lista de verificación de la versión objetivo

Una pantalla nuestra "es 6.0" cuando:

- [ ] La banda superior lleva modo · **título centrado en versales anchas** ·
      placa de calendario · contexto de competición.
- [ ] El contenido vive en placas con **barra de rótulo de color**.
- [ ] Las tablas usan las **abreviaturas de 2–3 letras** y una fila de cabecera
      de otro color.
- [ ] Los destinos cuelgan de un **raíl lateral permanente**, no de un menú.
- [ ] La **barra de mando está abajo**, con la salida a la derecha.
- [ ] Los estados especiales **sustituyen al dato** dentro de la propia fila.
- [ ] Las listas de ranuras **conservan su forma cuando están vacías**.
- [ ] Si hay un empleado responsable de la pantalla, **aparece en ella**.
- [ ] Todo dato agregado **abre su desglose** al pincharlo.
- [ ] Nada de esto usa un solo activo ajeno.
