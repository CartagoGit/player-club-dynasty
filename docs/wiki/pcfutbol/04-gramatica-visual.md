# 04 · Gramática visual — **documento normativo**

Este fichero define el lenguaje visual de PC Dynasty. **Una pantalla que lo
contradiga es un defecto**, no una variante de estilo. Describe *forma*, no
*material*: los colores, proporciones y nombres son nuestros; lo que se toma del
clásico es la manera de organizar la superficie.

> **La referencia es PC Fútbol 6.0/7.0.** Cuando este documento y una captura de
> otra entrega discrepen, manda 6.0. Ver
> [`00-objetivo-y-alcance.md`](00-objetivo-y-alcance.md) y la descripción
> pantalla a pantalla en [`13`](13-version-objetivo-6-0-y-7-0.md).

Origen: análisis de capturas del clásico + [`03-mapa-de-pantallas.md`](03-mapa-de-pantallas.md).
Todo lo marcado **inferencia** es deducción nuestra a partir de esas capturas.

---

## 1. Los ocho principios

1. **Todo vive dentro de una placa.** No existe contenido flotando sobre el fondo.
   Cualquier grupo de información está dentro de un rectángulo con marco y, salvo
   excepción justificada, con **barra de rótulo** arriba.
2. **La barra de rótulo nombra y colorea.** Cada placa se presenta con una franja de
   color sólido con su título en versales espaciadas. El color dice a qué área
   pertenece.
3. **Bisel, no sombra.** El relieve se consigue con un borde claro arriba/izquierda y
   uno oscuro abajo/derecha, de 1–2 px. **Nunca** desenfoques, degradados suaves,
   sombras difusas ni esquinas redondeadas grandes.
4. **Densidad primero.** Una tabla debe caber entera. Antes de recortar filas se
   recorta el espaciado. El objetivo es ver la plantilla completa sin desplazar.
5. **Cada número es una puerta.** Cualquier celda con un dato agregado abre el
   desglose que lo produce.
6. **El sitio de una acción no cambia nunca.** `SEGUIR` abajo a la derecha,
   `VOLVER`/`CANCELAR` a su izquierda, destructivo separado y marcado en rojo.
7. **Doble codificación.** El color nunca es el único portador de significado: va con
   texto, cifra, icono o forma (estrellas, flechas de tendencia).
8. **Nada de esperas.** Cambiar de área no puede iniciar trabajo pesado. Ver
   [`05-interaccion-y-controles.md`](05-interaccion-y-controles.md) §Latencia.

---

## 2. Estructura fija de pantalla

```
╔══════════════════════════════════════════════════════════════╗
║ BANDA DE CONTEXTO   club │ calendario │ rival/estado         ║  48–64 px
╠═══╤══════════════════════════════════════════════════════╤═══╣
║ R │  LIENZO                                              │ R ║
║ A │   ┌─ barra de rótulo ─────────────────┐              │ A ║
║ Í │   │  contenido de la placa            │              │ Í ║
║ L │   └───────────────────────────────────┘              │ L ║
╠═══╧══════════════════════════════════════════════════════╧═══╣
║ TIRA DE AVISOS   último mensaje del club                     ║  24 px
╠══════════════════════════════════════════════════════════════╣
║ BARRA DE MANDO   [SALIR] [GRABAR] [NOTICIAS]      [SEGUIR ▶] ║  44–52 px
╚══════════════════════════════════════════════════════════════╝
```

- **Banda de contexto**: siempre visible en carrera. Escudo + club a la izquierda,
  **placa de calendario** centrada (día de la semana, día, mes, año en cuatro líneas
  compactas), contexto a la derecha (rival, fase, tesorería).
- **Raíles**: los márgenes laterales llevan el nombre del área **rotado 90°**. No son
  navegación clicable obligatoria; son señalización. La navegación puede vivir en
  ellos o en el lienzo, pero el rótulo rotado es lo que ancla al jugador.
- **Tira de avisos**: una línea, siempre presente aunque esté vacía. Nunca se colapsa
  (si desaparece, el layout salta).
- **Barra de mando**: 2 a 4 acciones. La de avance a la derecha del todo.

**Prohibido**: paneles con posición absoluta calculada para una resolución concreta.
Toda banda se ancla; todo lienzo escala. Un panel que se sale de la ventana al
redimensionar es un defecto de máxima prioridad. **inferencia**

