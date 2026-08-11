# PC Dynasty — Especificación Maestra del Proyecto

> **Nombre de trabajo:** PC Dynasty  
> **Significado de PC:** Player Club  
> **Concepto:** Player Club Dynasty  
> **Estado:** documento maestro de visión, producto, dominio, arquitectura, simulación y desarrollo.  
> **Destinatario:** agente de desarrollo que recibirá esta conversación y trabajará sobre el proyecto local existente.

---

# 0. Propósito de este documento

Este documento recoge y organiza la visión desarrollada durante toda la conversación para convertirla en una base de trabajo real.

No pretende ser una especificación de clases definitiva ni una orden de implementar literalmente cada concepto de la forma aquí escrita. Su propósito es conservar:

- intención de producto;
- principios no negociables;
- comportamiento esperado;
- profundidad deseada;
- necesidades de simulación;
- restricciones de rendimiento;
- extensibilidad futura;
- filosofía de interfaz;
- modelo económico y humano;
- relación entre datos reales, observados y estimados;
- estrategia de contenidos y mods;
- integración obligatoria con `mcp-vertex`.

El agente que reciba este documento debe analizarlo, contrastarlo con el scaffold real del repositorio y convertirlo en propuestas técnicas implementables.

---

# 1. Instrucciones obligatorias sobre `mcp-vertex`

El proyecto local contiene un sistema/proyecto llamado **`mcp-vertex`**.

`mcp-vertex` forma parte del flujo de trabajo previsto y debe utilizarse.

## 1.1. Primera tarea del agente

Antes de diseñar arquitectura, generar módulos grandes o modificar el scaffold, el agente debe:

1. inspeccionar la raíz del repositorio;
2. localizar la documentación de `mcp-vertex`;
3. leer README, docs, ejemplos, templates, convenciones, scaffolds, proposals, ADRs y cualquier otro recurso local relevante;
4. identificar cómo espera `mcp-vertex` que se creen propuestas, módulos, agentes, herramientas, memorias, contextos o flujos de trabajo;
5. considerar la documentación local de `mcp-vertex` como fuente de verdad;
6. no inventar APIs, comandos, nombres de archivos o convenciones de `mcp-vertex`;
7. adaptar el proyecto PC Dynasty al modo de trabajo real de `mcp-vertex`;
8. utilizar las propuestas/mecanismos de `mcp-vertex` para documentar decisiones importantes cuando éste disponga de ellos.

## 1.2. `mcp-vertex` no es opcional

No reemplazar `mcp-vertex` por una herramienta arbitraria simplemente por preferencia del agente.

Si el agente considera que una parte de `mcp-vertex` no encaja, debe:

- explicar el problema;
- demostrarlo con la documentación local;
- proponer una adaptación;
- documentar la decisión.

## 1.3. Propuestas antes de grandes implementaciones

Los grandes sistemas deberán pasar primero por propuestas, preferentemente mediante el mecanismo que `mcp-vertex` ya tenga documentado.

Entre ellos:

- arquitectura general;
- motor de personas;
- motor de conocimiento/scouting;
- motor de competiciones;
- rules engine;
- simulación de partidos;
- simulación macro mundial;
- economía;
- mercado y negociaciones;
- instalaciones;
- cantera;
- persistencia;
- packs/modding;
- rendimiento;
- integración con Godot.

Una propuesta debería cubrir cuando corresponda:

- problema;
- requisitos;
- invariantes;
- alternativas;
- decisión;
- modelo de dominio;
- datos persistentes;
- datos derivados;
- coste de cálculo;
- frecuencia de actualización;
- estrategia de tests;
- benchmarks;
- compatibilidad futura;
- riesgos;
- migraciones.

---

# 2. Visión del producto

**PC Dynasty** debe comenzar como un juego de gestión futbolística mundial inspirado en la inmediatez, facilidad de navegación, profundidad progresiva y capacidad de jugar décadas de los managers clásicos, especialmente la filosofía que hacía atractivo a PC Fútbol.

No se busca copiar PC Fútbol.

No se deben copiar:

- recursos;
- nombres protegidos;
- interfaces exactas;
- bases de datos;
- escudos;
- marcas;
- código;
- assets.

La referencia es conceptual:

> entrar, gestionar, hacer planteamientos, fichar, revisar datos, avanzar rápido, simular partidos casi instantáneamente y vivir una historia deportiva durante décadas.

La diferencia fundamental es que PC Dynasty debe construirse sobre un **núcleo extensible**.

La primera versión real será fútbol.

El objetivo arquitectónico futuro será permitir que la comunidad pueda crear otros deportes sin reescribir el motor.

Ejemplos futuros:

- baloncesto;
- tenis;
- bádminton;
- balonmano;
- hockey;
- rugby;
- deportes de motor;
- deportes inventados.

La intención final puede resumirse así:

> **PC Dynasty es un simulador de ecosistemas deportivos. Fútbol será el primer ecosistema completo.**

---

# 3. Nombre

Nombre elegido actualmente:

# **PC Dynasty**

Interpretación:

```text
PC = Player Club
Dynasty = la historia que personas y clubes construyen durante décadas
```

Conceptualmente permite en el futuro:

```text
PC Dynasty — Football
PC Dynasty — Basketball
PC Dynasty — Tennis
```

No asumir disponibilidad legal definitiva.

Antes de comercialización deberá hacerse una comprobación registral real de marca y nombre comercial.

---

# 4. Filosofía de producto: fácil por fuera, profundo por dentro

La interfaz debe poder utilizarse en dos niveles simultáneamente.

## Nivel simple

El jugador casual debe poder entender rápidamente:

- calidad;
- forma;
- físico;
- mental;
- media;
- presupuesto;
- instalaciones;
- caché;
- química;
- situación de plantilla.

## Nivel profundo

El jugador avanzado debe poder pulsar cualquier indicador y descubrir:

- qué lo compone;
- qué datos están actualizados;
- qué datos son estimados;
- quién obtuvo la información;
- cuándo;
- con qué confianza;
- cómo ha evolucionado;
- qué consecuencias tiene;
- qué podría mejorarlo.

Patrón general:

```text
RESUMEN
   ↓
DETALLE
   ↓
SUBDETALLE
   ↓
DATOS / RAZONES / HISTORIAL
```

No se quiere una interfaz oscura ni una hoja de cálculo obligatoria.

Se quiere profundidad opcional.

---

# 5. Alcance inicial

## 5.1. Deporte inicial

Fútbol.

No desarrollar otros deportes completos durante la primera etapa.

## 5.2. Mundo inicial deseado

Objetivo de escala:

- aproximadamente 80 países;
- alrededor de 4 divisiones o más por país cuando tenga sentido;
- copas;
- supercopas;
- competiciones continentales;
- competiciones mundiales;
- ascensos;
- descensos;
- playoffs;
- reglas de inscripción;
- reglas financieras;
- sanciones;
- calendarios;
- cambios de reglas entre temporadas.

## 5.3. Datos oficiales del juego

Por falta de licencias, el juego debe poder distribuir:

- clubes ficticios;
- jugadores ficticios;
- entrenadores ficticios;
- escudos ficticios;
- equipaciones ficticias;
- competiciones genéricas cuando corresponda.

La comunidad podrá crear packs alternativos.

---

# 6. Stack técnico deseado

## 6.1. Godot

Godot será responsable de presentación:

- UI;
- navegación;
- input;
- mando;
- audio;
- animaciones;
- ventanas;
- paneles;
- editores;
- equipaciones;
- instalaciones;
- posibles representaciones 2D;
- timelines de partidos;
- gráficos y visualización.

## 6.2. .NET / C# puro

El dominio y simulación vivirán fuera de Godot.

Responsabilidades:

- personas;
- clubes;
- competiciones;
- reglas;
- economía;
- mercado;
- contratos;
- lesiones;
- scouting;
- simulación;
- mundo;
- persistencia;
- packs;
- historia.

Debe ser posible ejecutar tests y benchmarks sin abrir Godot.

## 6.3. Regla de dependencias

> **Godot depende del Core. El Core nunca depende de Godot.**

No crear un `Node` de Godot por persona del mundo.

---

