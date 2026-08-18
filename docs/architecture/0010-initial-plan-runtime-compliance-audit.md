# Auditoría de cumplimiento: plan maestro, arquitectura y runtime jugable

## Estado y alcance

Esta auditoría contrasta tres fuentes de verdad:

1. la visión y los requisitos de `docs/mcp-vertex/INITIAL-PLAN.md`;
2. las decisiones permanentes de `docs/architecture/0001..0009` y la guía
   visual;
3. el camino que realmente ejecuta `game/godot/Scenes/Main.tscn`.

Una propuesta cerrada demuestra únicamente su aceptación escrita. No implica
que la capacidad esté integrada, persistida, sea alcanzable desde Godot o
forme parte del bucle de carrera. Por eso la matriz distingue:

- **integrado**: lo usa el ejecutable y existe evidencia end-to-end;
- **parcial**: hay un flujo visible, pero no preserva todo el contrato;
- **isla**: dominio/tests verdes sin consumidor productivo;
- **ausente**: no existe aún una implementación verificable;
- **defectuoso**: el flujo visible contradice una invariante.

## Dictamen

Godot 4 + .NET sigue siendo la decisión adecuada. Unity no corregiría las
brechas encontradas y Tauri/Electron debilitarían la base de juego de
escritorio. Los límites físicos más importantes existen: Core no referencia
Godot, SQL ni Football; hay contratos separados; los importadores y SQLite
viven fuera del dominio; TestSport protege parte de la neutralidad.

El problema real es que hoy conviven dos arquitecturas:

- una arquitectura diseñada y probada en ensamblados aislados;
- una ruta simplificada, distinta, que usa Godot.

El ejecutable es un vertical slice visible, no todavía el manager completo del
plan maestro. Puede abrir una carrera, navegar, guardar una alineación, jugar
resultados sintéticos, comprar un jugador y continuar. Todavía no posee un
reloj de carrera, un estado mundial autoritativo, un orquestador único, una
temporada reglada, plantillas futbolísticas completas ni la integración de
desarrollo, lesiones, scouting, instalaciones, cantera, staff o carrera del
manager.

## Línea base verificable

| Evidencia | Estado actual |
| --- | --- |
| Proyectos de producción bajo `game/src` | 25 |
| Proyectos de tests | 27 |
| Proyectos alcanzables desde el ensamblado Godot | 15 de 25 |
| Módulos no alcanzables relevantes | Career, Careers, Club, Content compiler, Football, Rules, World y Persistence genérica |
| Starter world | 3 países, 6 clubes, 18 personas, 4 competiciones |
| Personas con ocupación `player` | 11 |
| Escenas Godot | 5 |
| Suite .NET Release | verde |
| Tests de arquitectura | 19 verdes, pero con falsos negativos descritos abajo |
| Godot Debug/Release + SmokeBoot | verde |
| Higiene Git | verde; `bin`, `obj`, `.godot`, saves y caches ignorados, sin artefactos generados trackeados |
| Export autocontenido | ausente; no existe `export_presets.cfg` |

La alcanzabilidad se calculó transitivamente desde
`game/godot/PCDynasty.Presentation.csproj`. TestSport no debe formar parte
del runtime, pero los demás módulos isla contienen capacidades que el roadmap
presenta como terminadas.

## Matriz de trazabilidad de producto

