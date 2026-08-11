# Referencia y presupuestos de rendimiento

## Decisión

La experiencia se gobierna con presupuestos de producto versionados en
`game/benchmarks/interaction-budgets.json`. Las cifras expresan el límite que
percibe el jugador; los benchmarks técnicos conservan además tamaño de fixture,
asignaciones, runtime y metadatos del equipo.

El equipo mínimo de referencia es una CPU x86-64 de cuatro núcleos físicos a
2,9 GHz, 16 GiB de RAM y SSD local, ejecutando .NET 8 x64 en Release sobre
Windows 11 o Ubuntu 22.04 o posterior. No se comparan tiempos absolutos entre
máquinas distintas. Una recalibración exige informe BenchmarkDotNet anterior y
nuevo, motivo escrito y revisión del impacto de producto.

## Clases de latencia

| Clase | Límite | Comportamiento obligatorio |
| --- | ---: | --- |
| comando de frame | 8 ms | sin I/O, sin simulación global y sin bloquear el hilo de UI |
| abrir ficha materializada | 16 ms | responder desde índice/read model versionado |
| materialización local | 50 ms | diferible y descartable si cambia el contexto |
| avance de temporada | 250 ms | resultado atómico; progreso honesto si excede el límite |
| carga normal | 500 ms | publicar el primer checkpoint consistente cuanto antes |
| primera carga mundial | 2.000 ms | progreso por etapas, cancelación segura y ninguna falsa finalización |

El trabajo de fondo recibe tramos máximos de 8 ms. Al terminar cada tramo debe
ceder ejecución y sólo puede publicar un checkpoint consistente. La UI sigue
navegando sobre la última versión confirmada y muestra que hay trabajo pendiente.

## Escenarios y medición

Las fichas de jugador, club y competición se miden con catálogos de tamaño
explícito. La materialización usa un checkpoint de 512 clubes y compromisos
históricos; las cargas incluyen snapshot y hechos; el primer arranque incluye
validación, compilación e indexado del pack externo.

El gate rápido usa cinco muestras después de un calentamiento y compara la
mediana. El benchmark de diagnóstico usa BenchmarkDotNet y registra CPU,
sistema, runtime, JIT, GC y memoria asignada. La primera carga se mide además en
proceso frío e incluye el lanzamiento. Un ruido superior al 20 % invalida la
medición: se repite en un runner estable, no se amplía el presupuesto.

## Consecuencias de diseño

- Application expone read models; Godot no consulta SQLite ni ejecuta reglas.
- Las operaciones largas son explícitas, observables y reanudables mediante
  checkpoints; nunca se disfrazan como datos finales.
- Las cachés se invalidan por versión de hechos y no forman parte de los
  contratos de dominio.
- La base y los packs siguen siendo externos, sustituibles y medidos a través de
  sus interfaces públicas.
- Un cambio que rebasa un límite bloquea la entrega o requiere una decisión de
  arquitectura que explique por qué cambió el producto.