# 7. Arquitectura conceptual inicial

La solución podría acabar teniendo módulos similares a:

```text
PCDynasty.Core
PCDynasty.Rules
PCDynasty.World
PCDynasty.People
PCDynasty.Knowledge
PCDynasty.Competition
PCDynasty.Simulation
PCDynasty.Economy
PCDynasty.Market
PCDynasty.Career
PCDynasty.Facilities
PCDynasty.Mods
PCDynasty.Persistence
PCDynasty.Benchmarks
PCDynasty.Tests
PCDynasty.TestSport
PCDynasty.Football
```

Esto NO es una orden de crear exactamente esos proyectos.

El agente debe revisar:

- scaffold actual;
- convenciones de `mcp-vertex`;
- dependencias;
- complejidad real;

antes de definir la solución final.

---

# 8. Principio arquitectónico más importante

> **El Core no conoce fútbol.**

El Core no debería saber qué significan:

```text
gol
fuera de juego
portero
delantero
córner
penalti
tarjeta amarilla
Champions
```

El `FootballPack` sí.

El Core conoce conceptos abstractos:

```text
Person
Participant
Organization
Role
PositionDefinition
Attribute
Action
Contribution
Contest
Competition
Stage
Ranking
Rule
Violation
Sanction
Contract
Facility
Observation
Projection
Knowledge
```

Cualquier aparición repetida en Core de:

```csharp
if (sport == Football)
```

debe considerarse un olor arquitectónico.

---

# 9. TestSport

Aunque no se desarrollen otros deportes, debe existir un deporte artificial mínimo para tests.

Ejemplo:

- 1 participante contra 1;
- sin club obligatorio;
- sin posiciones;
- 3 atributos;
- puntuación a 10;
- competición simple.

Objetivo:

si alguien introduce en Core una obligación de:

- posiciones;
- equipo;
- portero;
- goles;
- banquillo;
- liga;

los tests deben fallar.

Así se evita que tres años de desarrollo de fútbol conviertan accidentalmente el Core en un motor exclusivamente futbolístico.

---

# 10. Packs

La extensibilidad futura debe basarse principalmente en packs.

## 10.1. SportPack

Define un deporte:

- atributos;
- acciones;
- contribuciones;
- posiciones;
- roles;
- estructura de participantes;
- estructura del encuentro;
- scoring;
- estadísticas;
- sanciones;
- entrenamientos;
- modelo de rendimiento;
- simulación;
- dominios de conocimiento.

## 10.2. CompetitionPack

Define:

- competición;
- fases;
- calendario;
- participantes;
- rankings;
- clasificación;
- ascensos;
- descensos;
- playoffs;
- reglas financieras;
- reglas de inscripción;
- reglas de plantilla;
- sanciones;
- premios;
- reglas por temporada.

## 10.3. DatabasePack

Define:

- clubes;
- personas;
- jugadores;
- entrenadores;
- instalaciones;
- estadios;
- países;
- historias;
- datos.

## 10.4. AssetPack

Puede contener:

- escudos;
- fotografías;
- equipaciones;
- sonidos;
- recursos gráficos.

## 10.5. PresentationPack

Puede modificar:

- temas;
- layouts;
- pantallas;
- estilos de competiciones;
- iconografía.

## 10.6. Dependencias

Los packs deben poder declarar dependencias y versiones.

El formato exacto se deberá proponer.

---

# 11. Seguridad de mods

Por defecto los packs comunitarios deberían ser datos.

No permitir ejecución arbitraria de C# sin una propuesta seria de seguridad.

Si se necesita scripting:

- sandbox;
- API limitada;
- sin filesystem arbitrario;
- sin procesos;
- sin red arbitraria;
- validación.

No diseñar un lenguaje enorme si no hace falta.

La filosofía preferida:

```text
80–95% declarativo
5–20% escape hatch controlado
```

---

# 12. Person como entidad universal

Jugadores y empleados son personas.

No deben existir como universos completamente desconectados.

Una persona puede tener una carrera:

```text
Jugador
  ↓
Retirado
  ↓
Segundo entrenador
  ↓
Entrenador
  ↓
Director deportivo
```

El historial permanece.

## 12.1. Identidad

- nombre;
- apellidos;
- nacimiento;
- nacionalidad;
- región;
- idiomas;
- estado vital;
- historial.

## 12.2. Perfil personal

- profesionalidad;
- ética de trabajo;
- competitividad;
- resiliencia;
- ambición;
- adaptabilidad;
- disciplina;
- consistencia;
- lealtad;
- capacidad de aprendizaje;
- respuesta a presión.

## 12.3. Perfil biológico

- maduración;
- durabilidad;
- propensión a lesiones;
- recuperación;
- tolerancia de carga;
- longevidad;
- curvas de envejecimiento.

---

# 13. No existe un potencial fijo visible

Evitar el modelo conceptual de:

```text
PotentialAbility = 95
```

como destino fijo visible.

El futuro depende de:

- talento;
- trabajo;
- entrenamiento;
- profesionales;
- instalaciones;
- oportunidades;
- minutos;
- lesiones;
- edad;
- biología;
- mentalidad;
- estilo de vida;
- azar.

Las proyecciones futuras son estimaciones estadísticas.

---

# 14. Jugador de fútbol: todos tienen todos los atributos

No separar estructuralmente “atributos de portero” y “atributos de jugador de campo” de forma que uno no pueda tener los otros.

Todo futbolista posee el catálogo completo del FootballPack.

Ejemplo conceptual:

```text
TÉCNICA
- control
- regate
- pase corto
- pase largo
- centro
- disparo
- finalización
- cabeceo

DEFENSA
- entrada
- marcaje
- colocación
- anticipación

FÍSICO
- aceleración
- velocidad
- fuerza
- resistencia
- salto
- agilidad

MENTAL/COGNITIVO
- decisiones
- concentración
- visión
- compostura
- valentía

PORTERÍA
- reflejos
- blocaje
- colocación
- uno contra uno
- juego aéreo
- salida
- juego con pies
```

El catálogo final debe estudiarse.

---

# 15. Mostrar atributos según relevancia

En un delantero:

1. atributos relevantes ofensivos;
2. otros atributos de campo;
3. atributos defensivos menos relevantes;
4. atributos de portería al final.

En un portero:

1. atributos de portería;
2. juego con pies relevante;
3. resto de atributos de campo;
4. información general.

El usuario debe poder abrir “todas las características”.

Esto permite descubrir casos extraordinarios:

- central que podría ser gran portero;
- portero que podría ser mediocampista;
- lateral que podría ser extremo.

---

# 16. Posiciones y roles

Las posiciones no determinan los atributos.

Representan:

- familiaridad;
- experiencia;
- preferencia;
- comodidad;
- historial de uso.

Ejemplo:

```text
LI
familiaridad: 98
experiencia: 94
preferencia: 91

EI
familiaridad: 67
experiencia: 51
preferencia: 83
```

## 16.1. Media por posición

El jugador puede tener distintas valoraciones según posición/rol.

Ejemplo:

```text
EI  89
CAI 87
ED  86
LI  84
MI  82
MC  76
DC  73
POR 24
```

## 16.2. Reconversión

Un jugador puede aprender otra posición.

La familiaridad aumenta con:

- entrenamiento;
- minutos;
- entrenador;
- atributos;
- adaptación;
- preferencia.

Una posición secundaria puede acabar convirtiéndose en principal.

## 16.3. Preferencia

No todos quieren jugar en cualquier posición.

Forzar a alguien puede afectar:

- motivación;
- rendimiento;
- relación con entrenador;
- deseo de marcharse.

Otro jugador puede disfrutar de la versatilidad.

---

# 17. Indicadores principales del jugador

La ficha principal debe resumirse con cinco indicadores:

```text
┌──────────┬─────────┬─────────┬──────────┬─────────┐
│ TÉCNICA  │ FORMA   │ FÍSICO  │ MENTAL   │ MEDIA   │
│    88    │  92 ↑   │   84    │   76 ~   │   86    │
└──────────┴─────────┴─────────┴──────────┴─────────┘
```

## 17.1. TÉCNICA

Síntesis de capacidades futbolísticas actuales relevantes para el rol/posición.