| Capacidad del plan | Camino ejecutable real | Estado |
| --- | --- | --- |
| Menú, nueva carrera y continuar | Menú original, selección de club y un único perfil; no se elige manager y `Cargar carrera` está deshabilitado | parcial |
| Mundo navegable | Seis clubes y cuatro competiciones sin divisiones completas, calendario ni escala mundial | demo |
| Plantilla | Tabla visible, pero incluye managers como si fueran jugadores | defectuoso |
| Ficha profunda y conocimiento | Existen escena/controlador y dominio de Knowledge, pero las filas de plantilla no abren la ficha | isla |
| Alineación y táctica | Persiste una selección y un texto libre; admite un participante, no modela once/banquillo/posición y no afecta al partido | persistencia sin gameplay |
| Partido | Marcador, timeline y replay; usa prestigio, afición y tamaño de plantilla, no futbolistas ni táctica | demo técnica |
| Eventos de partido | El actor es el club; no hay goleadores. Con magnitud máxima 8, los umbrales 40/60/80 hacen imposibles Card/Shot/Save | defectuoso |
| Competiciones | Partido aislado o round-robin simple inmediato; una copa también se trata como liga | defectuoso |
| Carrera temporal | No hay fecha de juego ni AdvanceDay/Week/ToDate | ausente |
| Temporada | Guarda partidos y resumen, pero no aplica reglas, ascensos, descensos, economía, contratos, lesiones, desarrollo o cantera | parcial |
| Save/load | Perfil, historial, alineación, finanzas y fichajes en tablas paralelas; no persiste un mundo versionado con packs/hashes | parcial |
| Finanzas | Estado inicial y débito de compras; no evoluciona con actividad deportiva, salarios, premios o mantenimiento | parcial |
| Mercado | Compra inmediata; no acredita al vendedor ni cambia el ownership mundial, por lo que el jugador sigue en su club fuente | defectuoso |
| Scouting | Dominio/tests; botón Godot deshabilitado | isla |
| Entrenamiento, química y motivación | Tipos y tests sin caso de uso productivo ni UI | isla |
| Desarrollo, desgaste y lesiones | Dominio/tests sin calendario, persistencia de carrera o UI | isla |
| Instalaciones, cantera y staff | Dominio/tests sin Application, persistencia o Godot | isla |
| Carrera del manager | Reputación, relaciones y ofertas en un ensamblado no referenciado | isla |
| Editor | Escena aislada y servicios; opción de menú deshabilitada | isla |
| Contenido externo | Adaptadores JSON/CSV/SQLite existen, pero el juego fija el starter JSON y no permite elegir una fuente | parcial |
| Distribución | Launcher local robusto y de instancia única; requiere SDK/runtime y Godot Mono instalados | no entregado |

### Evidencia principal

- `WorldClubProjection.ListPlayers` filtra por `kindId == person` y relación
  con club, no por ocupación. El filtro `occupation == player` sólo se usa
  para el contador: `WorldClubProjection.cs:21-25, 91-98`.
- El starter world contiene 11 players y 7 managers:
  `game/content/starter-world/data/people.json`.
- `TacticsScreen` y `CompetitionsScreen` reciben el club consultado, no
  necesariamente el gestionado:
  `ManagerWorkspaceController.cs:165-176`.
- `MatchSimulator.BuildAttributes` usa prestigio, afición y tamaño:
  `MatchSimulator.cs:81-96`. La clasificación imposible de eventos está en
  `MatchSimulator.cs:136-144`.
- `SeasonSimulator` codifica round-robin y 3/1/0:
  `SeasonSimulator.cs:18-65, 126-134`.
- El guardado de alineación no es una entrada de
  `ICompetitionSimulationService`.
- `MainMenuController.cs:64-67` deshabilita selector de carreras y editor.
- No existe ningún consumidor productivo de `PCDynasty.Career`,
  `PCDynasty.Careers`, `PCDynasty.Club`, `PCDynasty.Football` o
  `PCDynasty.World`.

## Cumplimiento arquitectónico

### Bien establecido

- Core permanece libre de Godot, SQL y vocabulario público de Football.
- Football y TestSport están separados del Core.
- Los DTO de importación, los contratos de persistencia y los adaptadores
  SQLite están en ensamblados distintos.
- Nullability, analyzers, warnings-as-errors, formato y suite Release están
  activos.
- Hay puertos estrechos útiles para finanzas, alineación, mercado y stores de
  carrera.
- El launcher verifica .NET 8 y usa `flock` para impedir instancias dobles.
- La ruta de la base de carrera admite override externo mediante
  `PCDYNASTY_CAREER_DB`.

### Brechas críticas

#### 1. El runtime evita el pipeline de packs

`PresentationBootstrap.cs:38-47` fija `starter-world`, crea
`JsonContentReader` y entrega modelos normalizados a `WorldService`.
`WorldContentCatalog.cs:31-49` lee cinco nombres fijos, pero no usa
`PackValidator` ni `PackCompiler`. Los enteros inválidos se transforman
silenciosamente en cero en `WorldContentCatalog.cs:93-105`.

Esto incumple el pipeline Reader → normalized model → validator → compiler →
pack inmutable/versionado, deja fuera StableId y hash, y explica por qué un
manager con atributos ausentes acaba tratado como jugador de valor cero.

#### 2. No existe el orquestador temporal definido

`ICompetitionSimulationService` no recibe world version, fecha, seed raíz,
rules hash, correlación ni checkpoint.
`CompetitionSimulationService.ReserveSeedRange` deriva seeds del reloj y un
contador mutable. La instancia se crea una vez en
`PresentationBootstrap.cs:23-25` y se comparte entre carreras.

No existe un único `AdvanceWorld`; partidos, temporadas, mercado, economía y
desarrollo no participan en una agenda determinista.

#### 3. Application duplica reglas de Football

