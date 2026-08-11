# Orquestación determinista de la simulación

## Objetivo

Avanzar una partida debe ser reproducible, auditable y rápido. El motor no
simula «cada cosa cada día»: planifica trabajo según relevancia y frecuencia,
pero conserva el mismo resultado para la misma partida, reglas y semilla.

## Único punto de avance

`SimulationOrchestrator` es el único servicio autorizado a modificar el
estado temporal del mundo. Recibe un `AdvanceWorld` con:

- `worldId` y versión esperada del estado;
- intervalo semiabierto `[from, to)`;
- hash del catálogo de packs/reglas compiladas;
- semilla raíz de la partida;
- modo de fidelidad solicitado (`Detailed`, `Macro` o `Materialize`);
- ID de correlación y presupuesto de trabajo.

Devuelve un lote ordenado de hechos, métricas y un checkpoint. No expone
entidades mutables ni permite que Godot llame a subsistemas de forma directa.

## Pipeline de avance

```text
validar comando y versión
  → cargar snapshot + hechos posteriores
  → construir agenda determinista
  → resolver unidades independientes
  → ordenar y validar hechos candidatos
  → aplicar lote atómico
  → actualizar proyecciones/checkpoint
  → publicar resultado y telemetría
```

### Agenda determinista

El orquestador compila fechas, competiciones activas, plazos de contrato y
trabajos lazy en una agenda. Cada unidad tiene una clave de orden estable:

`(fecha efectiva, prioridad, tipo, entidad primaria, correlación local)`.

El orden nunca depende de la iteración de un diccionario, del reloj de pared
ni del orden de llegada de tareas en paralelo.

### Semillas y aleatoriedad

La semilla raíz no se consume secuencialmente. Cada unidad deriva la suya con
una función estable de `worldSeed`, versión de reglas, fecha, tipo de proceso e
IDs relevantes. Así, insertar una simulación de otra competición no altera un
partido ya existente.

La aleatoriedad sólo vive dentro de servicios de simulación explícitos. Los
read models, UI, repositorios y cargadores de packs no generan azar.

### Paralelismo seguro

Se pueden calcular en paralelo unidades sin dependencias —por ejemplo,
partidos sin participantes compartidos—, pero cada una produce una lista
inmutable de hechos candidatos. La aplicación ocurre en la secuencia
determinista de la agenda, después de comprobar versiones e invariantes.

Un conflicto de escritura no se resuelve «por última llegada»: se rechaza el
lote, se replanifica desde el checkpoint o se divide la unidad según una regla
documentada. El estado del mundo no se muta desde hilos de UI.

## Niveles de fidelidad

| Nivel | Uso | Salida mínima |
| --- | --- | --- |
| Detailed | club/competición bajo seguimiento | contest snapshot, eventos, carga, forma e historia relevante |
| Macro | mundo no observado | resultados estructurales, estadísticas anuales y hechos de impacto |
| Materialize | el usuario abre una entidad antes resumida | estado coherente derivado de sus compromisos e historial |

Los tres niveles comparten reglas, semillas y el pipeline de efectos:
`Performance → Form / Reputation / Development / Market`. Sólo difiere el
coste y la granularidad. Ningún resultado Macro es una ruta de reglas ajena que
pueda contradecir los hechos Detailed.

## Cadencias y ownership

- **Partido:** resuelve participantes, acciones, contribuciones, lesiones,
  forma y timeline de un contest.
- **Semanal/mensual:** actualiza entrenamiento, recuperación y desarrollo fino
  únicamente para las entidades activas.
- **Ventana/temporada:** resuelve economía, mercado, contratos, cantera,
  staff, reglas activas y estado anual de clubes fuera de foco.
- **Bajo demanda:** adelanta una persona, club o competición desde su último
  checkpoint hasta la fecha consultada sin cambiar el pasado confirmado.

Cada proceso declara qué agregados lee y qué hechos puede emitir. Si dos
procesos necesitan actualizar el mismo agregado, el orquestador impone una
relación de dependencia explícita en la agenda.

## Fallos, recuperación y límites

Un avance se persiste como transacción con `expectedVersion`. Si falla una
validación, no se guarda una partida a medio avanzar. El resultado devuelve un
error tipado con correlación, la unidad fallida y el checkpoint recuperable;
la UI puede mostrar un mensaje sin inventar una recuperación.

El presupuesto de trabajo evita bloqueos perceptibles. El orquestador puede
emitir un checkpoint y reanudar exactamente desde él, pero nunca publica una
proyección parcialmente aplicada como si fuera definitiva.

## Observabilidad y pruebas

Cada avance registra duración, entidades procesadas, semilla derivada, nivel
de fidelidad, hash de reglas, número de hechos y causa de reintento. No se
registran secretos ni objetos completos de partida.

Las puertas obligatorias son:

1. mismo snapshot/comando/semilla → mismos hechos en el mismo orden;
2. ejecutar unidades independientes en serie o paralelo → mismo resultado;
3. Macro y Detailed respetan los mismos invariantes de calendario, contratos y
   reglas;
4. un fallo durante el lote deja el snapshot previo intacto;
5. benchmarks miden coste y memoria de partido, transición anual y
   materialización antes de aumentar escala.
