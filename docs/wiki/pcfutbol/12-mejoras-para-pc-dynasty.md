# 12 · Mejoras para PC Dynasty

Este fichero **no describe el clásico**: decide qué hacemos nosotros. Es la fuente de
la que deben salir las propuestas nuevas, y cada entrada se etiqueta con su estado.

Leyenda: `[hecho]` · `[en curso]` · `[pendiente]` · `[descartado]`

---

## A · Deuda con la versión objetivo (6.0/7.0 ya lo tenía)

| # | Qué | Estado |
|---|---|---|
| A1 | Despacho central con 4 áreas × 3 destinos, todos visibles | `[pendiente]` |
| A2 | Banda de contexto permanente: club · calendario · tesorería | `[en curso]` |
| A3 | Barra de mando fija con `SEGUIR` abajo a la derecha como único avance del tiempo | `[pendiente]` |
| A4 | Tablas densas con cabecera coloreada, filas alternas y columna índice | `[en curso]` |
| A5 | **Cada número es una puerta**: celda agregada ⇒ ventana de desglose | `[pendiente]` |
| A6 | Escalera de responsabilidad (Entrenador / Manager / Presidente / Total) | `[pendiente]` |
| A7 | Cuatro modos de partido, con `Resultado` e informe escrito como ciudadanos de primera | `[en curso]` |
| A8 | Manipulación directa sobre el campo para todo lo espacial | `[pendiente]` |
| A9 | Ficha de jugador como póster: bloques de color, media grande, estrellas con cifra | `[pendiente]` |
| A10 | Simulación completa de la decisión **antes** de confirmarla (estadio, crédito, oferta) | `[pendiente]` |
| A11 | Tres semanas de números rojos antes de la quiebra: la ruina se ve venir | `[pendiente]` |
| A12 | Ligas consecutivas con clasificación europea encadenada por reglas | `[pendiente]` |
| A13 | Conmutador de vista sobre una misma tabla (`PARÁMETROS` / `CALIFICACIÓN`) | `[pendiente]` |
| A14 | Ficha lateral reactiva: señalar una fila actualiza el panel sin navegar | `[pendiente]` |
| A15 | Placa del empleado responsable en la pantalla que gobierna | `[pendiente]` |
| A16 | Listas de ranuras que conservan su forma vacías (negociaciones abiertas) | `[pendiente]` |
| A17 | Negociación como diario fechado en prosa, no como campos de estado | `[pendiente]` |
| A18 | Dossier con dos tiras de pestañas (faceta arriba, modo abajo) y notas propias | `[pendiente]` |
| A19 | Ficha de club: estadio, aforo, presupuesto, socios, patrocinador, equipador | `[pendiente]` |
| A20 | Pretemporada con amistosos de pago y partido de presentación | `[pendiente]` |

## B · Corregir lo que el clásico hacía mal

| # | Problema del clásico | Nuestra decisión |
|---|---|---|
| B1 | Motor 3D pobre que arrastraba al conjunto | No enseñar 3D hasta que esté a la altura. Informe de partido y visionado esquemático primero |
| B2 | Ruina en solitario ⇒ «empieza de nuevo» | Consecuencia narrativa: descenso, destitución, ofertas peores. Nunca borrar la partida |
| B3 | Contraseñas por manager para proteger progreso | Perfiles del sistema; sin contraseñas |
| B4 | Modos exclusivos entre sí que obligaban a reiniciar | Perfil de responsabilidad **cambiable a mitad de carrera** |
| B5 | Datos con licencia que caducan y obligan a comprar cada año | Mundo ficticio propio y paquetes versionados |
| B6 | Regresión de funciones entre versiones | Prueba de humo que **falla** si una pantalla pierde capacidad |
| B7 | Interfaz atada a una resolución | Todo anclado y escalable; comprobado a 1024×600 y 2560×1440 |
| B8 | Comparativas limitadas a 3–4 entidades por límite técnico | Mantener el límite **por diseño** (comparar no es analizar), pero que sea decisión, no restricción |

