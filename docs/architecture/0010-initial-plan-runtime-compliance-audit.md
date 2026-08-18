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
| Mundo navegable | Seis clubes y cuatro competiciones sin divisiones completas, calendario ni escala mundial | parcial |
| Plantilla | Tabla visible, pero incluye managers como si fueran jugadores | defectuoso |
| Ficha profunda y conocimiento | Existen escena/controlador y dominio de Knowledge, pero las filas de plantilla no abren la ficha | isla |
| Alineación y táctica | Persiste una selección y un texto libre; admite un participante, no modela once/banquillo/posición y no afecta al partido | parcial |
| Partido | Marcador, timeline y replay; usa prestigio, afición y tamaño de plantilla, no futbolistas ni táctica | parcial |
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
| Distribución | Launcher local robusto y de instancia única; requiere SDK/runtime y Godot Mono instalados | ausente |

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

## Trazabilidad de las propuestas cerradas

La tabla siguiente no reabre el alcance histórico de cada slice: contrasta su
aceptación demostrada con el consumidor actual. Todos los IDs que están en
`done` quedan asignados a una fila. «Verde» en la columna de evidencia significa
que la prueba del slice sigue pasando; el estado final exige además un camino
productivo desde Godot.

| Propuestas `done` | Aceptación que sí quedó demostrada | Consumidor productivo y evidencia actual | Estado |
| --- | --- | --- | --- |
| `c00001..c00004`, `i00001..i00002` | Contratos/ADRs, calidad .NET, metadatos Godot e higiene de artefactos | CI .NET y `check-repository-hygiene.sh` los consumen; CI aún no ejecuta Godot/SmokeBoot | parcial |
| `f00001..f00002`, `x00001`, `x00003..x00008` | Fundación Godot/.NET, contrato de sport pack, TestSport y ownership de proyectos | Godot usa la fundación; `ISportPack` sólo tiene consumidores de test | parcial |
| `f00003..f00004`, `f00018`, `x00002`, `x00020` | Persona/atributos/knowledge, FootballPack, read models y comando de alineación | Plantilla y táctica Godot usan una proyección paralela; Knowledge y atributos del pack no gobiernan la vista | parcial |
| `f00005..f00007`, `x00009..x00010`, `x00038` | Simulación/replay deterministas, grafos de competición y reglas de Football | El workspace usa `MatchSimulator`/`SeasonSimulator` de Application, no el contest pack ni los grafos declarativos | isla |
| `f00009`, `x00011`, `x00037` | WorldStore SQLite, checksum, migración y aislamiento de resultados de test | La carrera Godot usa cinco stores específicos y no carga `IWorldStore`/`SaveMigrator` | isla |
| `f00010`, `f00014..f00015`, `x00012..x00013`, `x00017..x00018`, `x00030..x00031`, `x00036` | Lectores JSON/CSV/SQLite, validator/compiler, contratos, fixtures y starter versionado | Godot sólo usa el lector JSON sobre rutas fijas y omite validator/compiler; el smoke de contenido no prueba el ejecutable | parcial |
| `f00011`, `x00014` | Hechos históricos, checkpoints y queries | El historial visible usa contratos/tablas específicos de carrera, no este world history | isla |
| `f00012..f00013`, `f00016`, `x00015..x00016`, `x00019` | Protocolo determinista, fidelidades y macro advancement testeados | Ningún `AdvanceWorld` ni orquestador es alcanzable desde Godot | isla |
| `f00017`, `f00027..f00029`, `x00039..x00040` | Shell, design tokens, alineación, finanzas y compra persistentes y visibles | Son alcanzables y SmokeBoot cubre el happy path; táctica no afecta al partido y el mercado no conserva ownership/dinero vendedor | parcial |
| `f00019`, `x00021..x00022` | Carrera de persona, development, retirement e injury engines con tests | Assemblies no referenciados por Application/Godot; no hay calendario que los dispare | isla |
| `f00020`, `x00023..x00024` | Economía, valoración, negociación y mercado aislados | Godot usa un servicio simplificado de Application; el motor Market/Economy original no participa | isla |
| `f00021`, `x00025..x00026` | Instalaciones, cantera, staff y evolución de club aislados | `PCDynasty.Club` no tiene consumidor productivo | isla |
| `f00022` | Scouting, assignments y knowledge projection testeados | Botón Scouting deshabilitado y módulo no conectado | isla |
| `f00023` | Training, chemistry y motivation testeados | No hay caso de uso, persistencia, pantalla ni efecto en partido | isla |
| `f00024`, `x00027` | Reputación, relaciones, contratos y ofertas del manager | `PCDynasty.Career` no tiene consumidor productivo | isla |
| `f00025`, `x00028..x00029` | Documento/editor/validación y escena Godot construibles | La opción de editor está deshabilitada; no hay autoría/exportación/importación accesible | isla |
| `f00026` | Pack ficticio con IDs/referencias/licencias y tests de integración | Es la fuente visible, pero sólo contiene 6 clubes y 11 jugadores; no alcanza la aceptación de una liga jugable | parcial |
| `r00001..r00006` | Reducción de tamaños, inversión de puertos, sesión por carrera, contratos de persistencia y simulación fuera de Godot | Mejoran el grafo, pero dejan estado en Presentation, reglas duplicadas en Application y módulos canónicos desconectados | parcial |
| `v00001..v00003`, `x00032..x00035` | Harness BDN, budgets y gates rápidos para fixtures pequeños | Las suites pasan; no ejercitan todas las cargas obligatorias ni varias rutas productivas/SQLite | parcial |

