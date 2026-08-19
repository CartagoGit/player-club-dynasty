# 09 · Competiciones y calendario

## 1. Qué se podía disputar

| Competición | Desde | Formato `[manual 4.0]` |
|---|---|---|
| Liga (1ª, 2ª, 2ªB) | 1.0 / 2ªB en 5.0 | Todos contra todos, 42 jornadas en Primera, 38 en Segunda |
| **Copa del Rey** | 4.0 | Equipos de 1ª y 2ª + base de datos de 2ªB. **Siete rondas**: cinco eliminatorias + semifinales + final. Los equipos con competición europea **se libran de las tres primeras** y entran en octavos. Doble partido salvo la final, a partido único en campo neutral. Ida de la 1ª eliminatoria entre jornadas 8 y 9; final tras acabar la Liga |
| **Copa de Europa** | 4.0 | 8 grupos previos: liguilla de cuartos con cabezas de serie (máximo dos por grupo), 3 puntos por victoria y 1 por empate. Los **dos primeros de cada grupo** pasan a cuartos. Cruces: 1ºA–2ºB, 1ºD–2ºC, 1ºB–1ºA, 1ºC–2ºD. Eliminatorias a doble partido hasta la final, a partido único en campo neutral. Primer partido entre jornadas 2 y 3; final entre la 34 y la 35 |
| **Recopa** | 4.0 | 32 equipos, arranca en dieciseisavos. 8 cabezas de serie que no se cruzan hasta la 3ª ronda; sorteo puro. Empieza entre jornadas 2 y 3; final entre la 32 y la 33, a partido único en campo neutral |
| **Copa de la UEFA** | 4.0 | 64 equipos ⇒ una eliminatoria más. **16 cabezas de serie**, sin cruzarse en las tres primeras rondas. La final se juega **a doble partido**. 1/32 entre jornadas 2 y 3; ida de la final entre la 31 y la 32, vuelta entre la 33 y la 34 |
| Ligas internacionales | 7.0 (Euro PC Fútbol) | España, Francia, Inglaterra, Alemania, Italia `[wiki]` |
| **EuroManager** | 2000/2001 | + Bélgica, Argentina, Escocia, Portugal, Holanda `[prensa]` |

Desempates en eliminatorias: **prórroga y lanzamientos desde el punto de penalti**.
`[manual 4.0]`

## 2. Clasificación a Europa `[manual 4.0]`

Modelado explícitamente como cadena de consecuencias, no como tabla fija:

- Campeón de Liga ⇒ **Copa de Europa** la temporada siguiente.
- Campeón de Copa del Rey ⇒ **Recopa**.
- Puestos 2º, 3º y 4º ⇒ **UEFA**… salvo que uno de ellos gane la Copa del Rey (jugaría
  Recopa) o una competición europea de rango mayor. En ese caso **su plaza de UEFA
  pasa al siguiente clasificado**.
- El campeón de Recopa juega la Recopa siguiente.

> Es un sistema de reglas encadenadas que el jugador puede razonar. Nuestra
> `PCDynasty.Competition` debe expresarlo como reglas declarativas, no como una tabla
> hard-codeada.

## 3. Ligas consecutivas `[manual 4.0]`

Concepto presente en Arcade, Entrenador, Manager y Pro-Manager: al terminar una
temporada **se encadena la siguiente** con las consecuencias de la anterior
(clasificaciones europeas, ascensos y descensos, envejecimiento). Es la base de todo
modo carrera.

En 2000 se añadió además **envejecimiento de los jugadores** y estadísticas
persistentes como opciones explícitas. `[prensa]`

## 4. El calendario como interfaz

- El panel de jornada con flechas `◀ nº ▶` sirve simultáneamente de **resultados** y
  de **calendario completo de la temporada**: se puede recorrer desde la jornada 1
  hasta la última aunque no se haya jugado. `[manual 4.0]`
- Las eliminatorias se expresan siempre como **«entre las jornadas X e Y»**: el
  calendario de copa se ancla al de liga en vez de existir aparte. El jugador razona
  en jornadas, no en fechas.
- La **placa de calendario** en la banda superior mantiene mes, día y año siempre
  visibles.

> Norma: **una única línea temporal**. Todas las competiciones se anclan a ella. Es lo
> que hace comprensible una temporada con cuatro torneos simultáneos.

## 5. Persistencia de partida `[manual 4.0]`

- Grabar y cargar liga desde el menú de la propia liga.
- El programa **asigna nombre automáticamente** a cada liga grabada.
- Cada entrenador puede tener cuantas ligas quiera, pero con la misma clave siempre
  se usa el mismo nombre de manager.
- En partidas multi-manager, retomar exige la presencia y la clave de **todos**.