## C · Añadir lo que en 1996 no se podía

| # | Idea | Origen | Estado |
|---|---|---|---|
| C1 | **Cuaderno del manager**: notas propias sobre jugadores y clubes, con etiquetas y listas de seguimiento | Evolución de las «Notas» de la base de datos | `[pendiente]` |
| C2 | **Repetición determinista**: mismo seed ⇒ mismo partido, y poder demostrarlo | Imposible entonces | `[hecho]` |
| C3 | **Historial consultable de decisiones**: por qué subió el precio, quién autorizó el crédito | — | `[pendiente]` |
| C4 | **Explicación del resultado**: qué pesó en el marcador (táctica, forma, campo, azar) | El clásico lo escondía | `[pendiente]` |
| C5 | **Accesibilidad**: contraste ≥4.5:1, objetivos ≥40 px, navegación completa por teclado, movimiento reducido | Parcialmente en GFX™ (letra roja de atajo) | `[en curso]` |
| C6 | **Idioma conmutable en caliente** sin reiniciar | — | `[hecho]` |
| C7 | **Paletas alternativas** (clásica, alto contraste, claro) sobre los mismos roles | — | `[hecho]` |
| C8 | **Paquetes de contenido verificables** con hash y validación previa | Evolución de las actualizaciones por módem | `[hecho]` |
| C9 | **Editor de datos de ida y vuelta**: lo que edita el jugador vuelve a compilar | La comunidad lo hacía por ingeniería inversa | `[en curso]` |
| C10 | **Presupuesto de latencia medido en CI**, no prometido | — | `[hecho]` |
| C11 | **Búsqueda global** («¿dónde estaba esto?») sobre clubes, jugadores y competiciones | El clásico usaba índices alfabéticos | `[pendiente]` |
| C12 | **Comparador libre** de hasta 4 entidades sobre cualquier tabla | Copiado de goleadores y clasificación | `[pendiente]` |
| C13 | **Resumen de jornada** legible: lo importante primero, el detalle a un clic | Evolución del seguimiento | `[pendiente]` |
| C14 | **Empleados con una mecánica exclusiva cada uno** (si contratarlo no mueve un número visible, no existe) | Lección de 2001 vs 2005 | `[pendiente]` |
| C15 | **Un torneo corto como modo secundario** sobre el mismo motor: convocatoria, cuadro y siete partidos, sin mercado ni finanzas | PC Selección '96 | `[pendiente]` |
| C16 | **La convocatoria como mecánica propia**, distinta de fichar: elegir N de entre cientos sin dinero de por medio | PC Selección '96 | `[pendiente]` |
| C17 | **Vídeo en los momentos que se recuerdan** — apertura, antesala del partido, trofeo — todos desactivables | — | `[en curso]` |
| C18 | **Audio con música y efectos separados**, cada uno con su nivel | Pantalla de configuración clásica | `[en curso]` |

## D · Descartado explícitamente

| # | Qué | Por qué |
|---|---|---|
| D1 | Quinielas / apuestas | Fuera del alcance y con implicaciones regulatorias |
| D2 | Licencias reales de clubes, jugadores o competiciones | Coste, caducidad y dependencia externa. El mundo es propio |
| D3 | Imagen o voz de personas reales | Igual que D2 |
| D4 | Impresión a papel | Sustituida por exportación |
| D5 | Contraseñas por manager | Ver B3 |
| D6 | Arcade jugable a corto plazo | Ver B1. Se reevalúa cuando el resto esté cerrado |

## E · Orden de ataque sugerido

1. **A2 + A3 + A7** — el marco: banda de contexto, barra de mando, informe de partido.
   Sin esto ninguna pantalla se siente un manager.
2. **A1** — el despacho. Es la pantalla que el jugador verá más veces.
3. **A4 + A5** — tablas densas y desgloses. Aquí está la profundidad percibida.
4. **A9 + A8** — ficha de jugador y campo manipulable.
5. **A10 + A11 + A6** — las decisiones con consecuencia.
6. **C1, C4, C13** — lo que nos diferencia del original.