No incluye forma reciente.

## 17.2. FORMA

Rendimiento reciente comparado con lo esperado de esa persona.

No significa “número de acciones buenas”.

Significa:

> “¿está haciendo más o menos de lo que su propio nivel actual hacía esperar?”

## 17.3. FÍSICO

Estado físico utilizable actualmente:

- fitness;
- fatiga;
- ritmo;
- molestias;
- carga;
- recuperación.

## 17.4. MENTAL

Estado psicológico actual:

- confianza;
- moral;
- motivación;
- concentración;
- presión;
- complacencia;
- determinación.

Puede ser estimado.

Ejemplo:

```text
MENTAL 76 ~
```

## 17.5. MEDIA

> **MEDIA = síntesis actual de TÉCNICA + FORMA + FÍSICO + MENTAL.**

Es la mejor cifra sencilla de “cómo esperamos que actúe ahora”.

No es potencial.

No es valor de mercado.

No es reputación.

El cálculo no tiene por qué ser aritmética 25/25/25/25.

Puede usar pesos, límites no lineales y contexto.

Pero conceptualmente representa la combinación de las cuatro dimensiones.

---

# 18. Forma basada en expectativa individual

Un jugador mediocre haciendo dos buenos regates puede estar jugando muy por encima de su media.

Un jugador de élite haciendo exactamente lo mismo puede estar jugando mal.

Por tanto:

```text
ExpectedPerformance
vs
ObservedPerformance
```

Debe considerar:

- atributos;
- rival;
- nivel de competición;
- rol;
- táctica;
- minutos;
- fatiga;
- contexto.

Esto produce:

```text
PerformanceVsExpected
```

---

# 19. Modelo universal de rendimiento

El Core debe trabajar conceptualmente con:

```text
CAPACIDADES
   ↓
ACCIONES
   ↓
EFICIENCIA CONTEXTUAL
   ↓
CONTRIBUCIONES
   ↓
RENDIMIENTO
```

El FootballPack define acciones como:

- pase;
- regate;
- duelo;
- remate;
- presión;
- intercepción.

Otro deporte podría definir acciones totalmente diferentes.

---

# 20. Rendimiento no equivale a goles

Un defensa no debe medirse por goles.

Un delantero tampoco tiene que medirse exclusivamente por goles.

Un atacante puede ser extraordinario por:

- presión;
- desmarques;
- creación de espacios;
- asociación;
- retención;
- asistencias;
- arrastre de defensas.

Un defensa puede aportar mediante:

- colocación;
- intercepciones;
- duelos;
- progresión;
- control de espacio;
- anticipación;
- reducción de errores.

El FootballPack debe definir contribuciones y pesos por rol.

---

# 21. Evidencia y evolución de atributos

Una buena actuación no debe subir instantáneamente atributos.

Debe existir:

- tamaño de muestra;
- confianza;
- acumulación de evidencia;
- regresión a la media.

Ejemplo:

```text
Regate real actual: 62
```

Un partido espectacular puede generar:

```text
62 → 62.3
```

o incluso sólo modificar forma.

Si durante meses rinde como un jugador claramente superior, puede evolucionar:

```text
62 → 65 → 68 → 71
```

Al revés, un crack con varios meses malos primero pierde:

- forma;
- confianza;
- reputación reciente;
- valor de mercado.

Sólo con evidencia persistente debe deteriorarse su capacidad base.

---

# 22. Talento, trabajo y longevidad

Dos jugadores extraordinarios pueden llegar a la élite por caminos diferentes.

Uno puede tener talento natural excepcional.

Otro puede combinar gran talento con una disciplina y capacidad de trabajo extremas.

Debe existir un perfil similar a:

```text
naturalTalent
learningCapacity
workEthic
professionalism
ambition
consistency
resilience
coachability
competitiveDrive
```

Y un perfil biológico:

```text
maturationSpeed
recoveryRate
durability
workloadTolerance
injurySusceptibility
declineRate
longevity
```

La edad por sí sola no determina decadencia.

---

# 23. Envejecimiento por capacidades

No usar:

```text
cumple 31 → -2 a todo
```

Puede haber distintas curvas:

```text
velocidad → declive temprano
fuerza → declive más tardío
visión → puede mejorar
colocación → puede mejorar con experiencia
```

El cuidado durante la carrera importa.

---

# 24. CareerWear

Debe contemplarse desgaste acumulado por:

- minutos;
- sobrecargas;
- entrenamientos;
- lesiones;
- recuperaciones;
- descanso;
- profesionalidad;
- instalaciones;
- médicos;
- estilo de juego.

Dos personas de la misma edad pueden estar físicamente en estados totalmente distintos.

---

# 25. Lesiones

Una lesión debe contener más información que “30 días”.

Conceptos:

```text
InjuryInstance
- type
- bodyArea
- severity
- structuralDamage
- recurrenceRisk
- rehabDifficulty
- expectedRecovery
- possibleSequelae
```

La recuperación depende de:

- persona;
- edad;
- biología;
- profesionalidad;
- disciplina de rehabilitación;
- historial;
- médico;
- fisio;
- instalaciones;
- presupuesto.

---

# 26. Recuperación médica y deportiva

No es lo mismo:

```text
apto para jugar
```

que:

```text
ha recuperado su mejor nivel
```

Estados conceptuales:

```text
LESIONADO
   ↓
APTO MÉDICAMENTE
   ↓
RECUPERANDO RITMO
   ↓
NIVEL COMPETITIVO
```

Una lesión grave puede dejar secuelas permanentes.

Otro jugador puede recuperarse casi por completo.

---

# 27. LifeEventEngine

Una persona puede sufrir eventos externos.

No se quiere un simulador de vida completo.

Se quieren eventos que tengan consecuencias deportivas, económicas o mentales.

## 27.1. Salud

- resfriado;
- gripe;
- infección;
- enfermedad;
- problema crónico.

Algunos no tienen “tratamiento deportivo”, simplemente requieren reposo.

## 27.2. Accidentes

Muy poco frecuentes:

- accidente menor;
- accidente grave;
- lesión permanente;
- fin de carrera;
- fallecimiento.

Los eventos extremos deben poder configurarse y tratarse con sobriedad.

## 27.3. Vida personal

- problemas familiares;
- adaptación;
- nostalgia;
- separación;
- estrés;
- cambio de residencia;
- nacimiento de hijos.

## 27.4. Profesionalidad/estilo de vida

Puede influir en:

- asistencia a entrenamientos;
- forma física;
- motivación;
- disciplina;
- longevidad;
- conflictos.

## 27.5. Problemas legales/reputacionales

Puede existir un modelo genérico de:

- investigación;
- escándalo;
- sanción;
- condena;
- indisponibilidad.

Sus consecuencias pueden afectar:

- contrato;
- reputación;
- club;
- patrocinadores;
- mental;
- disponibilidad.

---

# 28. Realidad del jugador vs conocimiento del club

Principio fundamental:

> **Player State != Club Knowledge**

Debe existir:

```text
REALIDAD
   ↓
OBSERVACIÓN
   ↓
CONOCIMIENTO DEL CLUB
   ↓
UI
```

La persona tiene un estado real.

El club tiene una estimación de ese estado.

---

# 29. Información atributo por atributo

No usar únicamente:

```text
scouted = 80%
```

Debe existir algo parecido a:

```text
AttributeKnowledge
- attributeId
- estimatedValue
- confidence
- observedAt
- observerId
- observations
- trend
```

Ejemplo:

```text
Regate: 87
confianza: 96%
actualizado: hace 4 días
```

Mientras:

```text
Reflejos de portero: 50–75
confianza: 18%
actualizado: hace 3 años
```

---

# 30. Información conocida, estimada, obsoleta y desconocida

La UI debe utilizar un lenguaje visual consistente.

Ejemplo:

```text
✓ fiable/reciente
~ estimación
! desactualizado
? desconocido
↑ mejora
↓ empeora
```

No esconder arbitrariamente la información.

Mostrar incertidumbre de forma comprensible.

---

# 31. Profesionales y conocimiento

El club puede contratar:

- entrenador principal;
- segundos entrenadores;
- entrenadores especializados;
- entrenador de porteros;
- preparadores físicos;
- médicos;
- fisioterapeutas;
- psicólogos;
- analistas;
- scouts;
- directores deportivos;
- responsables de cantera;
- otros especialistas.

Cada profesional tiene estadísticas propias.

---

# 32. KnowledgeDomain

Diferentes profesionales conocen distintos ámbitos.

Dominios posibles del FootballPack:

- técnico;
- táctico;
- físico;
- médico;
- mental;
- rendimiento;
- desarrollo;
- personalidad;
- longevidad;
- adaptación;
- contrato;
- posición/rol.

Ejemplo conceptual:

| Profesional | Técnico | Físico | Médico | Mental | Proyección |
|---|---:|---:|---:|---:|---:|
| Entrenador | alto | medio | bajo | medio | alto |
| Preparador | bajo | muy alto | medio | bajo | medio |
| Médico | bajo | alto | muy alto | bajo | bajo |
| Psicólogo | bajo | bajo | bajo | muy alto | medio |
| Analista | alto | medio | bajo | bajo | medio |
| Scout | alto | medio | medio | medio | muy alto |

---

# 33. Jugadores propios

Un jugador propio se observa diariamente por algunos profesionales.

Por tanto el club puede tener:

```text
Técnico 99%
Físico 98%
Médico 99%
Mental 61%
```

Si no hay psicólogo, la información mental puede ser una estimación.

No se debe conceder omnisciencia por ser jugador propio.

---

# 34. Jugadores externos

Se puede conocer públicamente:

- edad;
- club;
- partidos;
- estadísticas visibles;
- posición habitual;
- rendimiento;
- reputación aproximada;
- valor aproximado.

No conocer automáticamente:

- estado médico real;
- profesionalidad;
- personalidad;
- mental;
- deseos;
- atributos precisos;
- longevidad;
- proyección.

---

# 35. Scouting dirigido

Las órdenes de scouting deben poder ser específicas.

Ejemplos:

```text
Evaluación general
Evaluar técnica ofensiva
Evaluar físico
Evaluar estado médico
Evaluar personalidad
Evaluar longevidad
Evaluar como extremo izquierdo
Evaluar como portero
Evaluar adaptación al club
```

Cuanto más específica sea la tarea, mayor precisión en ese dominio.

---

# 36. Proyecciones

Una proyección es una distribución estadística basada en la información actual.

Ejemplo:

```text
Nivel actual: 76
Nivel probable en 3 años: 82–88
Probabilidad de élite: 36%
Confianza: 62%
```

No significa que el futuro esté decidido.

En cinco años pueden haber cambiado:

- profesionalidad;
- lesiones;
- motivación;
- carga;
- entorno;
- entrenadores;
- hábitos.

Y la proyección cambiará.

---

# 37. Proyección de longevidad

Un especialista puede estimar:

```text
Probabilidad de mantener nivel élite hasta 32: 91%
Hasta 35: 68%
Hasta 38: 31%
Hasta 40: 12%
```

Eso es una hipótesis actual, no una verdad.

Puede modificarse completamente años después.

---

# 38. Historial de scouting

Conservar informes antiguos.

Ejemplo:

```text
2028 Scout A: 77–84
2029 Scout A: 80–87
2030 Scout B: 85–91
2032 Scout B: 82–87
```

Permite observar:

- evolución real;
- errores de scouts;
- cambios de expectativa;
- incertidumbre.

---

# 39. Empleados también evolucionan

Un scout, médico o entrenador:

- aprende;
- envejece;
- gana experiencia;
- se especializa;
- puede empeorar;
- puede retirarse;
- puede cambiar de club;
- puede querer irse;
- puede recibir ofertas.

No existe edad fija universal de jubilación.

---

# 40. Conocimiento regional del empleado

Un scout puede tener:

```text
España 98
Portugal 84
Argentina 67
Japón 22
```

Si trabaja años en Argentina:

```text
Argentina → 91
```

Este conocimiento puede deteriorarse lentamente si deja de trabajar esa región.

---

# 41. Contratos de empleados

Un empleado tiene:

- salario;
- duración;
- rol;
- bonus;
- responsabilidades;
- cláusulas.

Si empeora durante el contrato, su salario no baja automáticamente.

Otros clubes pueden intentar contratarlo.

---

# 42. Relaciones persona-club

Debe existir algo similar a:

```text
PersonClubRelationship
```

Con:

- lealtad;
- apego;
- felicidad;
- confianza;
- identificación;
- relación con entrenador;
- historia;
- afición.

Un jugador puede ser extremadamente leal a un club concreto y no a otros.

---

# 43. Descensos y lealtad

Si un equipo desciende:

Jugador A:

```text
ambición alta
lealtad baja
→ quiere marcharse
```

Jugador B:

```text
lealtad extrema
identificación extrema
→ quiere quedarse
```

La decisión no puede basarse únicamente en categoría o salario.

---

# 44. Atracción del club

Una persona puede encontrar atractivo un club por:

- caché;
- historia;
- salario;
- ciudad;
- país;
- plantilla;
- entrenador;
- instalaciones;
- competición;
- proyecto;
- estabilidad.

Otro puede rechazarlo aunque pague más.

---

# 45. Negotiation Engine

Debe ser un subsistema central.

Aplicable a:

- jugadores;
- entrenadores;
- scouts;
- médicos;
- analistas;
- empleados.

Conceptos:

```text
Requirements
Preferences
Offer
CounterOffer
Alternatives
Leverage
Deadlines
Relationships
Promises
Clauses
```

No usar sólo:

```text
salario más alto → acepta
```

---

# 46. Valoraciones económicas separadas

Distinguir:

```text
MarketValue
InternalValuation
AskingPrice
TransferPrice
ContractValue
```

## MarketValue

Percepción general del mercado.

## InternalValuation

Valor que un club específico atribuye.

## AskingPrice

Precio solicitado.

## TransferPrice

Precio acordado.

## ContractValue

Compromiso contractual.

---

# 47. Valor de mercado dinámico

Debe reaccionar a:

- capacidad actual;
- forma;
- rendimiento;
- edad;
- reputación;
- contrato;
- demanda;
- liga;
- escasez;
- inflación;
- proyección percibida.

Un crack con una mala temporada puede perder mucho valor sin perder inmediatamente toda su técnica.

Un jugador mediocre que explota puede aumentar muchísimo su precio antes de saber si el rendimiento será sostenible.

---

# 48. Mercado puede equivocarse

El mercado no conoce la verdad absoluta.

Puede sobrevalorar o infravalorar.

Ejemplo:

```text
True sporting value estimado internamente: alto
MarketValue: bajo
```

Puede ser una ganga.

O al revés.

Esto crea:

- fichajes brillantes;
- errores;
- burbujas;
- jugadores de moda;
- oportunidades.

---

# 49. Mercado global eficiente

No comparar todos los clubes con todos los jugadores.

Usar índices y pools.

Ejemplo de filtros:

```text
sport
country
level
age
role
ability
salary
value
contractStatus
```

El club obtiene candidatos relevantes.

---

# 50. Mercado por rondas

Una ventana agregada puede resolverse mediante rondas:

1. necesidades principales;
2. reemplazos tras ventas;
3. oportunidades;
4. agentes libres;
5. cierre/emergencias.

Una venta aumenta el dinero del vendedor.

El vendedor puede entrar de nuevo al mercado.

---

# 51. Economía del club

Variables posibles:

```text
cash
debt
annualRevenue
wageBill
transferBalance
projectedRevenue
financialHealth
```

Ingresos:

- TV;
- premios;
- entradas;
- socios;
- merchandising;
- patrocinadores;
- transferencias;
- inversiones.

Gastos:

- salarios;
- primas;
- staff;
- mantenimiento;
- infraestructura;
- deuda;
- intereses;
- agentes;
- cantera;
- operaciones.

---

# 52. El dinero se conserva

Una venta de 50M implica:

```text
Club comprador: -50M
Club vendedor: +50M
```

Luego el vendedor decide cómo distribuirlo:

- fichajes;
- deuda;
- caja;
- instalaciones;
- cantera.

No convertir automáticamente 50M de venta en 50M de presupuesto de fichajes.

