# 06 · Sistemas deportivos

Reglas de simulación del clásico. Se documentan para entender **qué densidad de
sistema esperaba el público**, no para copiarlas número a número: nuestras fórmulas
son propias y viven en `PCDynasty.Simulation`.

## 1. Atributos del futbolista

### Base (desde 4.0) `[manual 4.0]`

| Atributo | Efecto declarado |
|---|---|
| **Velocidad** | Se comprueba en carrera |
| **Resistencia** | Velocidad a la que pierde o recupera energía |
| **Agresividad** | Más probabilidad de cometer falta **y** de robar balón |
| **Calidad** | Dificultad para quitarle el balón + facilidad goleadora |
| **Energía** | Empieza a 99; se agota durante el partido |

### Variables de temporada (desde Pro-Manager 4.0) `[manual 4.0]`

| Atributo | Comportamiento |
|---|---|
| **Estado de forma** | Empieza entre 50 y 59. Sube jugando; si juega con regularidad alcanza su óptimo en el último tercio de temporada. Baja si no juega. Las lesiones lo hunden más rápido que la suplencia, porque impiden **entrenar** además de jugar |
| **Moral** | Empieza entre 90 y 99. Depende de participación, resultados, posición en la tabla, eliminaciones en copa, cambios de demarcación y goles marcados |

> «Un jugador bajo de moral o de forma física rendirá muy por debajo de sus
> posibilidades. Y si alguno tiene muy bajos los dos, su capacidad sobre el césped
> será casi nula.» `[manual 4.0]`

**Veteranía**: los jugadores mayores con muchos partidos acumulados pueden sufrir un
bajón en la recta final de temporada. `[manual 4.0]`

**Único contrapeso de una caída colectiva de moral**: las **primas**. Es un enlace
directo entre el sistema económico y el deportivo, y es lo que hace que las finanzas
no sean una pantalla decorativa. `[manual 4.0]`

### Habilidades específicas (fichas de 6.0 en adelante)

Portero, pase, regate, remate, entradas, tiro — mostradas en estrellas **con su
cifra**. Además de los cinco parámetros base, hay una **MEDIA** global destacada.

### Energía dentro del partido `[manual 4.0]`

- Se agota según el esfuerzo de cada jugador y su resistencia.
- Empiezan la temporada al máximo y arrancan cada vez más mermados según acumulan
  partidos → **hay que dosificar la plantilla a lo largo del año**.
- Un jugador muy cansado corre menos, chuta más flojo y pierde el balón con
  facilidad.
- Tras el descanso recuperan **parcialmente**, nunca al 100 %.

## 2. Roles y demarcaciones

Cada jugador tiene al menos un **rol** asignado según sus características reales. Los
18 de PC Fútbol 4.0: `[manual 4.0]`

```
 1 Portero                        10 Medio centro organizador
 2 Lateral derecho                11 Centrocampista exterior zurdo
 3 Lateral izquierdo              12 Extremo derecho
 4 Líbero                         13 Media punta por el centro
 5 Central por la izquierda       14 Extremo izquierdo
 6 Central por la derecha         15 Centrocampista organizador defensivo
 7 Centrocampista exterior diestro 16 Media punta diestro
 8 Centrocampista interior diestro 17 Media punta zurdo
 9 Delantero centro               18 Centrocampista interior zurdo
```

Regla clave, y la razón de que el sistema se sintiera realista: **la penalización es
por distancia entre roles, no por línea**. Poner a un lateral derecho de interior
diestro (puestos con similitudes) cuesta poco; poner a un extremo izquierdo de
central derecho hunde su rendimiento. Los polivalentes reales lo son también en el
juego. `[manual 4.0]`

## 3. Alineación

- Once inicial + **5 suplentes**. `[manual 4.0]`
- Cada equipo trae una alineación por defecto; cambiarla es «selecciona al sustituido,
  luego al sustituto».
- Aviso integrado de lesionados con **semanas de baja** en la propia fila.
- Mantener a un lesionado en el once ⇒ se juega con **uno menos**: «los médicos del
  club no permiten que la lesión se agrave por negligencia del entrenador». Igual con
  un sancionado. Sanción por diseño, no mensaje de error. `[manual 4.0]`
