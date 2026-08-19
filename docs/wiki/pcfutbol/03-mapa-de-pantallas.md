# 03 · Mapa de pantallas

Inventario de pantallas con su **estructura**, no su decoración. Cada ficha describe
zonas, no píxeles, para que se pueda reimplementar con activos propios.

Notación de zonas usada en toda la wiki:

```
┌─ BANDA SUPERIOR ─────────────────────────────┐  contexto permanente
│ RAÍL │        LIENZO                  │ RAÍL │  áreas ↔ contenido
│      │  (placas con barra de rótulo)  │      │
├─ TIRA DE AVISOS ─────────────────────────────┤  último mensaje
└─ BARRA DE MANDO ─────────────────────────────┘  2–4 acciones fijas
```

---

## A · Menú principal del programa

**Función**: elegir destino dentro de la suite, no empezar la partida.

- Fondo a sangre con la identidad del producto (logotipo grande a la izquierda).
- **Rejilla de mosaicos** 3×3: cada modo es una tarjeta con ilustración propia y
  rótulo debajo en versales (Historia, Base de datos, Seguimiento, Partido amistoso,
  Liga Manager, Liga Promanager…).
- **Fila inferior de iconos pequeños** para lo auxiliar: Instrucciones, Infofútbol,
  Créditos, **Salir**.
- El icono de música se conecta/desconecta desde aquí. `[wiki]` `[manual]`

**Regla que impone**: los destinos principales se **ven todos a la vez**; lo
secundario baja de tamaño, no se esconde en un submenú.

---

## B · Selección de partida / manager

**Función**: identificar quién juega y con qué club.

- Barra de título centrada: «INTRODUCE NOMBRE Y EQUIPO».
- **Tabla de ranuras numeradas** (1–20) en dos columnas, con la fila activa
  resaltada en color cálido; campo de texto para el nombre encima, con flechas
  ◀ ▶ para pasar de jugador.
- **Columnas laterales de división** a izquierda y derecha (Primera, Segunda, 2ªB I…
  2ªB IV) como botones apilados.
- **Panel central de escudos** con barra de rótulo («Primera división») y el nombre
  del club seleccionado debajo.
- Barra de mando inferior: `VOLVER` · `CARGAR LIGA` · `BORRAR` · `SEGUIR`.

En Pro-Manager, esta pantalla es de **ofertas**: nombre + contraseña arriba y la
lista de clubes que te quieren abajo, con una flecha por oferta que despliega datos
del club. `[manual 4.0]`

---

## C · Pretemporada

**Función**: elegir competiciones a disputar y montar el calendario de amistosos.

- Banda superior: pestaña de modo arriba a la izquierda («Liga Manager»), título
  centrado «PRETEMPORADA».
- Placa con barra de rótulo «Primera división» conteniendo la **rejilla de escudos**.
- Dos placas contiguas con barras de rótulo de **colores distintos**: una para las
  categorías del país (lista de botones: PRIMERA, SEGUNDA, 2ªB I…) y otra para
  **PAÍSES** (rejilla de banderas). El color separa dos ejes de la misma búsqueda.
- Barra ancha de selección: `RIVALES PARA` + equipo A + equipo B.
- **Tabla densa** de amistosos: `# | PARTIDO | RIVAL | ESTADIO | FECHA`, con columna
  índice de color, filas alternas y una fila resaltada en verde para el partido de
  presentación, más una etiqueta de precio integrada en la propia fila.
- Barra de mando: `BORRAR` (icono rojo) · `SEGUIR` (icono verde).

La variante Pro-Manager sustituye la rejilla de escudos por un **mapa** con banderas
sobre el que se pincha, con raíles verticales `EUROPA` / `SUDAMÉRICA` y la lista de
rivales a la derecha en tarjetas numeradas.

---

## D · Despacho del club (la pantalla central)

**Función**: es el *hub*. Todo lo que un manager hace en una jornada sale de aquí.

- **Banda superior**: club propio a la izquierda, **placa de calendario** centrada
  (mes, día del mes, día de la semana, año), rival o dato de contexto a la derecha.
- **Cuatro grupos de tres botones anchos**, cada grupo con su color:
  - `SEGUIMIENTO` — Resultados · Posiciones · Fixture/Calendario
  - `D. TÉCNICO` — Formación/Alineación · Tácticas · Ver contrario
  - `MERCADO` — Fichar · Empleados · Plantel/Plantilla
  - `FINANZAS` — Caja · Decisiones · Estadio