---

# 53. Inflación

En una partida de décadas deben evolucionar:

- precios de transferencias;
- salarios;
- TV;
- patrocinadores;
- economía regional.

Puede haber índices:

```text
TransferMarketIndex
WageIndex
BroadcastingIndex
CommercialIndex
```

Evitar crecimiento infinito.

---

# 54. Reglas financieras

Cada competición/federación puede tener reglas distintas:

- salary cap;
- ratios;
- límites de pérdidas;
- deuda;
- impuesto de lujo;
- límites de inscripción.

Incumplimientos producen sanciones mediante el modelo genérico de `Violation → Sanction`.

---

# 55. Caché/prestigio del club

El caché debe evolucionar.

Dimensiones:

```text
Prestigio histórico
Prestigio actual
Reputación internacional
Poder económico
Calidad plantilla
Atractivo del proyecto
Popularidad
```

UI simple:

```text
CACHÉ 78
```

---

# 56. Clubes pueden transformarse por completo

El mundo debe permitir naturalmente:

- clubes menores que se convierten en potencias;
- gigantes que caen;
- históricos en ruina;
- proyectos nuevos de éxito.

El prestigio histórico cae lentamente.

El prestigio actual puede cambiar rápido.

---

# 57. Afición

No modelar sólo un número total.

Segmentos posibles:

```text
hardcore/socios
locales
casuales
nacionales
internacionales
```

Los hardcore cambian lentamente.

Los casuales e internacionales reaccionan más a resultados, estrellas y exposición.

---

# 58. Resultados y fanbase

Buenas temporadas producen:

- asistencia;
- camisetas;
- merchandising;
- patrocinadores;
- aficionados casuales.

Décadas de éxito pueden cambiar la base generacional de aficionados.

Décadas malas pueden reducirla.

---

# 59. Estadio y demanda

La asistencia puede depender de:

```text
FanDemand
MatchInterest
OpponentInterest
CompetitionImportance
TicketPrice
RecentResults
StadiumQuality
```

Limitada por aforo.

Un estadio sobredimensionado genera costes inútiles.

Uno pequeño frente a mucha demanda crea oportunidad de expansión.

---

# 60. Instalaciones

El jugador debe poder construir y mejorar instalaciones.

## Estadio

- gradas;
- palcos;
- restaurantes;
- tienda;
- museo;
- VIP;
- iluminación;
- accesos;
- césped;
- vestuarios.

## Ciudad deportiva

- campos;
- gimnasio;
- centro médico;
- recuperación;
- analítica.

## Cantera

- residencia;
- campos;
- educación;
- captación;
- analítica juvenil.

---

# 61. Instalaciones: calidad, estado y modernidad

Cada instalación debe distinguir:

```text
CALIDAD
ESTADO
MODERNIDAD
```

Ejemplo:

```text
Centro médico
Calidad: 86
Estado: 97
Modernidad: 64
```

Puede estar perfectamente conservado pero tecnológicamente obsoleto.

---

# 62. Deterioro físico

Sin mantenimiento:

```text
100 → 93 → 81 → 68
```

Puede llegar a:

- cierre de grada;
- reducción de aforo;
- imposibilidad de uso;
- necesidad de jugar en otro estadio.

---

# 63. Obsolescencia tecnológica

Aunque se mantenga físicamente:

```text
Modernidad relativa ↓
```

porque otros clubes invierten y aparecen nuevas tecnologías.

La mejor instalación actual puede ser mediocre dentro de 15 años sin modernización.

---

# 64. Mantenimiento

Construir genera costes permanentes.

Ejemplo:

```text
Nueva grada
Construcción: 40M
Mantenimiento: 1.8M/año
Capacidad: +10.000
```

No debe ser óptimo construir siempre todo al máximo.

---

# 65. Instalaciones + empleados

Una instalación 95 con staff 55 no rinde 95.

Un staff 99 en instalación 60 tampoco puede producir milagros ilimitados.

Estudiar una combinación equivalente a:

```text
EffectivePerformance =
Facility × Staff × Funding × Condition
```

normalizada.

---

# 66. Cantera

Debe depender de:

- instalaciones;
- entrenadores;
- scouts;
- reputación;
- financiación;
- metodología;
- red.

Una cantera excelente aumenta probabilidades.

No garantiza estrellas.

---

# 67. Deterioro de cantera

Si no se invierte durante años:

- instalaciones bajan;
- staff se marcha;
- metodología envejece;
- reputación cae;
- producción baja.

Debe ser posible destruir una cantera históricamente excelente mediante mala gestión.

---

# 68. Generación de jóvenes

Cada país/región puede tener `TalentProductionModel`.

Factores:

- población;
- popularidad;
- infraestructura;
- tradición;
- economía;
- éxito;
- azar.

Usar generación procedural cuando sea posible.

---

# 69. Personas procedurales

No es necesario materializar millones de jóvenes irrelevantes.

Puede existir una cohorte por seed.

Cuando un jugador:

- entra en cantera;
- es observado;
- se ficha;
- juega;

se convierte en entidad persistente.

---

# 70. Entrenamiento

El Core conoce `TrainingActivity`.

El FootballPack define:

- finalización;
- pases;
- físico;
- táctica;
- porteros;
- reconversión posicional.

Entrenadores, instalaciones, mentalidad, edad y carga modifican efectos.

---

# 71. Química del equipo

La química no forma parte de la media individual.

Es una propiedad del grupo.

Resumen:

```text
QUÍMICA 92 ↑
```

Detalle:

- relaciones entre jugadores;
- relación con entrenador;
- aceptación táctica;
- liderazgo;
- conflictos;
- integración;
- justicia percibida;
- estabilidad.

---

# 72. Resultados y química

Diez victorias pueden producir:

- confianza;
- cohesión;
- buen ambiente.

Diez derrotas pueden producir:

- culpas;
- conflictos;
- dudas sobre entrenador;
- vestuario roto.

La química debe influir en rendimiento colectivo.

---

# 73. Motivación del partido

Debe existir explícitamente.

No es lo mismo:

- amistoso;
- liga;
- derbi;
- final de copa;
- final continental.

Pero la importancia es personal.

Un jugador con muchos títulos puede valorar menos una competición menor.

Otro con competitividad extrema puede querer ganarlo todo.

---

# 74. Presión y complacencia

Motivación alta no significa automáticamente mejor rendimiento.

Puede existir:

```text
Motivación 100
Presión 95
Confianza 61
```

Y el jugador bloquearse.

Un favorito contra un equipo pequeño puede tener:

- complacencia alta;
- motivación baja.

El pequeño puede tener motivación extrema.

---

# 75. MEDIA de equipo no basta

El resultado de un partido no debe ser sólo la media de medias individuales.

Considerar:

```text
jugadores
roles
táctica
química
entrenador
matchup
localía
motivación
fatiga
mental
azar
```

---

# 76. Simulación probabilística

Un equipo mucho mejor debe ganar más frecuentemente.

Nunca tener certeza absoluta.

Ejemplo hipotético:

```text
Favorito gana 83%
Empate 10%
Pequeño gana 7%
```

Ese 7% permite un 0–3 extraordinario.

El motor debe producir distribuciones, no sentencias.

---

# 77. Partidos instantáneos

Principio no negociable:

> **simular un partido debe ser computacionalmente casi instantáneo.**

La interfaz puede reproducir un timeline después.

No calcular el partido mientras la animación transcurre.

---

# 78. Contest snapshot

El motor recibe algo equivalente a:

```text
ContestSimulationInput
```

Con:

- participantes;
- atributos;
- forma;
- físico;
- mental;
- media;
- química;
- entrenador;
- táctica;
- reglas;
- contexto;
- seed.

Devuelve:

```text
ContestSimulationOutput
```

Con:

- resultado;
- estadísticas;
- timeline;
- lesiones;
- sanciones;
- cambios;
- ratings.

---

# 79. Seed determinista

Mismo input + misma seed = mismo resultado.

Esto facilita:

- debugging;
- tests;
- reproducción de bugs;
- benchmarks.

---

# 80. Fidelidades del partido

Podrían existir:

```text
Fast
Standard
Detailed
```