---

## 3. Componentes

### 3.1 Placa (`Plate`)

El contenedor universal.

| Propiedad | Valor |
|---|---|
| Fondo | superficie |
| Borde | 1 px marco + bisel: claro arriba/izq., oscuro abajo/der. |
| Radio de esquina | **0 px** (2 px máximo en placas de diálogo) |
| Relleno | 8 px vertical, 12 px horizontal |
| Barra de rótulo | opcional pero recomendada |

### 3.2 Barra de rótulo (`CaptionBar`)

- Altura 22–26 px, texto en **versales**, `letter-spacing` amplio, 11–13 px.
- Fondo: color del área. Texto: máximo contraste sobre ese color.
- Alineación: izquierda por defecto; centrada cuando la placa es simétrica
  (escudos, celebración).
- Puede llevar un valor a la derecha (contador, total) — mismo estilo, alineado a la
  derecha.

### 3.3 Tabla densa (`DataTable`)

El componente más importante del juego.

| Elemento | Regla |
|---|---|
| Altura de fila | 22 px densa / 28 px cómoda (preferencia del jugador) |
| Fila de cabecera | fondo propio, versales, **un color por familia de columna** |
| Alternancia | filas pares e impares con tinte distinto, diferencia sutil |
| Columna índice | franja de color a la izquierda con el número de fila |
| Alineación | texto a la izquierda; **todo número a la derecha**, cifras tabulares |
| Estado de fila | el estado sustituye al dato: un lesionado muestra `2 SEM` sobre rojo en sus columnas de rendimiento |
| Filas de sección | fila a todo el ancho, centrada, con icono: «CONVOCADOS», «NO CONVOCADOS» |
| Selección | fila completa con borde de acento; **actualiza los paneles laterales**, no navega |
| Celda pinchable | abre ventana de desglose (§3.6) |
| Cabecera fija | al desplazar, la fila de cabecera no se va |

**Regla de columnas**: los atributos se abrevian a 2–3 letras (`VE` `RE` `AG` `CA`
`EF` `MO` `ME`) y el significado completo vive en el tooltip. La abreviatura es lo que
permite la densidad.

### 3.4 Botón de mando (`CommandButton`)

- Placa con bisel, icono a la izquierda, rótulo en versales.
- Altura mínima 40 px (objetivo táctil), ancho generoso.
- Cuatro estados con **forma además de color**: normal, señalado (bisel invertido +
  aclarado), pulsado (bisel hundido), inhabilitado (sin bisel, texto apagado).
- Semántica de color: avance = positivo, retroceso = neutro, destructivo = negativo.

### 3.5 Botón de área (`AreaButton`)

El de la rejilla del despacho. Ancho completo del grupo, rótulo en versales a la
izquierda, icono opcional a la derecha, fondo del color del área.

### 3.6 Ventana de desglose (`BreakdownWindow`)

Lo que abre una celda pinchable.

- Placa flotante con marco doble, título en barra de rótulo, contenido tabular.
- **Un solo nivel**: una ventana de desglose no abre otra.
- Se cierra con `Esc`, con clic fuera y con un botón explícito.
- Aparece **junto al dato**, no centrada en pantalla, para no perder el contexto.

### 3.7 Diálogo modal (`Dialog`)

Para negociar: marco grueso, barra de rótulo, contenido en dos columnas, y **barra de
mando propia** con 2–3 salidas explícitas siempre visibles.

### 3.8 Selector numérico (`Stepper`)

`◀ [ valor ] ▶`. Flechas grandes a los lados del valor. Mantener pulsado acelera.
Para dinero, años de contrato, aforo, precios. **Nunca** un campo de texto libre
donde un `spinner` sirve.

### 3.9 Valoración en estrellas (`StarRating`)

Estrellas + cifra al lado. Doble codificación obligatoria. Para habilidades
concretas, no para medias globales (ésas son un número grande).

### 3.10 Diagrama de campo (`PitchDiagram`)

Superficie de manipulación directa: dorsales colocables, círculo = posición
defensiva, flecha = posición ofensiva, marco arrastrable = área de actuación, líneas
`D`/`M` arrastrables. Se usa **siempre** que el dato sea espacial.

### 3.11 Placa de calendario (`DatePlate`)