- **Raíles verticales** en los márgenes izquierdo y derecho con el nombre del área
  rotado 90°, de modo que el grupo de botones y su rótulo se leen como una unidad.
- Columna lateral de **retratos del cuerpo técnico** (empleados del club).
- **Tira de avisos** sobre la barra inferior con la última noticia.
- **Barra de mando**: `SALIR` · `GRABAR` · `NOTICIAS` · `SEGUIR`.

**Reglas que impone** (las tres más importantes de toda la wiki):

1. **Doce destinos visibles simultáneamente**, agrupados en cuatro áreas de tres. No
   hay menú desplegable, no hay pestañas ocultas: la jornada entera cabe en una
   pantalla.
2. **`SEGUIR` siempre abajo a la derecha** y siempre significa lo mismo: avanzar el
   tiempo. Es el único botón que consume días.
3. **El color codifica el área**, y ese mismo color reaparece dentro de cada pantalla
   de esa área. El jugador sabe dónde está sin leer el título.

---

## E · Alineación / Formación

**Función**: elegir once, suplentes y ver el estado de la plantilla.

- Banda superior: dos placas apiladas a la izquierda con los rivales; título
  centrado «ALINEACIÓN»; placa de calendario; a la derecha, estado de la fase
  (`Pretemp.` / `Preparación`).
- **Tabla maestra** ocupando ~⅔ del ancho:
  `Nº | JUGADOR | EN | VE | RE | AG | CA | EF | MO | ME | ROL | DEM`
  — dorsal, nombre, energía, velocidad, resistencia, agresividad, calidad, estado de
  forma, moral, media, rol (icono) y demarcación.
  - Cabeceras de columna en versales con **un color por familia de atributo**.
  - **Filas de sección** dentro de la misma tabla: «JUGADORES CONVOCADOS»,
    «JUGADORES NO CONVOCADOS», centradas y con icono.
  - Estado especial embebido en la fila: un lesionado muestra `2 SEMANAS` sobre
    fondo rojo en lugar de sus números.
  - Un icono a la izquierda de cada fila permite arrastrar/asignar.
- **Raíl derecho** apilado, de arriba abajo:
  1. Botones de modo: `PARÁMETROS` / `CALIFICACIÓN` (cambian qué columnas muestra la
     tabla — **la tabla es una sola, las vistas son varias**).
  2. Placa `MEDIA EQUIPO` con escudo y valoración en estrellas.
  3. **Ficha del jugador señalado**: nombre + atributos en pares
     etiqueta/valor con estrellas.
  4. **Diagrama de campo** con las posiciones marcadas.
  5. Botones de destino: `ENTRENAMIENTO` · `LESIONADOS` · `ESTADÍSTICAS`.
  6. Barra de mando local: `TÁCTICA` · `VOLVER`.

**Regla que impone**: la tabla manda y el raíl **reacciona**. Señalar una fila
actualiza la ficha lateral sin abrir una pantalla nueva. Ningún dato importante
exige navegar.

---

## F · Tácticas / Emparejamientos

- **Campo de fútbol** como superficie de manipulación directa: cada jugador es su
  **dorsal**; se coloca su posición de defensa (círculo) y de ataque (flecha), y se
  define su **área de actuación** arrastrando cursores blancos en las esquinas.
- Tres líneas móviles `D` y `M` delimitan las áreas de marcaje de defensas, medios y
  delanteros: se pinchan y se arrastran a izquierda/derecha. `[manual 4.0]`
- Seis esquemas predefinidos (3-4-3, 4-4-2…), la táctica real por defecto de cada
  equipo, y **guardar/cargar** táctica propia.
- Asignar rol: pinchar el icono de rol **sin soltar** abre una lista de los 18 roles;
  el que esté señalado al soltar queda asignado. El rol inicial permanece
  sobreimpreso en amarillo para poder revertir.
- **Emparejamientos**: se selecciona un jugador propio y luego el rival al que marca.

**Regla que impone**: cuando el dato es espacial, el control es **espacial**. Nada de
listas desplegables para colocar a un lateral.

---

## G · Ver rival

Alineación propia y la del próximo rival lado a lado, con las características
técnicas de cada jugador y **la media de cada equipo**. En Pro-Manager añade moral y
estado de forma. `[manual 4.0]`

**Regla que impone**: comparar es una pantalla, no una gimnasia mental. Dos columnas
simétricas + una cifra resumen por lado.

---