Esta trazabilidad explica por qué no se pueden usar los estados de la propuesta
como porcentaje de producto terminado. Las filas «isla» conservan valor como
tests de diseño, pero su reparación debe integrar o retirar la implementación
paralela; crear una tercera variante queda prohibido.

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

Cada etapa nombra sus prerequisitos y su puerta de salida. Los umbrales de
rendimiento usan el hardware de referencia de
`game/benchmarks/interaction-budgets.json`; sólo pueden cambiar mediante ADR y
una línea base publicada, nunca para hacer verde una regresión.

### 1. Sanear registro, roadmap y gates

**Prerequisito:** esta auditoría aprobada.

**Entrega:** dar ID único al roadmap, retirar su `WorldService.Instance` y sus
fases obsoletas, y convertir esta cartera en propuestas parseables. El gate de
producto debe ejecutar MCP, solución Release, arquitectura, Godot headless y
SmokeBoot cuando una slice declare integración visible. CI debe instalar Godot
y comparar, no sólo publicar, todos los budgets.

**Gate:** catálogo y board resuelven exactamente una propuesta por ID; prueba
negativa hace fallar una propuesta sin ruta `Main.tscn` cuando declara una
capacidad visible; el workflow completo pasa desde checkout limpio y no deja
artefactos trackeados.

### 2. Corregir identidad y autoridad del club gestionado

**Prerequisito:** 1.

**Entrega:** separar read models de player, manager y staff; introducir
`ManagedClubContext` y `BrowsedClubContext`; llevar país y fecha de nacimiento
reales hasta la UI. Táctica, competiciones, finanzas y mercado sólo aceptan el
contexto gestionado para comandos.

**Gate:** fixture con 11 players y 7 managers devuelve exactamente 11 filas en
plantilla/táctica y cero managers en mercado; tests de autoridad rechazan mutar
un rival; edad se calcula con la fecha de carrera en dos cumpleaños límite;
SmokeBoot navega un rival y demuestra que su estado no cambia.

### 3. Establecer el cargador canónico e indexado de mundo

**Prerequisito:** 2.

**Entrega:** un `WorldLoader` de Application compone reader → normalización →
validator → compiler y devuelve snapshot inmutable con StableIds, hash e índices
`byId`/`byClub`/`byCompetition`. Godot elige una fuente integrada o externa
JSON/CSV/SQLite y deja de usar `WorldContentValueReader` tolerante.

**Gate:** contenido inválido falla con diagnóstico y nunca se convierte en
cero; fixtures equivalentes JSON/CSV/SQLite producen el mismo hash; búsquedas
indexadas sobre 160.000 personas son O(1)/O(log n), con mediana menor o igual a
16 ms; cold compile/index del starter queda en 2.000 ms.

### 4. Entregar contenido jugable y autoría/modding de ida y vuelta

**Prerequisito:** 3.

**Entrega:** ampliar el starter a una primera y segunda división de 20 clubes
cada una, 22–25 jugadores por club, managers/staff separados, posiciones,
contratos, equipaciones, estadios, instalaciones, reglas y calendario. Conectar
el editor para personas no programadoras: clubes, personas, equipaciones,
estadios, instalaciones, competiciones por fases, reglas, bases y packs. Import
y export deben usar el mismo pipeline canónico y conservar licencia/manifest.

**Gate:** validator comprueba 40 clubes y entre 880 y 1.000 jugadores, plantillas
válidas y referencias cerradas; round-trip editor → export → import conserva el
hash semántico para cada tipo editable; un pack externo alternativo abre en
Godot sin depender de IDs/nombres del starter; smoke crea, corrige y exporta un
pack inválido.

### 5. Unificar carrera, persistencia externa y transacciones

**Prerequisito:** 3.

**Entrega:** una única familia de contratos de save, manifest con schema/save
version, seed y hashes, y una unidad de trabajo SQLite compartida. Nueva carrera,
reset, migración y commit de hechos son atómicos. Los saves viven por defecto en
`user://`, admiten varios slots, backup, importación de DB externa y override.
Presentation deja de poseer el historial; los puertos de I/O son async y
cancelables.

