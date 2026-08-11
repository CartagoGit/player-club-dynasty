# ADR 0001 — Godot y .NET como base del juego

## Estado

Aceptada para el primer vertical slice.

## Decisión

PC Dynasty usará **Godot 4 con soporte .NET/C#** para presentación y
**.NET/C# sin referencias a Godot** para el dominio y la simulación.

La distribución inicial se limita a escritorio: Windows, macOS y Linux. El
objetivo no incluye navegador ni móvil en el primer vertical slice.

## Motivos

El juego es, ante todo, una simulación de datos, reglas y generaciones de
historia. C# permite ejecutar, perfilar y probar ese núcleo desde la línea de
comandos; Godot aporta la UI de escritorio, escenas, input, gráficos 2D,
audio y herramientas de edición sin convertir cada entidad del mundo en un
objeto del motor.

Godot encaja especialmente bien con la interfaz 2D, de navegación rápida y
profundidad opcional que pide el plan. Unity sería una alternativa razonable
si el producto cambiara hacia 3D complejo, certificación de consola como
prioridad inmediata o una dependencia crítica de su ecosistema. Tauri y
Electron no son la base adecuada: resuelven una aplicación web de escritorio,
no las necesidades de input, escenas, renderizado y empaquetado de un juego.

## Límites de dependencia

```text
Godot presentation  ─────► application/read models ─────► Core
Godot presentation  ─────► assets and UI state

Core ─────► ninguna referencia a Godot, escenas, nodos o assets
```

- `PCDynasty.Core` contiene IDs, datos de dominio, reglas puras y simulación.
- Los hot loops usan IDs numéricos, arrays y estructuras compactas; los packs
  resuelven sus nombres externos durante la carga.
- La presentación consume comandos y read models. No guarda la fuente de
  verdad del mundo ni ejecuta reglas de negocio propias.
- Ninguna `Person`, club, competición o partido del Core es un `Godot.Node`.
- El primer test de independencia será `TestSport`: un deporte artificial sin
  posiciones, clubes, goles ni banquillo debe poder usar el Core.

## Arquitectura del motor del manager

La simulación se organiza en cuatro planos con contratos explícitos. No se
trata de una jerarquía de clases que reproduzca la interfaz: cada plano tiene
una frecuencia, un coste y una fuente de verdad diferentes.

```text
packs versionados ─► compilación/validación ─► reglas y catálogos inmutables
                                                   │
hechos persistidos ─► World State ─► Simulation Orchestrator ─► nuevos hechos
                              │                 │
                              │                 ├─ partidos detallados
                              │                 ├─ procesos semanales
                              │                 └─ transiciones macro
                              ▼
                         read models ─────────────────────────────► Godot UI
```

### 1. Definiciones, no estado vivo

Los packs describen deportes, atributos, competiciones, reglas y contenido.
Al cargar, sus claves humanas se validan y compilan a identificadores
numéricos. Un pack no puede cambiar silenciosamente una partida: cada save
guarda las versiones y hashes de los packs que determinaron sus reglas.

### 2. Estado mundial y hechos

El estado contiene sólo la información necesaria para continuar una partida.
Contratos, lesiones, resultados, cambios de club, cambios de regla y otros
eventos con consecuencias históricas se registran como hechos. Las medias,
rankings, proyecciones y tarjetas de UI se derivan y pueden reconstruirse.

Esta separación evita saves inflados, permite migraciones y hace auditables
las decisiones del motor.

### 3. Orquestador de simulación

`SimulationOrchestrator` será el único punto que avanza el tiempo. Recibe un
estado, un intervalo, reglas compiladas y una semilla; devuelve hechos y el
estado siguiente. Los subsistemas no se llaman entre sí de manera circular ni
avanzan el calendario por su cuenta.

La frecuencia se fija por contrato:

| Cadencia | Responsabilidad |
| --- | --- |
| partido | participantes, fatiga, forma, lesiones y timeline |
| semanal/mensual | entrenamiento, recuperación y desarrollo fino de ligas activas |
| ventana/temporada | economía, contratos, cantera, mercado y evolución macro |
| bajo demanda | materialización de una competición o una persona no activa |

Un mismo input y una misma semilla deben producir los mismos hechos. La UI no
puede consultar aleatoriedad ni modificar el estado durante la simulación.

### 4. Presentación y comandos

Godot muestra read models inmutables y envía comandos validados, por ejemplo
`SelectLineup` o `AdvanceToDate`. La capa de aplicación traduce el comando a
una operación del Core y publica el nuevo read model. Las escenas nunca
resuelven mercado, clasificación, evolución de atributos ni conocimiento de
scouting.

## Reglas de diseño no negociables

1. El Core usa tipos de dominio e IDs, no strings en bucles calientes ni
   `if (sport == Football)`.
2. El flujo de dependencias siempre apunta hacia el centro: presentación y
   persistencia implementan puertos del Core, nunca al revés.
3. El motor no modifica datos compartidos desde tareas paralelas. Primero se
   paralelizarán cálculos puros y se aplicarán los hechos en un orden
   determinista.
4. Cada transición valida invariantes antes de persistir: referencias,
   calendario, reglas versionadas y contabilidad básica.
5. Un cambio de save, pack o regla necesita versión y migración explícitas.
6. Los benchmarks ejercitan los mismos puertos que el juego; no habrá una
   implementación rápida distinta de la implementación real.
7. `TestSport` protege el Core contra contaminación de fútbol y debe formar
   parte de la puerta de calidad antes de ampliar el FootballPack.

## Secuencia de construcción

1. Validar esta frontera con un Core mínimo, `TestSport` y tests deterministas.
2. Añadir un FootballPack mínimo y el read model de una ficha de jugador.
3. Incorporar el primer orquestador de partido instantáneo y su replay
   reproducible.
4. Sólo entonces diseñar competiciones, transición macro, persistencia y
   mercado como propuestas separadas con benchmarks.

## Estructura que se introducirá gradualmente

```text
game/
  src/PCDynasty.Core/       # dominio y simulación sin Godot
  tests/PCDynasty.Core.Tests/
  godot/                    # proyecto, escenas y adaptadores Godot
```

No se crearán todavía proyectos para economía, mercado, mundo, persistencia o
packs. Aparecerán cuando un vertical slice demuestre una necesidad concreta.

## Prerrequisitos de desarrollo

- SDK de .NET LTS compatible con el Godot .NET elegido.
- Editor y plantillas de exportación de la edición **.NET** de Godot, no la
  edición estándar.
- Un runner de tests .NET para verificar el Core sin abrir Godot.

Este entorno aún no tiene `dotnet`, por lo que el siguiente slice debe
instalar o provisionar el SDK antes de poder validar compilación y tests.

## Riesgos y contingencia

Los proyectos C# de Godot tienen soporte de escritorio; el soporte móvil
requiere atención adicional y las exportaciones web no están disponibles para
proyectos C# en Godot 4. Si una versión de navegador se convierte en requisito
de producto, se abrirá una propuesta específica para evaluar un cliente web
separado, una capa de presentación alternativa o una revisión del motor. No
se introducirá esa complejidad antes de que exista el vertical slice de
escritorio.

## Referencias

- [Godot: C#/.NET](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [Godot: requisitos del sistema](https://docs.godotengine.org/en/stable/about/system_requirements.html)