`MatchSimulator` fija 90 minutos, goles y un ruleset de fútbol.
`SeasonSimulator` fija 3/1/0. Ya existen `FootballContestPack` y
`FootballScoringRules`, pero sólo los usan tests. La capa de aplicación está
ejecutando una segunda implementación de fútbol y los packs no gobiernan el
juego.

#### 4. Presentation puede divergir de la persistencia

`CareerSession` en Presentation.Core es propietario de historial mutable.
`CareerHistoryPersistence.Save` modifica la memoria antes de guardar y
captura cualquier excepción como warning. La UI puede mostrar un resultado
que nunca quedó persistido.

La creación de carrera también limpia historial, alineación, transferencias,
finanzas y perfil mediante operaciones separadas. No hay una unidad atómica,
manifest de carrera ni migración de sus schemas.

#### 5. I/O síncrono en handlers Godot

Mercado consulta SQLite durante `Render` y ejecuta la transacción desde el
evento de botón. Finanzas, historial y alineaciones usan contratos síncronos.
Esto contradice el presupuesto de frame sin I/O de ADR 0007/0009.

#### 6. La ruta real no está indexada

`WorldClubProjection.ListPlayers` recorre todas las personas en cada
consulta. `FindClub` reconstruye toda la lista. El mercado recorre clubes y
vuelve a consultar personas para cada uno: aproximadamente O(clubes ×
personas). No puede escalar al mundo objetivo.

El benchmark llamado `OpenClubView` mide `HistoryQuery`, no
`WorldService`; el benchmark de manager no incluye SQLite aunque su fixture
diga “persistence record”.

#### 7. Save contracts y arquitecturas paralelas

Existen dos manifestos de save similares en `PCDynasty.Contracts` y
`PCDynasty.Persistence.Abstractions`. `IWorldStore`, `SaveMigrator`,
`CommandEnvelope`, `EventEnvelope` y el protocolo determinista no tienen
consumidor en el juego.

La carrera visible usa tablas específicas de presentación/aplicación, mientras
el world store y el migrador validados quedan en otra ruta.

## SOLID, Clean Code, nombres y reutilización

La descomposición anterior mejoró WorldService, SQLite world store y el
workspace de 1.500 líneas. Aun así, los gates actuales permiten nueva deriva:

- `PresentationStructureTests` acepta hasta 500 líneas por archivo y se basa
  en búsquedas de strings; no verifica el grafo completo.
- `HistoryScreen.cs` mezcla estado de filtros, consulta, orden, creación de
  nodos y la regla `wins * 3`.
- `CareerTransferMarketService.cs` consulta candidatos, proyecta plantilla,
  valora, negocia, genera timestamps, debita y coordina persistencia.
- `PackDocument.cs` contiene ocho tipos públicos y validación en 257 líneas.
- `IManagerWorld.cs` contiene la interfaz y tres records públicos sin
  validación completa.
- `SeasonSimulator.cs` contiene el simulador y varios contratos públicos.
- Cinco stores de carrera repiten lock, apertura, creación de directorio e
  inicialización de schema; los locks son por instancia aunque comparten DB.
- `PCDynasty.Career` y `PCDynasty.Careers` expresan contextos distintos con
  nombres casi idénticos y sin integración, lo que hace ambiguo el ownership.
- `WorkspaceUiFactory.PlayerAge` fija 2026 y `PlayerCountry` infiere país
  desde substrings del ID. Presentación inventa datos que deberían llegar en
  un read model.

La reparación no consiste en añadir patrones indiscriminadamente. Debe
separar queries, comandos, políticas y adaptadores donde ya existe cambio
independiente, mantener interfaces pequeñas y borrar implementaciones
paralelas.

## Persistencia y contenido externos

La base de carrera puede apuntar a una ruta externa por variable de entorno,
pero no es todavía “cargable” por un jugador:

- sólo hay un slot;
- `Cargar carrera` está deshabilitado;
- no hay manifest con versión de save, seed y hashes de packs;
- no hay selector/importación/backup/migración recuperable;
- el contenido visible está fijado a JSON y al directorio del repositorio;
- el adapter SQLite de contenido no es seleccionable;
- las rutas por defecto usan directorios relativos al proyecto, no
  `user://`, y pueden fallar en un export instalado.

El objetivo debe ser un catálogo de fuentes sustituibles y un selector de
carreras, no exponer SQL ni rutas a las pantallas.

## Experiencia visual y de uso

La paleta, tokens, densidad de tabla y menú son originales y coherentes. No se
debe copiar PC Fútbol. La brecha no es “hacerlo más parecido” mediante assets
ajenos, sino completar la gramática de un manager clásico:

- la guía limita la navegación principal a siete áreas; el workspace muestra
  nueve más el menú, una de ellas deshabilitada;
- falta cabecera persistente con fecha, club, próximo partido y decisiones;
- los botones no conservan visualmente la sección activa;
- no hay agenda/inbox que orqueste el día del manager;
- las filas de plantilla no abren el drill-down de jugador existente;
- no existen formación visual, banquillo, posición ni ficha por atributo;
- no hay estados cargando/obsoleto/error por componente;
- “Pretemporada”, alertas y siguientes decisiones son texto fijo;
- no hay crest, kit, retrato o estadio más allá de iniciales;
- opciones como densidad y movimiento se muestran, pero no se aplican ni se
  persisten.

La UI debe crecer sobre componentes y read models, no añadir reglas a las
pantallas.

## Rendimiento y benchmarks

| Obligación del plan | Evidencia actual | Estado |
| --- | --- | --- |
| 10.000 partidos Fast | 32 contests sintéticos; un partido en el gate de manager | ausente |
| 160.000 DevelopmentStep | 50.000 | parcial |
| Mercado de 6.400 clubes | sin workload | ausente |
| 320 cierres de liga | una transición de 512 clubes/16 competiciones | distinto |
| 30 años macro | una temporada | ausente |
| Load/save | BDN de 1 MiB + 256 hechos, sin gate rápido | parcial |
| Materialización | 1 club, 512 clubes y 1.000 commitments | parcial |
| Scouting queries | sin workload | ausente |
| Clasificación continental | sin workload | ausente |
| First-world cold start | presupuesto declarado, no medido | ausente |
| Godot navegación/render | no medido | ausente |

`game/benchmarks/budgets.json` declara más workloads de los que
`BenchmarkBudgetTests` ejecuta. `InteractionBudgetTests` tampoco exige
`advance-season`, `load-existing-save`, `first-world-load`,
`background-work-slice` ni la ruta SQLite real.

## CI, propuesta y gates

- `bun run validate`, configurado como gate de mcp-vertex, valida la
  integración MCP/TypeScript, no la solución .NET ni Godot.
- GitHub Actions compila/testea .NET y ejecuta BDN, pero no instala Godot, no
  compila el proyecto Godot ni ejecuta SmokeBoot.
- El job BDN publica informes, pero no compara todos los resultados con los
  budgets versionados.
- Los tests de arquitectura revisan Core y patrones concretos, no el grafo
  completo de assemblies, contratos centrales, módulos isla o reglas
  duplicadas.

### Defecto del registro

Hay dos documentos con `id: r00001`: un refactor terminado y el roadmap
ready. El catálogo resuelve el refactor, el board devuelve cero propuestas
accionables y el roadmap queda invisible.

El roadmap invisible también está obsoleto:

- propone `WorldService.Instance`, patrón eliminado posteriormente;
- exige ocho clubes, pero el pack tiene seis;
- afirma que no queda dominio por inventar;
- estima el juego completo en unas 64 horas sin existir el orquestador;
- las fases posteriores son tablas narrativas, no slices parseables;
- mezcla trabajo ya reemplazado con trabajo aún ausente.

## Cartera de reparación, en orden de dependencia

### 1. Sanear registro, gates y roadmap

Objetivo: recuperar una cola MCP fiable antes de modificar más runtime.

Aceptación mínima:

- ID único para el roadmap y board/catalog sin colisiones;
- retirar pasos obsoletos y convertir fases reales en propuestas parseables;
- gate MCP combinado para propuesta, .NET, arquitectura, Godot y smoke cuando
  aplique;
- CI Godot headless y comparación ejecutable de budgets;
- regla de cierre: una capacidad de producto requiere ruta desde
  `Main.tscn`, efecto observable, persistencia y smoke, no sólo una clase.

### 2. Corregir identidad y contexto gestionado

Objetivo: eliminar fallos visibles antes del nuevo motor.

Aceptación mínima:

- players, managers y staff son proyecciones distintas;
- managers nunca aparecen en plantilla, táctica o mercado;
- `ManagedClubContext` y `BrowsedClubContext` explícitos;
- táctica, competiciones, finanzas y mercado no pueden mutar rivales;
- país y edad llegan de datos/fecha de carrera, sin heurísticas UI.

### 3. Unificar carga de mundo y carrera externa

Objetivo: establecer una fuente de verdad sustituible.

Aceptación mínima:

- `WorldLoader` de Application usa reader, validator y compiler;
- snapshot inmutable indexado por StableId y hash;
- fuente integrada o externa JSON/CSV/SQLite seleccionable;
- save por defecto en `user://`, con override/importación;
- manifest único con versión, seed y hashes de packs;
- inicialización/reset/migración de carrera atómicos y recuperables;
- selector de múltiples carreras.

### 4. Construir el orquestador temporal

Objetivo: un único dueño de fecha y mutaciones.

Aceptación mínima:

- `AdvanceWorld` incluye versión, intervalo, rules hash, seed raíz,
  fidelidad, correlación y presupuesto;
- agenda y semillas estables sin reloj de pared;
- hechos ordenados aplicados en unidad atómica;
- checkpoints reanudables;
- partidos, competición, economía, desarrollo, lesiones, mercado y cantera se
  registran como unidades/cadencias, no llamadas entre pantallas.

### 5. Integrar FootballPack, alineación y competición

Objetivo: que decisiones futbolísticas cambien el partido y la temporada.

Aceptación mínima:

- Application recibe políticas del pack y no codifica 90 minutos o 3/1/0;
- snapshot usa plantilla, titulares, banquillo, posiciones, táctica, forma,
  físico, mental, química y motivación;
- goles, tarjetas, lesiones, sustituciones y ratings tienen actor persona;
- alineaciones inválidas se rechazan;
- formatos declarativos gobiernan liga/copa, calendario, clasificación,
  ascensos, descensos y premios;
- replay y save/load reproducen los mismos hechos.

### 6. Hacer el starter world realmente jugable

Objetivo: contenido mínimo coherente antes de escalar.

Aceptación mínima:

- al menos una liga y segunda división completas;
- 22–25 futbolistas por club, managers y staff separados;
- posiciones, contratos, calendario y reglas consistentes;
- origen/licencia/manifest verificables;
- pack externo alternativo que produzca el mismo modelo compilado;
- ninguna pantalla depende de nombres o IDs concretos del starter.

### 7. Completar economía y mercado autoritativos

Objetivo: conservar dinero y ownership.

Aceptación mínima:

- transferencia atómica debita comprador, acredita vendedor y cambia roster;
- todas las plantillas proyectan el mismo estado;
- oferta editable, contraoferta, salario, contrato, cláusulas, venta y cesión;
- macro market y reemplazos;
- inflación y cierre financiero anual;
- UI async sobre read models versionados.

### 8. Conectar profundidad del manager

Abrir propuestas separadas, cada una con Application, persistencia, Godot,
efecto de simulación y benchmark:

1. ficha/knowledge/scouting;
2. entrenamiento/química/desarrollo/lesiones;
3. staff/cantera/instalaciones;
4. carrera del manager, relaciones, contratos y cambios de club.

### 9. Rehacer el workspace como ciclo de decisiones clásico y original

Objetivo: usabilidad de manager, no imitación visual.

Aceptación mínima:

- cabecera con fecha, club, próxima cita y acciones urgentes;
- máximo siete áreas agrupadas, estado activo, teclado/mando y `Esc`;
- agenda/inbox como siguiente decisión;
- resumen → detalle → razones/historial;
- filas interactivas y drill-down real;
- crest, kit y retrato originales/generados;
- estados vacío/cargando/disponible/obsoleto/error;
- componentes reutilizables y pantallas sin reglas.

### 10. Cerrar escala, distribución y rendimiento

Objetivo: convertir el vertical slice en producto entregable.

Aceptación mínima:

- los benchmarks obligatorios exactos pasan por los mismos puertos del juego;
- mercado 6.400 clubes, 160.000 personas, 320 ligas y 30 años;
- memoria y tamaño de save presupuestados;
- cold start, SQLite, filtros y navegación Godot medidos;
- operaciones largas async/checkpoint sin bloquear input;
- presets y exports autocontenidos Windows/Linux;
- smoke desde instalación limpia sin SDK ni editor preinstalados.

## Regla de ejecución posterior

No se deben borrar los buenos ladrillos ni reescribir todo. Cada propuesta de
reparación debe:

1. declarar qué implementación paralela reemplaza;
2. conectar primero el puerto productivo;
3. conservar tests válidos y añadir evidencia end-to-end;
4. eliminar el camino antiguo en el mismo slice o en uno dependiente;
5. medir la ruta real;
6. obtener revisión independiente;
7. publicar un commit pequeño y trazable.

Hasta completar los puntos 1–5, ampliar Scouting, Training o Facilities sólo
añadiría más islas. El siguiente trabajo debe reparar el registro y la columna
vertebral del runtime antes de sumar nuevas pantallas.
