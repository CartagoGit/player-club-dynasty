# Puertas de arquitectura y secuencia de entrega

## Regla de avance

No se amplía el alcance por entusiasmo. Cada vertical slice debe demostrar una
capacidad del motor y una invariante de arquitectura antes de habilitar el
siguiente. Una pantalla vistosa no sustituye una prueba determinista, un
benchmark ni una migración verificable.

## Estándar permanente de código

Todo código nuevo debe cumplir estas condiciones desde la primera línea:

- **SOLID pragmático:** una responsabilidad por tipo, dependencias hacia
  abstracciones pequeñas, implementaciones sustituibles y sin interfaces
  infladas. No se crean patrones si no reducen un acoplamiento real.
- **Clean Code:** nombres de dominio, funciones breves, validación en bordes,
  errores explícitos, nullability activada, sin código muerto ni comentarios
  que repitan el código.
- **Reutilización con cohesión:** se extrae una abstracción sólo tras detectar
  comportamiento común estable. Se prohíbe duplicar reglas entre UI, Core,
  importadores y simuladores.
- **Contratos externos:** lectores de datos, almacenamiento, packs y Godot
  implementan puertos versionados. Los DTO externos no se convierten en
  entidades del dominio.
- **TDD y determinismo:** primero la prueba que fija un comportamiento;
  después la implementación mínima. Las pruebas no usan reloj real, orden no
  estable ni números aleatorios no controlados.

## Puertas obligatorias por cambio

| Tipo de cambio | Pruebas mínimas antes de cerrar |
| --- | --- |
| regla de dominio | unitarias de invariante y casos límite |
| simulación | repetibilidad de semilla, orden y serie/paralelo |
| pack/importador | esquema, referencias, incompatibilidad y hash reproducible |
| save/migración | carga previa/posterior, rollback y compatibilidad de packs |
| read model | contrato de consulta y ausencia de mutación del dominio |
| hot loop | benchmark de tiempo/memoria con presupuesto documentado |
| integración Godot | adaptador de comando/read model sin referencia Godot en Core |

El pipeline de calidad de .NET incorporará formateo, análisis estático,
compilación con warnings tratados como errores, tests y benchmarks
seleccionados. Ninguna propuesta puede declarar verde una ruta que el entorno
no puede ejecutar; el SDK y editor requeridos se provisionan primero.

## Roadmap incremental

### A. Fundamentos ejecutables

**Entrega:** solución .NET, `Core.Primitives`, Core mínimo, `TestSport` y
adaptadores vacíos.

**Demuestra:** Core sin Godot ni fútbol, IDs compactos, contrato de errores y
tests que se ejecutan sin abrir el motor.

**No permite aún:** simulación global, UI extensa, base de datos real ni packs
de comunidad.

### B. Vertical slice de persona y conocimiento

**Entrega:** persona, atributos compilados, observaciones, confianza y read
model de ficha de jugador; FootballPack mínimo sólo en un adaptador de pack.

**Demuestra:** técnica, forma, físico, mental y media como derivados, no como
campos duplicados; el conocimiento del club puede ser incierto.

**Puerta:** TestSport sigue funcionando y ninguna regla de fútbol vive en
Core.

### C. Partido instantáneo reproducible

**Entrega:** convocatoria, snapshot, simulación de contest, timeline y
aplicación de efectos de rendimiento.

**Demuestra:** mismo seed/mismo input/mismo resultado, y UI que sólo consume
read models.

**Puerta:** benchmark de partidos y pruebas de invariantes de plantillas,
lesiones y orden de hechos.

### D. Un ecosistema pequeño completo

**Entrega:** un país, dos divisiones, una temporada, ascensos/descensos y
transición macro.

**Demuestra:** la competición es un grafo de fases/reglas, no una clase League
codificada; Macro y Detailed preservan los mismos compromisos.

**Puerta:** miles de temporadas de prueba, migración de un save y carga desde
un pack externo validado.

### E. Escala y persistencia

**Entrega:** persistencia de hechos, checkpoints, importador SQLite/CSV/JSON,
materialización lazy, historia y benchmarks de escala.

**Demuestra:** avanzar varias décadas sin simular partidos innecesarios ni
perder coherencia histórica.

**Puerta:** presupuestos de memoria, tiempo de transición anual, carga/save y
recuperación de migración aprobados antes de ampliar países.

### F. Sistemas profundos

**Entrega:** mercado, economía, cantera, instalaciones, staff, lesiones y
scouting avanzado, uno por propuesta y con sus tests de estabilidad.

**Demuestra:** historias emergentes sin convertir el motor en una colección de
scripts de UI.

### G. Herramientas de contenido

**Entrega:** editores de pack y validadores; nunca acceso directo y mutable a
la partida desde UI.

**Demuestra:** comunidad capaz de cargar contenido con compatibilidad,
seguridad y diagnósticos.

## Criterios para detener y rediseñar

Se abre una propuesta de corrección antes de continuar si ocurre cualquiera de
estas señales:

- Core necesita una referencia a Godot, JSON, SQL o un término de fútbol;
- dos sistemas calculan la misma regla de maneras distintas;
- una decisión depende del orden de concurrencia o de una semilla global
  consumida;
- no se puede reproducir un bug a partir de save, reglas, seed y correlación;
- una migración modifica un save sin copia recuperable;
- un benchmark empeora el presupuesto acordado sin explicación;
- un pack puede ejecutar código, acceder a archivos arbitrarios o alterar una
  partida sin compatibilidad explícita.

## Próxima implementación autorizada

La siguiente propuesta de código debe ser el fundamento ejecutable de la fase
A. Su aceptación incluye instalar/provisionar el SDK .NET LTS y la edición
.NET de Godot, porque el entorno actual no tiene `dotnet` y no puede validar
un Core C# todavía.