Todos deben estar calibrados contra el mismo modelo estadístico.

No permitir que Fast produzca un universo radicalmente distinto de Detailed.

---

# 81. Convocatoria, titulares y banquillo

El fútbol necesita:

```text
Plantilla
   ↓
Convocados
   ├── Titulares
   └── Banquillo

No convocados
```

El Core debe manejar conceptos genéricos:

```text
ParticipantPool
Selected
Active
Reserve
Unselected
```

Las reglas del deporte/competición definen números y restricciones.

---

# 82. Sustituciones

No son específicas de fútbol en el Core.

Debe existir algo similar a:

```text
ParticipationTransitionRules
```

Fútbol puede declarar:

```text
Reserve → Active
Active → Removed
maxChanges = X
reentry = false
```

Otros deportes podrán declarar reglas distintas.

---

# 83. Sanciones

Modelo universal:

```text
Event
 ↓
Infraction
 ↓
DisciplinaryLedger
 ↓
Rule
 ↓
Sanction
```

Puede existir scope:

- competición;
- fase;
- deporte;
- federación;
- global.

Puede incluir:

- acumulación;
- suspensión;
- multa;
- puntos;
- prohibición;
- exclusión.

---

# 84. Competition no equivale a League

El Core debe manejar:

```text
Competition
```

No clases rígidas que asuman sólo:

- liga;
- copa;
- torneo.

Una competición tiene:

- fases;
- reglas;
- transiciones;
- clasificación;
- inscripción;
- presentación.

---

# 85. Temporada como grafo

Ejemplo:

```text
Liga regular
  ↓ top 6
Grupo campeonato
```

Otro:

```text
Grupos
 ├── 1–2 → eliminatoria
 └── 3 → otra competición
```

Otro:

```text
Regular Season
 ↓
Play-In
 ↓
Playoffs
```

---

# 86. Reglas versionadas

Las reglas cambian cada temporada.

Debe existir:

```text
Competition
 ├── Ruleset 2026
 ├── Ruleset 2027
 └── Ruleset 2030
```

Preferiblemente con deltas/herencia cuando sea razonable.

---

# 87. Rankings independientes

No asumir que ranking = puntos de liga.

Debe existir `RankingSystem` capaz de representar en el futuro:

- puntos;
- ventanas temporales;
- mejores N resultados;
- coeficientes;
- bonus;
- penalizaciones;
- descartes.

Esto será importante para tenis/bádminton y otros deportes.

---

# 88. Scoring genérico futuro

El sistema futuro debe poder representar:

Fútbol:

```text
Goal → Match
```

Tenis:

```text
Point → Game → Set → Match
```

Bádminton:

```text
Rally → Game → Match
```

Estudiar una `Scoring State Machine` o equivalente.

---

# 89. Mundo mundial sin simular todos los partidos

Este es otro principio no negociable.

El mundo debe evolucionar.

Pero no debe simular cada partido de cada país.

---

# 90. Simulación estructural anual

Cada competición del deporte seleccionado debe avanzar estructuralmente por temporada.

Debe resolver:

- clasificación;
- campeón;
- ascensos;
- descensos;
- clasificados;
- economía;
- fichajes;
- ventas;
- renovaciones;
- desarrollo;
- jubilaciones;
- jóvenes;
- empleados;
- instalaciones;
- nueva fuerza.

---

# 91. No congelar una liga durante 30 años

Incorrecto:

```text
2026 ─────────────→ 2056
```

Correcto:

```text
2026
 ↓ SeasonTransition
2027
 ↓ SeasonTransition
2028
 ↓
...
2056
```

Cada paso puede ser macro y muy barato.

Pero debe existir dependencia de estado entre temporadas.

---

# 92. 80 países y 4 divisiones

Escenario inicial:

```text
80 países
× 4 divisiones
× 20 clubes
≈ 6.400 clubes
```

Con 25 jugadores:

```text
≈160.000 jugadores
```

El mundo total puede crecer con:

- empleados;
- reservas;
- canteras;
- agentes libres.

El diseño debe soportar varios cientos de miles de personas.

---

# 93. Evitar millones de partidos innecesarios

Con 320 ligas de 20 equipos:

```text
≈121.600 partidos de liga/año
```

En 30 años:

```text
≈3,65 millones
```

No deben materializarse si nadie los necesita.

---

# 94. MacroSeasonTransition

Una liga dormida debe producir un resumen coherente de temporada.

Mínimo:

```text
ranking
champion
promoted
relegated
qualified
clubPerformance
clubEconomy
rosterChanges
staffChanges
playerDevelopment
facilities
seed
```

No necesita almacenar:

- minuto de cada gol;
- alineaciones;
- sustituciones;
- cada tarjeta.

---

# 95. La liga sigue evolucionando aunque nadie la mire

Argentina puede pasar 20 años sin ser consultada.

Aun así cada año debe cambiar:

- clubes en cada división;
- jugadores;
- entrenadores;
- economía;
- instalaciones;
- fuerza;
- caché.

Cuando el usuario la consulte, no puede aparecer congelada con los clubes de 2026.

---

# 96. Materialización cuando el usuario consulta

Si el usuario consultó una liga el 20 de enero de 2030 y vuelve en febrero de 2032:

no calcular desde el estado de 2030 como si nada hubiera pasado.

Debe haber ocurrido:

```text
resto 29/30 macro
mercado
30/31 macro
mercado
31/32 hasta fecha
materialización
```

---

# 97. HistoricalCommitments

Una vez que un hecho afecta a otro sistema debe persistir.

Ejemplos:

- campeón;
- ascendido;
- descendido;
- clasificado;
- fichaje;
- retirada;
- ganador de copa.

Si un club entró en una competición continental por ser campeón, al materializar después su liga no puede dejar de ser campeón.

---

# 98. Dependencias de clasificación

Una competición continental pide salidas:

```text
España → slots 1–4
Francia → slots 1–3
```

No necesita todos los partidos nacionales.

La liga resuelve el mínimo estado coherente necesario.

---

# 99. Calendarios mundiales

No asumir que todo acaba en junio.

Cada competición tiene:

- inicio;
- final;
- ruleset;
- calendario.

Los procesos macro pueden distribuirse por el año.

---

# 100. Jugadores y desarrollo mundial

No recalcular diariamente cientos de miles de personas.

Usar distintas frecuencias.

## Por partido

Sólo participantes:

- forma;
- fatiga;
- confianza;
- eventos;
- lesiones.

## Semanal/mensual

Sólo ligas/personas activas:

- entrenamientos;
- recuperación;
- desarrollo fino.

## Temporada/ventana

Mundo macro:

- evolución;
- declive;
- mercado;
- contratos;
- jubilaciones;
- cantera;
- instalaciones;
- economía.

---

# 101. Lazy evaluation

Una persona puede almacenar:

```text
lastDevelopmentUpdate
```

Cuando sea necesario:

```text
AdvanceState(from, to)
```

Conceptualmente evolucionó durante ese periodo.

Computacionalmente se calcula cuando hace falta.

---

# 102. Rendimiento de datos

No utilizar un objeto pesado por persona.

No usar Nodes de Godot para todo el mundo.

Preferir:

- IDs numéricos;
- arrays compactos;
- estructuras eficientes;
- índices;
- hot/cold data.

Los packs pueden definir:

```text
"finishing"
```

Pero al cargar se compila:

```text
finishing → AttributeId 17
```

Los hot loops trabajan con IDs.

---

# 103. No usar strings dinámicos en hot loops

Evitar millones de accesos del tipo:

```text
Dictionary<string, float>
```

si pueden resolverse al cargar el pack.

Configurabilidad externa y rendimiento interno deben estar separados.

---

# 104. Benchmarks desde el inicio

Crear benchmarks reales para:

- 10.000 partidos Fast;
- 160.000 jugadores evolucionando;
- mercado de 6.400 clubes;
- 320 ligas cerrando temporada;
- 30 años macro;
- carga/guardado;
- materialización de liga;
- clasificación continental.

Detectar regresiones continuamente.

---

# 105. Objetivo de experiencia de rendimiento

El usuario no debe sentir:

> “está calculando todos los países”.

