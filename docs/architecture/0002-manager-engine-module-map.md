# Arquitectura del motor del manager — módulos y contratos

## Propósito

Este mapa fija las fronteras del motor antes de crear proyectos y clases. Su
objetivo es que el mundo pueda crecer durante décadas sin acoplar la
simulación a Godot, al fútbol, a una base de datos o a una pantalla concreta.

No es una lista de paquetes obligatorios: un módulo sólo nace cuando el
vertical slice que lo necesita tiene contrato, test y propietario claros.

## Regla de dependencia

```text
Godot UI ─► Application ─► Core contracts ◄─ Infrastructure
             │                 ▲              (save, pack loading)
             ▼                 │
          Read models      FootballPack / TestSport
```

Las flechas señalan dependencias de código. El Core no importa Godot, SQLite,
JSON, un contenedor DI, ni tipos específicos de fútbol. La infraestructura y
los packs dependen de los contratos del Core.

## Capas y módulos iniciales

| Módulo | Responsabilidad | Puede depender de | No puede depender de |
| --- | --- | --- | --- |
| Core.Primitives | IDs, tiempo, semillas, resultados, errores de dominio | BCL mínima | Godot, persistencia, fútbol |
| Core.Domain | estado y reglas independientes de deporte | Core.Primitives | UI, I/O, packs concretos |
| Core.Simulation | transiciones puras y aplicación ordenada de hechos | Core.Domain | Godot, repositorios concretos |
| Pack.Abstractions | contratos de deporte, competición y contenido | Core contracts | Godot, formatos de archivo |
| FootballPack | definiciones y algoritmos de fútbol | Core y Pack.Abstractions | Godot, SQL |
| TestSport | deporte artificial para tests | Core y Pack.Abstractions | FootballPack |
| Application | casos de uso, comandos, consultas y read models | Core, puertos | escenas, SQL, formatos |
| Infrastructure | adaptadores de save, packs y telemetría | Application/Core puertos | lógica de negocio duplicada |
| Godot.Presentation | escenas, input, navegación y renderizado | Application contratos | reglas de dominio y almacenamiento |

`Core.Domain` y `Core.Simulation` pueden vivir inicialmente en un mismo
ensamblado si el límite se mantiene mediante namespaces y tests. Separarlos en
ensamblados sólo se justificará cuando reduzca dependencias o tiempo de build.

## Tipos de contrato

### Comandos

Un comando expresa una intención del usuario o del sistema, nunca una
mutación parcial de UI. Ejemplos: seleccionar convocatoria, ofrecer contrato,
avanzar a una fecha. Lleva un identificador de partida, el actor, los datos
validados en el borde y un ID de correlación.

El caso de uso devuelve un resultado explícito: éxito con hechos y read model
actualizado, o rechazo con un código de dominio. No se usan excepciones para
errores normales de reglas.

### Hechos

Un hecho registra algo que ocurrió y no se reescribe: un contrato firmado,
un resultado confirmado, una lesión o una regla activada. Cada hecho contiene
ID estable, fecha efectiva, causalidad, correlación, versión de esquema y
origen. Los hechos son la entrada de auditoría, migración e historia.

### Estado y proyecciones

El estado de la partida se reconstruye a partir de un snapshot validado más
hechos posteriores. Las proyecciones están optimizadas para consultas, no son
fuente de verdad: ficha de jugador, tabla de liga, estimación de scouting y
tarjeta de club se recalculan cuando el estado o el conocimiento cambia.

### Puertos

El Core define interfaces pequeñas para lo que no controla, por ejemplo:

- `IWorldStore`: carga un snapshot consistente y persiste un lote atómico de
  hechos/snapshot.
- `IPackCatalog`: entrega packs ya validados y compilados, identificados por
  versión y hash.
- `IClock` únicamente para bordes no deterministas; la simulación recibe fechas
  como valor.
- `IExecutionObserver`: métricas y trazas, sin permitir alterar decisiones.

No existirá un repositorio genérico que exponga entidades mutables ni un
service locator dentro del dominio.

## Límites del dominio

El Core representa `Person`, `Organization`, `Participant`, `Contest`,
`Competition`, `Rule`, `Contract`, `Observation` y sus relaciones mediante
IDs. Sólo los packs aportan el significado de gol, portería, formación,
tarjeta, posición, ranking específico o regla futbolística.

La API de Core se diseña en términos de capacidades, acciones, contribuciones,
participantes y resultados genéricos. Cualquier condición repetida basada en
un deporte es deuda arquitectónica y requiere una propuesta de corrección.

## Datos calientes y fríos

- **Calientes:** IDs, atributos compilados, calendario activo, estado de
  partido, presupuestos resumidos y datos necesarios en bucles de simulación.
  Se almacenan de forma compacta y se procesan sin strings dinámicos.
- **Fríos:** biografía, narrativa, observaciones, historial detallado,
  metadatos de pack y texto traducible. Se cargan bajo demanda o por índice.

Un agregado no es necesariamente un objeto pesado: su representación puede
ser data-oriented mientras sus invariantes se mantengan en una única frontera
de escritura.

## Anticorrupción y estabilidad

- El cargador convierte formatos de pack externos en definiciones compiladas;
  el Core no deserializa JSON de mods.
- El adaptador de save convierte filas/binarios en snapshots y hechos; el
  dominio no conoce SQL ni rutas de archivo.
- Los adaptadores Godot convierten comandos y read models; escenas y nodos no
  se filtran hacia Application.
- Una interfaz pública se versiona de forma semántica. Los cambios que rompan
  packs o saves exigen migración y propuesta específica.

## Criterios de aceptación de cualquier módulo nuevo

1. Tiene una única responsabilidad y un propietario de datos inequívoco.
2. Declara dependencias permitidas y tests de invariantes.
3. No introduce una referencia desde Core hacia presentación o infraestructura.
4. Añade una métrica o benchmark si participa en una ruta de simulación.
5. Conserva la ejecución de TestSport sin importar FootballPack.