Bloque compacto de 3–4 líneas: mes, día grande, día de la semana, año. Va centrada en
la banda de contexto. Es el ancla temporal permanente.

### 3.12 Rejilla de escudos (`CrestGrid`)

Escudos en rejilla, dentro de una placa con barra de rótulo que nombra la categoría,
y el nombre del seleccionado bajo la rejilla. Preferida frente a una lista desplegable
siempre que haya identidad visual disponible.

### 3.13 Raíl rotado (`EdgeRail`)

Franja de 24–32 px en el borde, texto rotado 90°, versales, fondo del color del área.

### 3.14 Mosaico de destino (`DestinationTile`)

Tarjeta con ilustración y rótulo debajo, para el menú principal. Todos los destinos
principales visibles a la vez.

### 3.15 Tira de pestañas (`TabStrip`)

Pestañas trapezoidales sobre el borde de una placa. El dossier del clásico usaba
**dos tiras a la vez**: arriba la faceta del contenido (técnicas, palmarés,
trayectoria…), abajo el modo de trabajo (datos, notas, seguimiento). Máximo seis
pestañas por tira; más de seis significa que faltaba una pantalla.

### 3.16 Pestaña de fila (`RowTab`)

La acción de una fila cuando la fila es alta: una pestaña vertical con el rótulo
rotado, pegada al borde derecho de la lista. Se usa cuando la acción es la misma
para todas las filas y necesita nombre (`OFERTA`), en vez de repetir un botón.

### 3.17 Conmutador de vista (`ViewSwitch`)

Dos o tres botones que cambian **qué columnas muestra la misma tabla**, no qué
pantalla se ve. El botón de la vista activa se pinta con el acento; es el único
elemento resaltado de su raíl, para que la pantalla diga siempre qué enseña.

### 3.18 Placa de empleado (`StaffPlate`)

Retrato, cargo y nombre del miembro del cuerpo técnico responsable de la
pantalla, abajo a la izquierda. No es decoración: identifica a quién compete lo
que se está haciendo, y es el gancho natural para "mejora a este empleado".

### 3.19 Lista de ranuras (`SlotList`)

Una lista de tamaño fijo cuyos huecos vacíos **conservan su forma**. Cinco
ranuras de negociación son siempre cinco cajas; las libres se dibujan vacías. La
pantalla nunca cambia de altura y el jugador ve de un vistazo cuánto le queda.

### 3.20 Indicador segmentado (`SegmentIndicator`)

Barritas en el extremo de una barra de rótulo que dicen cuántos elementos hay y
en cuál estás. Más barato y más legible que "3 de 6" cuando el total es pequeño.

### 3.21 Par de campo (`FieldPair`) — **normativo**

El patrón de dato más repetido de toda la saga, y el que más se echa en falta al
comparar nuestras pantallas con las suyas. **Un dato nunca se escribe como
`Etiqueta: valor` en una línea.** Son dos barras apiladas:

```
┌────────────────────────────────┐
│ ESTADIO                        │  barra clara, etiqueta en mayúsculas oscuras
├────────────────────────────────┤
│ Santiago Bernabéu              │  barra saturada, valor en blanco
└────────────────────────────────┘
```

Reglas:

- La barra de valor va **saturada**, y su color **agrupa por familia de dato**
  (los datos del estadio comparten color; los del club, otro; el nombre, el
  patrocinador y el equipador, otro). El color clasifica, no decora.
- Los pares se disponen en **rejilla de dos columnas**; un valor largo ocupa el
  ancho completo en lugar de encogerse.
- La etiqueta va siempre en mayúsculas y el valor en caja normal: la forma de la
  palabra distingue metadato de dato antes de leerlos.

Fuente: ficha de club de 6.0, leída en [`16` §6](16-lectura-de-capturas.md).

### 3.22 Celda de mapa de calor (`HeatCell`) — **normativo**

En las tablas de atributos, **cada celda va teñida por su propio valor** (verde
alto → rojo bajo) y **la fila entera va teñida por demarcación**. Las dos capas
conviven, y ninguna sustituye al texto: la columna `DEM` sigue diciendo `POR`,
`DEF`, `MED`, `DEL`.