**Gate:** fault injection en cada escritura deja antes o después completo, nunca
estado mixto; save/load y migración golden conservan hash; dos slots y una DB
externa no se contaminan; un handler Godot completa su parte síncrona en 8 ms sin
I/O; carga de checkpoint de 16 MiB/10.000 hechos queda en 500 ms.

### 6. Construir calendario y orquestador determinista mínimo

**Prerequisitos:** 4 y 5.

**Entrega:** un único `AdvanceWorld` posee fecha, world version, intervalo,
rules hash, seed raíz, fidelidad, correlación y work budget. Primero registra
calendario/competiciones como unidades reales; los sistemas de etapas posteriores
se conectarán mediante el mismo puerto cuando existan, no con stubs que finjan
su integración. La agenda produce checkpoints reanudables y hechos ordenados.

**Gate:** las cinco puertas de ADR 0003 pasan: misma entrada produce mismos
hechos/orden; serie y paralelo coinciden; Macro/Detailed conservan calendario,
contratos y reglas; un fallo deja intacto el snapshot; partido, transición anual
y materialización publican tiempo y memoria. `AdvanceDay`, `AdvanceWeek` y
`AdvanceToDate` sobreviven save/load; cada slice de background cede en 8 ms.

### 7. Integrar FootballPack, alineación y formatos de competición

**Prerequisitos:** 2, 4 y 6.

**Entrega:** Application recibe políticas del pack y elimina 90 minutos, eventos
y 3/1/0 duplicados. El snapshot usa plantilla, once, banquillo, posiciones,
táctica, forma, físico, mental, química y motivación. Liga/copa declarativas
gobiernan calendario, clasificación, ascensos, descensos, premios y sanciones
deportivas. Los hechos nombran personas para goles, tarjetas, lesiones,
sustituciones y ratings.

**Gate:** alineaciones fuera de 11 titulares o con posición inválida se rechazan;
dos tácticas válidas cambian los inputs efectivos sin romper replay; todos los
tipos de evento son alcanzables en fixtures deterministas; liga y copa producen
formatos distintos; replay y save/load conservan exactamente hechos, tabla y
campeón; partido visible queda en 8 ms y temporada de 190 partidos en 50 ms.

### 8. Conectar economía, contratos y mercado autoritativos

**Prerequisitos:** 4, 5, 6 y 7.

**Entrega:** un único hecho de transferencia debita comprador, acredita vendedor,
cambia ownership/rosters y crea contrato. Añadir oferta editable, contraoferta,
salario, cláusulas, venta, cesión, reemplazo macro, inflación, premios, salarios,
mantenimiento, deuda y sanciones financieras. La UI opera sobre read models
versionados y comandos async.

**Gate:** pruebas de conservación de dinero y ownership antes/después, rollback
en cada fallo y proyección idéntica desde ambos clubes; cierre anual reconcilia
ledger a cero; mercado macro sobre 6.400 clubes tiene mediana menor o igual a
250 ms y ninguna interacción bloquea más de 8 ms; SmokeBoot compra, vende,
continúa y verifica ambos rosters/saldos.

### 9. Conectar ficha, knowledge y scouting

**Prerequisitos:** 2, 4, 5 y 6.

**Entrega:** drill-down de jugador, verdad/percepción separadas, visibilidad,
asignaciones, regiones, calidad/carga del scout, progresión y caducidad del
informe. Las decisiones de fichaje sólo consumen knowledge permitido.

**Gate:** tests prueban que dos clubes perciben distinto al mismo jugador y que
la verdad oculta no cruza el contrato UI; save/load conserva observaciones;
query indexada sobre 160.000 personas queda en 16 ms; SmokeBoot asigna scout,
avanza fecha y abre un informe actualizado.

### 10. Conectar entrenamiento, química y ciclo vital de personas

**Prerequisitos:** 4, 5, 6, 7 y 9.

**Entrega:** entrenamiento, desarrollo, desgaste, lesión, recuperación, química,
motivación, LifeEventEngine, jubilación, jóvenes y reconversión posicional son
unidades deterministas de calendario con hechos persistidos y efecto en partido.

**Gate:** 160.000 `DevelopmentStep` quedan en 1.000 ms y son reproducibles en
serie/paralelo; fixtures cubren lesión/recuperación, vida fuera del fútbol,
retirada/relevo y reconversión válida/inválida; un plan de entrenamiento cambia
atributos efectivos y riesgo medidos; SmokeBoot avanza una semana y muestra
causa, cambio e historial sin revelar información oculta.

### 11. Conectar staff, cantera, instalaciones y evolución de club

**Prerequisitos:** 4, 5, 6, 8 y 10.

