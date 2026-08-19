# 10 · Datos, seguimiento y base de datos

La mitad no jugable del producto. Es la parte que hoy tenemos peor cubierta y la que
más barata sale, porque se alimenta de datos que el simulador ya genera.

## 1. Base de datos interactiva `[manual 4.0]`

Información exhaustiva de los 42 equipos de 1ª y 2ª (≈800–1.000 futbolistas). Control
por ratón **o teclado**, presentación «totalmente gráfica».

### Vías de entrada

- **Por equipos**: rejilla de escudos por división → pantalla del club.
- **Por protagonistas**: jugadores / entrenadores / **árbitros**, y dentro, la letra
  inicial del primer apellido → listado alfabético.

### Pantalla de club

Datos generales, estadio y aforo, presidente, presupuesto. Desde ahí:

- **Plantilla**: relación de jugadores marcando **nuevos fichajes**, **ascendidos del
  filial** y quién tiene foto disponible.
- **Palmarés**: competiciones nacionales y europeas en las que ha participado o ganado
  algo; abajo, participaciones y resultados en Primera y **gráfica de evolución de su
  clasificación en las 10 últimas temporadas**.
- **Comparativas**: ventana con escudos para elegir rival y ver los datos enfrentados.

### Ficha de jugador

Nombre completo, equipo, lugar y fecha de nacimiento, edad, procedencia, veces
internacional, demarcación; ventana de texto con características técnicas y biografía;
y **cuadro estadístico jornada a jornada** que se rellena con las actualizaciones:
alta/baja, minutos jugados, goles, lesiones, amarillas y rojas.

Complementos:

- **Demarcación**: campo con la posición habitual y flechas de zonas preferentes.
- **Notas**: el usuario escribe sus propias observaciones sobre cualquier jugador o
  entrenador; el programa las archiva y permite consultarlas, modificarlas e
  imprimirlas. **El jugador podía anotar su propia base de datos.**
- **Impresión**: cualquier pantalla con el icono `IMPRIMIR` se lleva a papel, con
  elección de número de copias y tipo de impresora, incluida la salida a fichero
  `PRINTER.TXT`.

> La opción de **notas** es la característica más infravalorada de toda la saga:
> convertía una base de datos de sólo lectura en el cuaderno del usuario. En un
> manager moderno equivale a listas de seguimiento, etiquetas y informes propios de
> ojeo. Coste bajo, apego altísimo.

## 2. Historia `[manual 4.0]`

Palmarés y clasificaciones históricas desde el inicio de cada competición:

- **Liga**: dos ventanas — clasificación histórica de todos los participantes, y
  resultados de todas las finales desde 1955 (campeón, subcampeón, ida y vuelta).
- **Copa del Rey**: desde 1902, década a década — campeones, alineación, rival,
  resultado en la final y palmarés histórico.
- **Copa de Europa**: además, ciudad y fecha de la final y **alineación del campeón**.
- **Recopa** y **UEFA**: mismo esquema.

## 3. Seguimiento de la Liga `[manual 4.0]`

Seguimiento completo de la temporada real: clasificación jornada a jornada,
alineaciones, resultado e incidencias, cuadro de goleadores y análisis del
comentarista.

- **Partidos de la jornada**: selector de jornada arriba a la derecha. Sin datos aún,
  muestra el calendario y los enfrentamientos. Al actualizar aparecen los resultados.
  Pinchar un partido da fecha, alineaciones, cambios con minuto, resultado,
  goleadores con minuto, amarillas, rojas, árbitro e incidencias.
- **Clasificación de la jornada**: posición y puntos de 1ª y 2ª, con PJ/PG/PE/PP/GF/GC.
  **Cada casilla abre su desglose**.
- **Goleadores**: listado encabezado por el pichichi; pinchar la casilla de goles
  detalla en qué partidos se marcaron; comparativa gráfica de hasta **3** jugadores.
- **Comentario del experto** reproducido mediante el sistema «TDC™», con un mando a
  la derecha de la pantalla para avanzar o parar.

## 4. Seguimiento manual `[manual 4.0]`

El usuario introduce los resultados y el programa **genera la clasificación
automáticamente**: jornada, PJ, PG, PE, PP, GF, GC, puntos y positivos, más goles de
la jornada y acumulado desde el inicio.

- Introducción: mantener pulsado sobre la flecha gris despliega una columna de
  números (0–7); arrastrar por debajo hace scroll hasta 29.
- Flechas a los lados del número de jornada para navegar.
- Todas las columnas de la clasificación son **pinchables** con desglose.
- **Comparativa**: gráfica de evolución de hasta 4 equipos, con conmutador para ver la
  temporada anterior.
- Disponible en Primera y Segunda; todo imprimible.

## 5. Actualizaciones `[manual 4.0]`

Por módem (ibertex) o disquete. Cuatro categorías:

| Categoría | Contenido |
|---|---|
| **Jornadas** | El programa indica cuáles hay disponibles, cuáles ya tienes en disco y cuáles ya están integradas en la base de datos |
| **Fotos** | Por división → equipo → jugador |
| **Programa** | Mejoras del ejecutable, con ventana de texto explicando qué aporta cada fichero |
| **Info** | Textos sobre versiones y cambios del servicio |

Al conectar, el servicio anunciaba la última novedad («¡¡Ya disponible la foto del
nuevo fichaje del…!!»).

> Traducción moderna: **paquetes de contenido versionados y verificables**. Nuestro
> `PackCompiler` + hash de contenido ya es la forma correcta de esto; lo que falta es
> la **experiencia**: enterarte de que hay algo nuevo y ver qué te aporta antes de
> aplicarlo.

## 6. Indicadores que conviene robar

| Indicador | Dónde | Por qué funciona |
|---|---|---|
| **Flecha ▲ ▼ ■** de tendencia junto al puesto | Clasificación | Estado y cambio en un glifo |
| Positivos/negativos | Clasificación | Métrica derivada gratis que da conversación |
| Marca de **nuevo fichaje / ascendido del filial** | Plantilla | Contexto sin abrir la ficha |
| Marca de **foto disponible** | Plantilla | Honestidad sobre la completitud del dato |
| Gráfica de 10 temporadas | Ficha de club | Historia larga en 200 píxeles |
| Comparativa de 3–4 entidades | Goleadores / clasificación | Techo bajo y deliberado: comparar no es analizar |