El principio es el de [`§1`](#1-los-ocho-principios): el color **duplica** el
dato para poder leer la tabla de un vistazo, y nunca lo reemplaza — quien no
distinga los tintes sigue teniendo el número y la palabra.

Fuente: alineación de 6.0 y ver rival de 5.0, leídas en
[`16` §4 y §5](16-lectura-de-capturas.md).

---

## 4. Color

El clásico usaba azul marino profundo como sustrato y saturaba los acentos. Nuestra
paleta es propia, pero adopta la **estructura**:

| Rol | Uso |
|---|---|
| Sustrato | fondo general, casi negro azulado |
| Cromo | bandas de contexto y de mando |
| Superficie | interior de placas |
| Superficie realzada | fila alterna, selección, panel de detalle |
| Marco | borde de placa |
| Bisel claro / Bisel oscuro | relieve |
| **Color de área** × 4–6 | seguimiento, técnico, mercado, finanzas, mundo, ajustes |
| Acento | acción principal y estado activo |
| Positivo / Aviso / Negativo | resultado, riesgo, error |
| Texto primario / secundario | dato y metadato |

**Reglas**:
- Contraste mínimo 4.5:1 en texto normal.
- Un color de área no se usa nunca para significar estado (positivo/negativo) ni al
  revés.
- Las paletas alternativas (claro/oscuro/alto contraste) **sólo** redefinen estos
  roles; ninguna pantalla codifica un color literal.
- **El color nunca es el único portador de un dato.** Donde hay tinte hay también
  texto: la demarcación se tiñe *y* se escribe, el atributo se tiñe *y* se
  numera.

> **Por qué la paleta puede ser nuestra.** Al comparar 5.0 con 6.0 se ve que los
> colores concretos de las cuatro áreas **cambiaron** de una versión a la
> siguiente mientras la agrupación y el reparto de destinos se mantuvieron
> intactos ([`16` §3](16-lectura-de-capturas.md)). El invariante de la gramática
> es «cuatro áreas separadas por color, y el color se repite dentro de las
> pantallas que abre», no una paleta concreta. La variante argentina lo confirma
> desde el otro extremo: el mismo esqueleto sobrevive a un cromo gris claro
> ([`15`](15-variantes-argentina-y-selecciones.md)).

---

## 5. Tipografía y ritmo

- Escala: 11 / 12 / 13 / 15 / 18 / 22 / 30 px.
  - 11–12: rótulos de barra, abreviaturas de columna, tira de avisos.
  - 13: cuerpo de tabla.
  - 15: cuerpo de texto.
  - 18–22: título de placa.
  - 30: título de pantalla en la banda de contexto.
- **Versales para todo rótulo estructural** (barras de rótulo, botones, cabeceras de
  columna, raíles). Nunca versales para texto corrido.
- Cifras tabulares alineadas a la derecha, obligatorio.
- Rejilla de espaciado: 2, 4, 8, 12, 16, 24, 32 px. Dentro de tablas sólo 2, 4 y 8.

---

## 6. Lista de comprobación antes de dar por buena una pantalla

- [ ] ¿Todo el contenido está dentro de una placa con barra de rótulo?
- [ ] ¿Las esquinas son rectas y el relieve es bisel, no sombra?
- [ ] ¿La pantalla se ancla y escala, sin `offset` absolutos?
- [ ] ¿Nada se sale de su contenedor a 1024×600 ni a 2560×1440?
- [ ] ¿La tabla cabe entera o, si no, tiene cabecera fija?
- [ ] ¿Los números están a la derecha con cifras tabulares?
- [ ] ¿Las celdas agregadas abren su desglose?
- [ ] ¿`SEGUIR` está abajo a la derecha y es el único que avanza el tiempo?
- [ ] ¿Hay siempre una salida (`VOLVER` / `CANCELAR`) visible?
- [ ] ¿Cada color va acompañado de texto, icono o forma?
- [ ] ¿Todos los rótulos salen del fichero de idioma, ninguno incrustado?
- [ ] ¿Ningún control interactivo tiene hijos que le roben el clic? (ver
      [`05`](05-interaccion-y-controles.md) §"Trampas de implementación")
- [ ] ¿Abrir el área no dispara consultas ni cargas?
- [ ] ¿El estado especial de una fila **sustituye** a su dato en vez de añadir una
      columna?
- [ ] ¿Las listas de tamaño fijo conservan su forma cuando están vacías?
- [ ] ¿La vista activa de una tabla está resaltada en su conmutador?