- Pasar el cursor por un jugador muestra sus **tarjetas acumuladas** en el hueco de la
  demarcación — información justo donde se toma la decisión. `[manual 4.0]`

## 4. Tácticas

- Seis esquemas predefinidos (3-4-3, 4-4-2…) + la táctica real de cada equipo por
  defecto + **guardar y cargar** tácticas propias. `[manual 4.0]`
- Cada jugador se coloca por su **dorsal**: posición defensiva (círculo), posición
  ofensiva (flecha) y **área de actuación** ajustable arrastrando las esquinas.
- Tres bandas de marcaje delimitadas por líneas móviles `D` y `M`:
  - defensas marcan desde su fondo hasta la línea `D`;
  - medios desde su fondo hasta la línea `M`;
  - delanteros desde `M` hasta el fondo contrario.
- **Emparejamientos**: marcaje al hombre par a par.
- «Todas tus indicaciones serán seguidas con gran disciplina por los componentes de tu
  plantilla» — en modo Entrenador la incertidumbre viene del rival y del azar, no de
  que tus jugadores te ignoren. `[manual 4.0]`

## 5. Entrenamiento

- Entrenamiento **general** o **específico por línea/puesto** (p. ej. entrenar
  porteros como `AR`, el resto en `G`). `[comunidad]`
- **El entrenamiento específico sólo rinde si la táctica concuerda**: si entrenas la
  línea de ataque, plantea un partido ofensivo. `[manual 4.0]`
- Puede causar **lesiones**, igual que un partido. `[manual 4.0]`
- En 2005 el entrenamiento se personaliza por equipo **o por jugador individual**.
  `[prensa]`

## 6. Lesiones y sanciones

- Lesiones en partido y en entrenamiento. Más probables tras una entrada dura y en
  los jugadores con más minutos acumulados. `[manual 4.0]`
- Se comunican en la pantalla de alineación con las semanas de convalecencia.
- Tarjetas: expulsión por doble amarilla o roja directa; **un partido de sanción por
  acumulación de cinco amarillas** o por expulsión. En competición europea, la
  expulsión sanciona el partido siguiente y **dos amarillas en partidos distintos**
  también. `[manual 4.0]`

## 7. Arbitraje

Árbitro y **dos jueces de línea** con atribuciones completas: el árbitro sigue el
juego, señala faltas y penaltis, muestra tarjetas e indica el camino del vestuario a
los expulsados; los linieres corren su banda y señalan faltas, fueras de juego y
córners. `[manual 4.0]`

> Nota de diseño: no aportaba nada mecánicamente, y sin embargo se anunció como
> novedad estrella. **El detalle que no cambia el resultado sí cambia la
> credibilidad.**

## 8. Controles del simulador arcade `[manual 4.0]`

- Jugador controlado marcado con **4 flechas**; al pasar el balón, las flechas se
  mudan al receptor.
- **Pase**: orienta y da un toque seco al botón de acción. **Pase largo**: mantener
  más tiempo, con menos precisión.
- **Tiro**: mantener un segundo y soltar. La fuerza sube muy rápido para que un
  disparo potente no entorpezca el manejo.
- **Remate / cañonazo**: tener el botón pulsado en el instante del contacto.
- **Chilenas y cabezazos**: sólo si el balón llega a la altura idónea — el mérito
  empieza en quien centra.
- **Balón parado**: si la falta admite barrera, aparece la lista de jugadores en
  campo y se elige lanzador con cursores + `Enter`.
- **Defensa**: se maneja al más cercano al balón, pero **el cambio de jugador no es
  automático** (a diferencia de 3.0): hay que pulsar acción para tomar al más
  cercano. Decisión de diseño deliberada, para que la defensa exija intención.
- **Entrada**: mantener acción hasta el nivel de fuerza deseado y soltar.
- **Portero autónomo**; sólo lo controlas tras una parada, para decidir fuerza y
  dirección del saque.
- Zoom ×2 (detalle) o normal (visión de juego); tamaño de campo completo o ampliado.
- Sonido separable: **ambiente** (murmullo, euforia, cánticos) y **efectos** (chuts,
  postes, silbato), combinables.