Avanzar debe sentirse casi instantáneo.

Si una transición estructural normal tarda muchos segundos, investigar arquitectura y perfilado.

---

# 106. Economía mundial y clubs macro

Aunque una liga no se observe, sus clubes necesitan cada temporada:

- ingresos;
- gastos;
- salarios;
- presupuesto;
- ventas;
- fichajes;
- deuda;
- inversión;
- mantenimiento.

Esto afecta su fuerza del año siguiente.

---

# 107. ClubSeasonState

Puede existir un estado anual compacto equivalente a:

```text
sportingStrength
financialStrength
reputation
facilities
academyStrength
fanbase
commercialStrength
managementQuality
```

La temporada N+1 deriva del estado de N.

---

# 108. Mercado macro

No simular 60 días de llamadas.

Resolver resultados estructurales:

- necesita delantero;
- vende jugador;
- recibe dinero;
- busca reemplazo;
- firma jugador.

Cuando el usuario consulte, la operación ya existe como hecho histórico.

---

# 109. Desarrollo macro de jugadores

Una liga dormida no necesita miles de acciones individuales.

Puede generar `PlayerSeasonPerformance` estadístico basado en:

- capacidad;
- minutos;
- rol;
- club;
- nivel;
- forma anual;
- lesiones;
- azar.

Después se usa el mismo pipeline que en liga detallada:

```text
Performance
 ↓
Form / Reputation / Development / MarketValue
```

---

# 110. Staff como parte del mundo macro

Entrenadores y empleados también deben:

- cambiar de club;
- jubilarse;
- mejorar;
- empeorar;
- recibir ofertas.

No mantener los mismos entrenadores durante 30 años simplemente porque su liga no se observa.

---

# 111. Historia

Debe conservarse de forma eficiente:

- campeones;
- clasificaciones;
- ascensos;
- descensos;
- títulos;
- fichajes;
- carreras;
- récords;
- grandes eventos;
- instalaciones importantes.

Una partida de 80 años debe tener memoria.

---

# 112. Persistencia

El agente debe proponer estrategia de persistencia capaz de manejar:

- mundo;
- personas;
- clubes;
- contratos;
- historia;
- reglas;
- seeds;
- resúmenes;
- knowledge;
- packs;
- versionado.

Debe existir:

- save version;
- migraciones;
- validación de dependencias;
- compatibilidad con mods.

---

# 113. Datos persistentes vs derivados

No guardar indiscriminadamente todo.

Ejemplos candidatos a derivados:

- media actual;
- valoración por posición;
- algunos índices UI.

Persistir hechos:

- contrato;
- fichaje;
- lesión;
- título;
- ascenso;
- instalación construida;
- relación importante;
- historial.

La propuesta de persistencia debe definir fuente de verdad.

---

# 114. Editor interno

Objetivo futuro/importante:

- clubes;
- jugadores;
- entrenadores;
- equipaciones;
- estadios;
- instalaciones;
- competiciones;
- reglas;
- bases de datos;
- packs.

Debe ser usable por personas que no programen.

---

# 115. Editor de competiciones

Idealmente visual y por fases.

Permitir:

- liga;
- grupos;
- eliminatoria;
- playoff;
- transiciones;
- ascenso;
- descenso;
- clasificación;
- inscripción;
- sanciones;
- reglas financieras;
- versionado temporal.

---

# 116. Editor de equipaciones

Permitir:

- color;
- patrón;
- mangas;
- cuello;
- escudo;
- sponsor;
- pantalón;
- medias;
- local;
- visitante;
- tercera;
- portero.

Previsualización inmediata en Godot.

---

# 117. Editor de instalaciones

Permitir crear/mejorar:

- gradas;
- restaurantes;
- tiendas;
- palcos;
- campos;
- gimnasios;
- centros médicos;
- residencias;
- cantera.

No hace falta 3D.

Puede ser 2D/modular.

---

# 118. UI de instalaciones

Resumen simple:

```text
Estadio        88
Entrenamiento  92
Cantera        81
Medicina       73
Analítica      64
```

Detalle:

```text
CALIDAD
ESTADO
MODERNIDAD
PERSONAL
PRESUPUESTO
MANTENIMIENTO
```

---

# 119. UI de jugador

Resumen:

```text
TÉCNICA 88
FORMA   92 ↑
FÍSICO  84
MENTAL  76 ~
MEDIA   86
```

## Pulsar TÉCNICA

Mostrar:

- atributos prioritarios;
- secundarios;
- portería;
- confianza;
- fecha;
- observador.

## Pulsar FORMA

Mostrar:

- rendimiento vs esperado;
- último mes;
- últimos 3 meses;
- tendencia;
- tamaño de muestra.

## Pulsar FÍSICO

Mostrar:

- fitness;
- fatiga;
- molestias;
- carga;
- recuperación.

## Pulsar MENTAL

Mostrar:

- confianza;
- motivación;
- motivación del partido;
- presión;
- complacencia;
- moral;
- precisión del conocimiento.

## Pulsar MEDIA

Mostrar:

- cómo se compone;
- posición actual;
- valoraciones por otras posiciones;
- familiaridad;
- pesos.

---

# 120. Química visual

Equipo:

```text
QUÍMICA 92 ↑
```

Detalle:

- cohesión;
- relación entrenador;
- conflictos;
- liderazgo;
- integración;
- confianza colectiva;
- justicia percibida.

---

# 121. Profesionales como gameplay

Contratar buen staff debe crear ventajas reales.

Un gran médico:

- diagnostica mejor;
- reduce incertidumbre;
- gestiona rehabilitación.

Un gran scout:

- mejora precisión;
- detecta gangas;
- evalúa posiciones alternativas.

Un buen psicólogo:

- conoce mejor mental;
- ayuda recuperación psicológica;
- reduce incertidumbre.

Un gran entrenador:

- mejora desarrollo;
- táctica;
- motivación;
- adaptación.

---

# 122. No omnisciencia como sistema de juego

Si falta un profesional, el club pierde información o eficacia.

Ejemplos:

- sin psicólogo → mental menos conocido;
- sin analista → rendimiento menos contextualizado;
- sin médico → riesgo médico menos fiable;
- sin scouts → conocimiento externo bajo.

La plantilla de empleados importa tanto por lo que hace como por lo que permite conocer.

---

# 123. Carrera del manager/gestor

Debe contemplarse una experiencia tipo carrera profesional además de control directo de un club.

Puede incluir:

- reputación;
- ofertas;
- despidos;
- dimisiones;
- progresión;
- historial;
- cambios de club.

El nombre comercial no necesita usar “Manager”.

---

# 124. No objetivos iniciales

Para proteger el alcance:

- no implementar 5 deportes de lanzamiento;
- no hacer 3D;
- no simular todos los partidos mundiales;
- no crear IA diaria por persona;
- no crear un simulador de vida completo;
- no crear un DSL gigante antes de necesitarlo;
- no sobrecargar la UI principal;
- no intentar licenciar todos los datos reales desde el principio.

---

# 125. Roadmap sugerido para propuestas

El agente deberá crear un roadmap real basándose en dependencias y `mcp-vertex`.

Una posible secuencia conceptual:

## Fase 0 — repositorio + mcp-vertex

- inspección;
- documentación;
- proposals;
- arquitectura;
- TestSport;
- benchmarks iniciales.

## Fase 1 — vertical slice de jugador

- Person;
- Football attributes;
- posición;
- técnica/forma/físico/mental/media;
- knowledge;
- UI mínima.

## Fase 2 — partido instantáneo

- snapshots;
- seed;
- alineación;
- banquillo;
- simulación;
- timeline;
- forma.

## Fase 3 — competiciones

- stages;
- rulesets;
- clasificación;
- ascensos;
- descensos;
- sanciones.

## Fase 4 — mundo macro

- SeasonTransition;
- historia;
- 80 países;
- materialización;
- commitments.

## Fase 5 — carreras

- desarrollo;
- lesiones;
- jubilaciones;
- jóvenes;
- profesionales;
- scouting.

## Fase 6 — economía

- contratos;
- valor de mercado;
- transferencias;
- negociación;
- salarios;
- inflación.

## Fase 7 — club profundo