**Entrega:** empleados envejecen, progresan, se retiran y participan en mercado;
cantera genera/forma/promociona personas; instalaciones y estadio tienen
capacidad, modernidad, deterioro, mantenimiento, obra y cierres; fanbase y
demanda responden a historia, ciudad y rendimiento. Todo afecta economía y
simulación mediante puertos, no llamadas de pantalla.

**Gate:** una simulación de 30 temporadas reemplaza staff sin congelarlo, conserva
presupuesto y produce jóvenes; instalaciones se deterioran y una obra cambia
capacidad/coste sólo tras completarse; demanda nunca excede capacidad vendible;
save/load y rollback conservan hechos; SmokeBoot contrata staff, inicia una obra
y promociona un juvenil.

### 12. Conectar la carrera del manager

**Prerequisitos:** 5, 6, 8 y 11.

**Entrega:** identidad de manager, contrato, reputación, relaciones, objetivos,
ofertas, despido, dimisión, cambio de club y desempleo gobiernan qué club puede
mutarse. Nueva carrera y continuar reconstruyen el mismo contexto.

**Gate:** tests cubren éxito/fracaso de objetivo, oferta, despido, periodo sin
club y firma por otro club sin fuga de estado; save/load conserva reputación y
relaciones; SmokeBoot cambia de club y demuestra que toda acción usa la nueva
autoridad.

### 13. Rehacer el workspace como ciclo de decisiones clásico y original

**Prerequisitos:** 2 y 5–12.

**Entrega:** máximo siete áreas agrupadas, cabecera con fecha/club/próxima cita,
agenda-inbox, estado activo, teclado/mando/Esc, drill-down, formación visual y
componentes reutilizables. Crest, kit, retrato y estadio serán originales; cada
componente representa vacío, loading, disponible, obsoleto y error. Las pantallas
no contienen reglas ni SQL.

**Gate:** test estructural limita pantallas a composición/render y prohíbe
referencias de infraestructura; navegación completa es accesible por teclado y
mando; contraste AA y escalas 100/125/150 % pasan capturas golden; SmokeBoot
recorre las siete áreas, agenda y cinco estados sin bloqueo mayor de 8 ms.

### 14. Completar mundo macro, continuidad e historia de 80 años

**Prerequisitos:** 6–12.

**Entrega:** 320 ligas/6.400 clubes avanzan con fidelidad macro sin congelar
economía, plantillas, empleados ni reglas; clasificación continental y
materialización preservan invariantes. La memoria compacta conserva campeones,
tablas, ascensos, descensos, títulos, fichajes, carreras, récords, grandes
eventos e instalaciones durante 80 años.

**Gate:** Macro/Detailed convergen en invariantes compartidas; 320 cierres de liga
quedan en 1.000 ms, materialización de club en 50 ms y clasificación continental
en 50 ms; benchmark de 30 años queda en 30 s fuera del frame, con slices de 8 ms;
una partida golden de 80 años conserva todos los tipos históricos y ocupa como
máximo 512 MiB en disco y 2 GiB de memoria residente.

### 15. Cerrar rendimiento real y distribución autocontenida

**Prerequisitos:** 1–14. Cada etapa anterior añade su benchmark en su misma
slice; ésta cierra la matriz y el empaquetado, no pospone el perfilado.

**Entrega:** todos los workloads usan los mismos puertos/composition root del
juego. Añadir 10.000 partidos Fast, 160.000 development, mercado 6.400, 320
ligas, 30 años, load/save, materialización, scouting, continental, SQLite,
cold start y navegación Godot. Crear presets Windows/Linux autocontenidos y
launcher que no requiera SDK, editor ni runtime externo.

**Gate:** 10.000 Fast quedan en 1.000 ms; los demás umbrales son los definidos en
3, 5 y 7–14; CI falla si un workload carece de presupuesto o lo supera, y publica
tiempo, allocations, RSS y tamaño de save. Exports limpios Windows/Linux abren
una sola instancia, crean carrera, juegan/guardan/continúan y cargan un pack/DB
externos sin .NET ni Godot preinstalados.

### Cobertura de promesas maestras

| Promesa explícita que antes no tenía destino | Etapa propietaria |
| --- | --- |
| Editor/modding de clubes, personas, equipaciones, estadios, instalaciones, competiciones, reglas, DB y packs | 4 |
| LifeEventEngine, jubilación, jóvenes y reconversión posicional | 10 |
| Evolución, retirada y mercado de empleados | 11 |
| Fanbase, demanda, estadio, mantenimiento y cierres | 11 |
| Deuda, inflación, premios y sanciones financieras | 8 |
| Cambio de club, desempleo y carrera del manager | 12 |
| Macro world, materialización y clasificación continental | 14 |
| Memoria histórica de 80 años | 14 |
| Escala exacta, memoria, save y distribución limpia | 15 |

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
