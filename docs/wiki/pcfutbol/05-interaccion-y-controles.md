# 05 · Interacción y controles

> **Este capítulo está deducido, no observado.** Todo lo que afirma sale del
> manual de 4.0 y de capturas fijas. Una imagen quieta no dice cuántos clics hay
> entre dos pantallas, qué cambia al pasar el ratón por encima, qué se ve
> mientras carga ni qué suena al pulsar.
>
> Para comprobarlo hay dos vías, ambas en
> [`../pc-dynasty/como-mirar-el-juego.md`](../pc-dynasty/como-mirar-el-juego.md) §5:
> **jugarlo emulado en el navegador** en
> <https://online.dinamicmultimedia.es/juegos.html>, o **ejecutarlo en local**
> bajo Wine en un display virtual, que es la única de las dos que un agente
> puede conducir por su cuenta.
>
> Cada afirmación que se confirme jugando debería pasar de `[manual 4.0]` o
> `[capturas]` a una etiqueta nueva, `[jugado]`, con la fecha.

## 1. El entorno GFX™ y sus reglas

PC Fútbol 4.0 declaraba su capa de interfaz como un producto con nombre propio,
«entorno gráfico GFX™», y enunciaba sus reglas en el manual. Merece la pena citarlas
porque son, literalmente, un manifiesto de usabilidad de 1995: `[manual 4.0]`

- **«Todas las opciones posibles se visualizan claramente en pantalla mediante
  iconos.»** Nada esencial oculto.
- **El cursor se convierte en una mano** al pasar sobre un icono: la zona activa se
  descubre moviendo, no leyendo.
- **Tooltip por reposo**: «para saber su utilidad basta con mantener el cursor unos
  segundos sobre ellos, apareciendo una reseña escrita».
- **Atajo de teclado visible**: cada icono lleva **una letra en rojo**; pulsarla
  equivale a activarlo. En monitores monocromo la letra aparece más clara. El atajo no
  está en un manual aparte: está impreso en el propio botón.
- **Emulación de ratón por teclado**: mayúsculas + cursores mueven el puntero.
- **Mini-iconos**: acceso directo entre áreas **sin volver al menú principal**.

> Cinco de esas seis reglas siguen siendo buenas prácticas treinta años después. La
> sexta (emulación de ratón) hoy la resuelve el sistema operativo.

## 2. Idiomas de interacción

### Selección directa sobre la representación

Si el objeto tiene forma, se manipula por su forma: escudos en rejilla en vez de
lista, dorsales sobre el campo en vez de desplegables, líneas `D`/`M` arrastrables en
vez de un valor «altura de la defensa: 65», banderas sobre un mapa en vez de un
selector de país. `[manual 4.0]`

### El dato como puerta

Pinchar una cifra agregada abre el desglose que la produce, en una ventana junto al
dato: los empates de un equipo, los partidos en que un goleador marcó, la composición
de cada columna de la clasificación. Es **la** característica que convertía tablas
estáticas en una base de datos navegable. `[manual 4.0]`

### Pulsación mantenida = elegir de una lista

Para asignar rol se pincha el icono **sin soltar**; se abre la lista de los 18 roles;
el que esté señalado al soltar queda asignado, y el rol original sigue sobreimpreso
en amarillo para poder revertir. Es un menú contextual antes de que existiera el
nombre. `[manual 4.0]`

### Selección por pares

Emparejamientos (marcajes) y sustituciones funcionan igual: **selecciona A, luego
selecciona B**, y el juego infiere la operación. Sin diálogo intermedio.
`[manual 4.0]`

### Introducción numérica por columna

Para meter resultados en el seguimiento manual: mantener pulsado sobre la flecha gris
despliega una columna vertical de números (0–7); se suelta sobre el elegido; si hace
falta más, arrastrar por debajo hace scroll hasta 29. `[manual 4.0]`

## 3. Navegación

- **El despacho es el centro**. Todo destino está a un clic de él, y todo destino
  vuelve a él.
- **Sin migas de pan**: la jerarquía es tan plana que no hacen falta. Máximo dos
  niveles: despacho → pantalla → ventana de desglose.
- **Una sola vuelta atrás**: `VOLVER` sube un nivel; nunca hay ambigüedad sobre a
  dónde va.
- **`SEGUIR` es sagrado**: abajo a la derecha, siempre, y es el único control que
  avanza el tiempo. El jugador puede tocar cualquier cosa sin miedo a que pase la
  jornada por accidente.
- **Atajos globales durante el partido**: `F1`/`F5` cambio, `F9` pausa, `F10`
  abandonar (con la consecuencia de abandonar la liga). `[manual 4.0]`

## 4. Latencia y percepción

El clásico era instantáneo porque no podía permitirse no serlo. Nuestro presupuesto
está en [`docs/architecture/0007-interaction-latency-and-work-budget.md`](../../architecture/0007-interaction-latency-and-work-budget.md).
Traducido a reglas de pantalla:

1. **Cambiar de área no consulta nada.** Los modelos de lectura se preparan antes de
   mostrarse.
2. **Durante una actualización permanece visible el último estado válido.** Nunca se
   vacía la pantalla para volver a llenarla.
3. **Nada de animaciones de transición entre áreas.** El clásico cortaba en seco y se
   percibía como rapidez.
4. Si algo tarda de verdad (simular una temporada), se muestra **progreso con
   unidades reales** (jornada 12 de 38), no una barra genérica.

## 5. Trampas de implementación (Godot) — **leer antes de tocar la UI**

Errores concretos que ya se han cometido en este repositorio y que rompen justo lo
que esta wiki pide. **inferencia**, verificada contra el código.

### 5.1 Un `Button` con hijos `Control` deja de ser pulsable

`Button` **no es un contenedor**. Si se le añade un `HBoxContainer`/`VBoxContainer`
como hijo para componer icono + texto:

- El hijo hereda `mouse_filter = STOP` y **se come el clic**: el botón nunca emite
  `pressed`.
- El hijo no se posiciona ni se dimensiona por el botón: se dibuja en su tamaño
  natural y **se sale del marco**.

Solución: componer con un `PanelContainer` + `MarginContainer` que dibuje el aspecto,
y capturar la entrada explícitamente, o poner `MouseFilter = Ignore` en **todos** los
descendientes y dimensionar el botón a mano. Nuestra elección es la primera: un
control compuesto propio.

### 5.2 Construir hijos en `_Ready` cuando se configura antes de entrar al árbol

Si una pantalla hace `panel.SetTexts(...)` y **después** `AddChild(panel)`, todo lo
que `_Ready` cree llegará vacío: los textos se asignaron a campos que aún no
existían. Los hijos de un componente se construyen **en el constructor**.

### 5.3 Posiciones absolutas

`offset_left/right/top/bottom` fijos en la escena atan el diseño a una resolución.
Todo lo que no sea un elemento decorativo debe usar anclas y contenedores.

### 5.4 Traducir dentro del componente

Un componente compartido no llama a `tr()`: **pinta el texto que le dan**. Si traduce
por su cuenta, o duplica claves, o —como pasó aquí— muestra la clave en crudo cuando
nadie ha registrado el catálogo.

### 5.5 Reconstruir el árbol en cada refresco

Vaciar y recrear un panel entero por cada cambio provoca el parpadeo y la lentitud que
el jugador percibe como «carga lenta». Lo correcto es reconstruir **sólo las filas
afectadas** y mantener la identidad de los nodos.