- instalaciones;
- afición;
- prestigio;
- cantera;
- staff.

## Fase 8 — modding/editor

- packs;
- editores;
- import/export;
- validación.

---

# 126. Vertical slice 1 recomendado

Un primer slice podría demostrar:

- TestSport;
- FootballPack mínimo;
- dos clubes;
- 22 jugadores;
- atributos;
- posiciones;
- técnica/forma/físico/mental/media;
- knowledge;
- convocatoria;
- titulares;
- banquillo;
- simulación instantánea;
- timeline;
- seed;
- UI Godot;
- tests.

---

# 127. Vertical slice 2 recomendado

Demostrar mundo:

- un país;
- dos divisiones;
- ascensos;
- descensos;
- mercado;
- temporada macro;
- temporada detallada;
- jóvenes;
- jubilaciones;
- diez años.

---

# 128. Vertical slice 3 recomendado

Demostrar escala sin UI completa:

```text
80 países
4 divisiones
~6.400 clubes
~160.000 jugadores
30 años
```

Medir:

- tiempo;
- memoria;
- tamaño de save;
- economía;
- estabilidad.

---

# 129. Vertical slice 4 recomendado

Demostrar profundidad:

- scouting por atributo;
- empleados;
- lesiones;
- química;
- motivación;
- negociación;
- instalaciones;
- prestigio;
- afición.

---

# 130. Tests obligatorios

## Core

TestSport debe garantizar que Core no requiere fútbol.

## Competiciones

Miles de temporadas verificando:

- participantes;
- campeón;
- ascensos;
- descensos;
- clasificación;
- reglas.

## Economía

Décadas verificando:

- inflación;
- deuda;
- salarios;
- dinero;
- presupuestos.

## Personas

Verificar:

- edades;
- desarrollo;
- declive;
- lesiones;
- jubilaciones;
- jóvenes.

## Mercado

Verificar:

- ventas;
- reemplazos;
- contratos;
- ofertas;
- presupuestos.

## Instalaciones

Verificar:

- deterioro;
- modernidad;
- mantenimiento;
- cierres;
- inversión.

---

# 131. Benchmarks obligatorios

Crear y mantener benchmarks para:

- 10.000 partidos Fast;
- 160.000 `DevelopmentStep`;
- mercado de 6.400 clubes;
- 320 cierres de liga;
- 30 años macro;
- load/save;
- materialización;
- queries de scouting.

Usar benchmarks como guardrail arquitectónico.

---

# 132. Riesgos técnicos principales

El agente debe analizar al menos:

1. sobreabstracción temprana;
2. Core contaminado por fútbol;
3. rules engine demasiado complejo;
4. rendimiento de mundo macro;
5. mercado global ineficiente;
6. inflación económica;
7. saves gigantes;
8. materialización inconsistente;
9. mods inseguros;
10. UI demasiado compleja;
11. scouting difícil de entender;
12. diferencias entre simulación macro y detallada;
13. feedback loops explosivos;
14. deuda técnica por contenido de 80 países.

---

# 133. Preguntas que deben resolverse en proposals

## Arquitectura

- agregados;
- DDD vs data-oriented;
- ECS parcial;
- arrays;
- hot/cold data;
- comunicación Godot/Core.

## mcp-vertex

- cómo organiza propuestas;
- cómo documenta decisiones;
- cómo se integra con agentes;
- qué scaffold impone.

## Reglas

- formato declarativo;
- compilación;
- validación;
- versionado;
- escape hatches.

## Simulación

- modelo matemático;
- expected performance;
- forma;
- química;
- azar;
- calibración de fidelidades.

## Mundo macro

- SeasonTransition;
- materialización;
- commitments;
- historia.

## Economía

- inflación;
- salarios;
- TV;
- patrocinio;
- mercados.

## Knowledge

- confidence;
- aging de datos;
- observaciones;
- proyecciones;
- contradicciones.

## Persistencia

- SQLite;
- binario;
- híbrido;
- migraciones;
- packs.

---

# 134. Entregables esperados del agente

Tras leer este documento y la documentación local de `mcp-vertex`, el agente debería producir:

1. inventario del repositorio;
2. resumen de `mcp-vertex` aplicado al proyecto;
3. arquitectura propuesta;
4. mapa de módulos;
5. dependencias permitidas/prohibidas;
6. modelo de dominio inicial;
7. propuesta de packs;
8. propuesta de rules engine;
9. propuesta de persistencia;
10. propuesta de simulación mundial;
11. propuesta de conocimiento/scouting;
12. propuesta de rendimiento;
13. roadmap incremental;
14. riesgos;
15. decisiones abiertas;
16. proposals/ADRs siguiendo `mcp-vertex`.

---

# 135. Criterios de éxito técnico

La arquitectura será adecuada cuando:

1. fútbol pueda ser profundo;
2. 80 países evolucionen durante décadas;
3. avanzar sea rápido;
4. clubes cambien realmente;
5. personas tengan carreras emergentes;
6. scouting no sea omnisciente;
7. economía importe;
8. instalaciones importen;
9. reglas cambien por temporadas;
10. mods sean viables;
11. TestSport siga funcionando;
12. Godot sea sólo presentación.

---

# 136. Criterios de éxito de producto

El juego debe generar historias como:

- fiché a un desconocido y explotó;
- mi scout se equivocó;
- un lateral se convirtió en extremo estrella;
- un portero resultó ser buen jugador de campo;
- una leyenda se quedó tras el descenso;
- otro exigió salir inmediatamente;
- abandoné la cantera y diez años después era mediocre;
- amplié demasiado el estadio y casi arruiné al club;
- un club pequeño acabó siendo potencia;
- un gigante cayó durante décadas;
- un entrenador joven se convirtió en leyenda;
- una lesión cambió una carrera;
- una estrella tuvo una temporada terrible y perdió valor;
- un mediocre tuvo una temporada extraordinaria y engañó al mercado;
- una cantera generó una generación histórica;
- un empleado clave fue fichado por un rival.

Si estas historias emergen de los sistemas sin estar guionadas, PC Dynasty estará cumpliendo su propósito.

---

# 137. Resumen final del proyecto

PC Dynasty será inicialmente un juego mundial de gestión futbolística con espíritu de manager clásico, sin 3D y con simulación muy rápida.

El jugador podrá:

- gestionar plantilla;
- hacer alineaciones;
- convocar;
- elegir banquillo;
- entrenar;
- fichar;
- contratar empleados;
- desarrollar cantera;
- construir instalaciones;
- mantener estadio;
- controlar economía;
- negociar;
- analizar jugadores;
- observar scouting;
- gestionar competiciones;
- vivir décadas.

Cada jugador se resumirá mediante:

```text
TÉCNICA + FORMA + FÍSICO + MENTAL = MEDIA ACTUAL
```

La química es del equipo.

La motivación es contextual.

La forma se calcula respecto a lo que se esperaba de esa persona.

El conocimiento del club puede ser incompleto.

El futuro se proyecta, no se conoce.

Los empleados también evolucionan.

Los clubes tienen caché, afición, instalaciones, economía e historia.

El mundo evoluciona cada temporada aunque no se miren sus partidos.

No se simulan todos los partidos mundiales.

Las temporadas lejanas se resuelven estructuralmente.

Los hechos importantes se conservan.

Fútbol será el primer SportPack.

El Core debe poder soportar otros deportes en el futuro.

Godot presenta.

.NET simula.

`mcp-vertex` debe utilizarse siguiendo su documentación local y debe ser la base del flujo de propuestas/arquitectura cuando corresponda.

---

# 138. Última instrucción al agente

No convertir inmediatamente este documento en cientos de clases.

Primero:

1. leer `mcp-vertex`;
2. inspeccionar el scaffold;
3. comprender invariantes;
4. crear propuestas;
5. crear benchmarks;
6. construir vertical slices;
7. medir;
8. refinar.

No buscamos la abstracción más sofisticada posible.

Buscamos:

> **la abstracción mínima suficientemente potente para construir un gran manager de fútbol hoy y permitir que mañana PC Dynasty pueda convertirse, mediante packs comunitarios, en una plataforma de gestión de otros deportes.**