## H · Ficha de jugador

- Barra superior con el nombre sobre placa redondeada.
- **Bloques de pares etiqueta/valor codificados por color**: identidad (EDAD, PESO,
  ALTURA), procedencia (NACIONALIDAD, PLAZA), ROL, ESTADO (lesionado/no), SEGURO.
- **Bloque de atributos numéricos** (velocidad, resistencia, agresividad, calidad,
  estado de forma, moral) con una **MEDIA** grande destacada.
- **Habilidades en estrellas** con su número al lado: portero, pase, regate, remate,
  entradas, tiro. Doble codificación: forma + cifra.
- Panel `CONTRATO` a la derecha (o «INFORMACIÓN NO DISPONIBLE» si no se tiene).
- Ventana de texto biográfico y **cuadro estadístico jornada a jornada**: alta/baja,
  minutos, goles, lesiones, amarillas y rojas. `[manual 4.0]`
- **Demarcación**: campo de fútbol con la posición habitual y flechas de zonas
  preferentes.
- **Notas**: el usuario podía escribir sus propias observaciones sobre el jugador,
  archivarlas, consultarlas, modificarlas e imprimirlas. `[manual 4.0]`

**Regla que impone**: una ficha es un **póster**, no un formulario. Se lee de un
vistazo por bloques de color y jerarquía de tamaño.

---

## I · Mercado · Comprar jugador

- Lista de **transferibles** con características, demarcación y **precio**.
- **Dinero disponible** siempre a la vista, en el lado opuesto a la lista.
- Pagar la **cláusula de rescisión** íntegra ⇒ fichaje inmediato y seguro. Ofertar
  menos ⇒ el club dueño se lo piensa y puede rechazar, perdiendo el turno.
- Icono `OFERTAS` ⇒ pantalla con **todos los escudos**; pinchar uno abre su plantilla
  y permite tentar a cualquier estrella (Pro-Manager). `[manual 4.0]`

### Diálogo «HACER OFERTA»

Ventana modal con marco, dividida en dos mitades:

- **OFERTA AL EQUIPO**: dinero disponible, cantidad (control numérico con flechas
  ◀ ▶).
- **OFERTA AL JUGADOR**: cláusula de rescisión, ficha anual, años de contrato,
  incorporación (inmediata / a final de temporada).
- **CLÁUSULAS**: casillas — libertad por descenso, partidos para renovación, primas
  por gol, casa y coche.
- Barra de mando del diálogo: `PEDIR CESIÓN` · `CANCELAR` · `HACER OFERTA`.

**Regla que impone**: negociar es **una** pantalla con todos los términos visibles y
tres salidas explícitas. No hay asistente por pasos.

---

## J · Mercado · Vender jugador

Cualquiera de la plantilla puede declararse transferible. Cada uno tiene su cláusula,
pero puedes pedir más o menos: **más barato, más probable, menos rentable**. Si hay
ofertas, el juego informa de la mayor y puedes aceptarla o rechazarla; si la
rechazas, mantienes al jugador en venta o lo sustituyes por otro. Pinchar su posición
en la lista lo retira del mercado. `[manual 4.0]`

---

## K · Caja

- **Izquierda: ingresos** desglosados en orden descendente — taquilla, publicidad
  estática, retransmisión por TV (por competición; en Copa de Europa, por punto
  conseguido) y traspasos. Suma total al final.
- **Derecha: gastos** — generales fijos semanales (empleados, fichas de jugadores,
  desplazamientos), créditos (devolución + 12 % de intereses), primas y compras. Suma
  total al final.
- **Balance**: caja + crédito solicitado.
- En Pro-Manager añade **selector de semana** para revisar cualquier jornada pasada, y
  las **reformas del estadio** entre los pagos. `[manual 4.0]`

**Regla que impone**: dos columnas espejo (entra / sale) y **una** cifra de cierre.
El desglose está siempre expandido, no colapsado.

---

## L · Decisiones económicas

- **Precios**: entradas y vallas publicitarias. El rival, su posición y la tuya
  determinan el interés; a veces se recauda más con más público y menor precio.
- **Crédito**: importe hasta un máximo dependiente del presupuesto real del club, y
  **número de semanas** para devolverlo, acotado por las jornadas restantes (42 al
  empezar en Primera, 38 en Segunda; 1 en la última). También se puede amortizar
  anticipadamente para reducir intereses.
- **Primas** por ganar o empatar, con techo por partido según el presupuesto real. A
  mayor prima, mayor motivación. `[manual 4.0]`

