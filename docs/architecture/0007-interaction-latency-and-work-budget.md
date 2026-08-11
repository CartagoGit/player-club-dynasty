# Presupuesto de interacción y trabajo de fondo

## Principio

El jugador no debe notar que el juego calcula el mundo. La navegación, la
selección y las consultas habituales responden con un presupuesto corto y
predecible; el trabajo que lo excede se planifica como avance visible pero no
bloqueante, con checkpoints reanudables.

## Clases de trabajo

| Clase | Ejemplos | Política |
| --- | --- | --- |
| inmediata | cambiar pestaña, abrir ficha ya materializada, filtrar lista | ejecuta dentro del presupuesto de frame/interacción; no inicia simulación global |
| diferida corta | materializar un historial local, calcular un read model costoso | se cancela si cambia el contexto y publica resultado sólo si sigue vigente |
| avance explícito | simular partido, avanzar día/semana/temporada | agenda unidades deterministas, muestra progreso honesto y acepta checkpoints |
| preparación | índices, cachés, importación, benchmark | se ejecuta antes de necesitarse o en segundo plano; nunca falsea una proyección final |

Los presupuestos exactos se versionan en los benchmarks sobre hardware de
referencia. La regla de UX es absoluta: si una operación no puede ser
inmediata, su estado debe ser explícito, cancelable cuando sea seguro y no debe
impedir navegar por información ya disponible.

## Contrato de ejecución

Cada trabajo tiene `WorkId`, causa, prioridad, presupuesto, estado y versión
del read model que lo solicitó. El resultado se aplica sólo si la versión/causa
siguen vigentes. Un resultado obsoleto se descarta, no se mezcla con la UI
actual.

El motor puede partir trabajo en unidades deterministas y publicar checkpoints
atómicos. UI y Godot nunca observan una mutación parcial del mundo; reciben el
último checkpoint consistente y una indicación de que existe trabajo pendiente.

## Estrategias permitidas

- índices y read models cacheados con invalidación por versión de hechos;
- materialización lazy desde compromisos históricos;
- prefetch limitado de la navegación probable, sin alterar reglas;
- procesamiento paralelo de cálculos puros y aplicación ordenada posterior;
- degradación de detalle de presentación, nunca de corrección de hechos.

## Estrategias prohibidas

- bloquear el hilo de UI esperando una transición anual o una consulta SQL;
- mostrar resultados estimados como hechos confirmados;
- recalcular todo el mundo al abrir una ficha;
- cachear datos sin versión o invalidación;
- ocultar una operación larga tras una pantalla congelada.

## Verificación

Cada flujo frecuente tiene benchmark de latencia, asignaciones y memoria. Las
pruebas validan cancelación, descarte de resultados obsoletos, reanudación desde
checkpoint y que un trabajo de fondo no modifica directamente el estado visible
sin pasar por Application.
