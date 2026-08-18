# Invariantes permanentes de PC Dynasty

Estas reglas se aplican a toda propuesta, módulo, test, herramienta y cambio
futuro. Sólo pueden relajarse mediante una decisión de arquitectura explícita,
con riesgo, alternativa y plan de reversión documentados.

## Arquitectura y contratos

1. Godot presenta; el Core .NET modela y simula. El Core no depende de Godot,
   UI, escena, asset, JSON, SQL ni un deporte concreto.
2. Las integraciones externas llegan por contratos pequeños, inmutables,
   versionados y testeados. No se exponen entidades mutables ni tipos de un
   proveedor externo.
3. Bases de datos y contenido externos se cargan mediante Readers/Importers,
   se validan, normalizan y compilan a packs inmutables. CSV, JSON y SQLite son
   fuentes, no modelos del dominio.
4. Los packs comunitarios son declarativos y seguros por defecto; no ejecutan
   código, procesos, red ni acceden a rutas arbitrarias.
5. El Core usa IDs y estructuras compactas en rutas calientes. Los nombres
   externos se resuelven al cargar el contenido.

## Calidad de diseño y código

1. SOLID se aplica de forma pragmática: una responsabilidad por tipo, puertos
   estrechos, dependencia hacia abstracciones y sustitución verificable. No se
   introduce una capa o patrón que no reduzca acoplamiento real.
2. Los nombres expresan el dominio; las funciones son pequeñas; las entradas
   se validan en bordes; los errores normales son resultados tipados y no
   excepciones escondidas.
3. Se reutiliza comportamiento estable mediante políticas, servicios o
   contratos cohesionados. Se prohíbe copiar una regla entre Core, UI,
   importadores o simuladores.
4. No hay código muerto, ramas comentadas, números mágicos, dependencias
   circulares ni comentarios que sólo repiten el código.
5. Cada módulo declara sus dependencias permitidas y se añade una prueba de
   arquitectura cuando una frontera sea crítica.

## Corrección y reproducibilidad

1. El orquestador es el único dueño del avance temporal y aplica hechos en un
   orden total determinista.
2. Igual save, contrato, reglas, semilla y comando producen los mismos hechos
   ordenados, independientemente de proceso o paralelismo.
3. Macro, Detailed y Materialize comparten compromisos históricos; ninguna
   fidelidad reescribe resultados confirmados.
4. Los saves persisten hechos y checkpoints versionados; los derivados se
   reconstruyen. Migrar nunca destruye la entrada recuperable.
5. TestSport protege que el Core siga siendo independiente de fútbol.

## Experiencia de uso y rendimiento

1. Las acciones del jugador deben sentirse inmediatas. La UI nunca espera una
   simulación global ni muestra datos parciales como definitivos.
2. El trabajo largo se presupuesta, se divide en checkpoints y se reanuda sin
   bloquear input, navegación ni renderizado.
3. Cada ruta caliente dispone de benchmark y presupuesto de tiempo, memoria y,
   cuando aplique, tamaño de save. Una regresión se investiga antes de escalar
   contenido.
4. La interfaz es una creación original: navegación rápida, información densa
   y profundidad progresiva inspiradas en managers clásicos, sin copiar la
   apariencia, assets, texto ni identidad de PC Fútbol.
5. Toda pantalla aplica el patrón resumen → detalle → razones/historial y usa
   componentes reutilizables en lugar de lógica de escena duplicada.

## Método de entrega

1. Cada cambio relevante se formula y se ejecuta mediante propuesta MCP con
   archivos acotados, aceptación, pruebas y benchmark cuando corresponda.
2. Primero se escribe una prueba que fije el comportamiento; después la
   implementación mínima y una refactorización con la suite verde.
3. Los commits son pequeños, convencionales y sólo incluyen los archivos de la
   tarea. Se publican desde una rama aislada cuando no interfieren con trabajo
   concurrente.
4. No se omite una puerta de calidad por urgencia. Si falta una herramienta o
   SDK, se registra como bloqueo de infraestructura y se resuelve antes de
   declarar la tarea lista.