---

## M · Estadio (Pro-Manager)

- Izquierda: **panorámica del estadio actual**, que cambia al ganar aforo.
- Derecha: presupuesto completo de la obra — precio total, semanas de duración,
  desembolso semanal, semanas restantes, dinero pendiente y aforo resultante. Los
  números se recalculan **en vivo** al pedir un número concreto de asientos.
  `[manual 4.0]`

**Regla que impone**: toda decisión con coste muestra su consecuencia numérica
completa **antes** de confirmarse.

---

## N · Clasificación y resultados

- Tabla con **indicador de tendencia** por equipo: flecha arriba, flecha abajo o
  cuadrado (se mantiene).
- Columnas PJ, PG, PE, PP, GF, GC, PTS y positivos/negativos.
- **Cada celda es pinchable**: al pinchar «empatados» del Athletic se abre una
  ventana con todos sus empates y en qué jornada. Vale para cualquier equipo y
  cualquier columna.
- Icono para saltar a Segunda División; icono `GOLEADORES` desde la misma pantalla.
- Panel de jornada con flechas ◀ ▶ a los lados del número: sirve de **calendario
  completo** de la temporada. `[manual 4.0]`

**Regla que impone**: **cada número es una puerta**. Ésta es la característica más
imitable y la que más "profundidad" aporta por línea de código.

---

## O · Goleadores

Lista encabezada por el pichichi con goles, nombre y equipo. Pinchar la casilla de
goles abre en qué partidos se consiguieron. Gráfico comparativo de hasta **3**
jugadores a la derecha. `[manual 4.0]`

---

## P · Comienzo y fin de partido

- **Comienzo**: jornada, ambos equipos (el local a la izquierda), y selección de
  controles, duración y tamaño del campo.
- **Fin**: resultado, goleadores con minuto y tarjetas mostradas.
- Durante el juego: `F1`/`F5` pedir cambio (aparece una «C» en el marcador hasta la
  siguiente interrupción; volver a pulsar lo anula), `F9` pausa con marcador y
  escudos, `F10` abandonar — **y abandonar el partido es abandonar la liga**.
  `[manual 4.0]`

---

## Q · Opciones de configuración

- El título vive en un **raíl vertical** a la izquierda: «OPCIONES DE CONFIGURACIÓN».
- Una placa grande contiene **seis sub-placas**, cada una con su barra de rótulo:
  - `PARTIDO` — casillas: Resultado, Resumen, Highlights, Visionado, Interactivo
  - `RESOLUCIÓN` — lista de radios: 400×300, 512×384, 640×480, 1024×768, 1280×800
  - `CALIDAD GRÁFICA` — pares etiqueta/valor: Jugadores ALTA, Dorsales ON, Animaciones
    ALTA, Estadio ALTA, Sombras ALTA, Césped ON + `OPCIONES HARDWARE ▶`
  - `CÁMARAS` — diagrama de campo + botón `LIBRE` (control espacial, no lista)
  - `SONIDO` — casillas con icono: Sonido FX, Ambiente, Comentarios
  - `TIEMPO DE JUEGO` — 4 / 8 / 12 / 20 minutos con casillas
- Arriba, un bloque aparte para **TECLADO** (Arriba/Abajo/Izquierda/Derecha/Pase/Tiro
  con la tecla asignada al lado y botón `REDEFINIR`) y otro para **ORDENADOR**.
- Barra de mando: `PREVIO OFF` · `AYUDA` · `CANCELAR` · `SEGUIR`.

**Reglas que impone** (directamente aplicables a nuestra pantalla de ajustes):

1. Los ajustes se **agrupan en placas rotuladas por tema**, no se apilan en una lista.
2. Cada grupo usa el control adecuado a su naturaleza: casillas para lo acumulable,
   radios para lo excluyente, pares etiqueta/valor para lo escalonado, **diagrama**
   para lo espacial.
3. Siempre hay **`CANCELAR` y `SEGUIR`**: se puede salir sin aplicar.
4. Todo cabe en una pantalla: cero desplazamiento vertical.

---

## R · Pantallas de celebración

Fin de competición: fondo sobrio, equipación del campeón y del subcampeón, trofeo,
rótulo «CAMPEÓN <competición> <año>» y un único botón `OK` discreto.

**Regla que impone**: los momentos ceremoniales rompen la densidad **a propósito**.
Una pantalla por temporada puede permitirse ser casi vacía.
