# Matriz de cumplimiento permanente

| Invariante | Prevención | Evidencia de cierre |
| --- | --- | --- |
| Core sin Godot/fútbol/I-O | ensamblados separados y test de arquitectura | test de dependencias verde |
| SOLID y reutilización | revisión de propuesta, analizador y pruebas por contrato | interfaces estrechas y sin regla duplicada |
| Datos externos seguros | contrato de Reader, validador y compilador de pack | fixture válido, inválido y hash reproducible |
| Save compatible | manifiesto versionado, migrador y copia recuperable | tests de carga, migración y rollback |
| Determinismo | spec de semilla, orden total y golden replay | replay igual entre procesos y paralelo/serie |
| UI inmediata | work budget, read models y benchmarks | interacción dentro de presupuesto sin bloqueo |
| Rutas calientes eficientes | escenarios y budgets versionados | benchmark sin regresión aceptada |
| Estilo propio | guía de diseño y componentes compartidos | revisión visual sin activos ni layouts copiados |

Ninguna tarea se declara terminada si la fila aplicable no tiene evidencia. La
propuesta especifica la fila; CI ejecuta los checks automatizables; la revisión
independiente comprueba los límites de diseño que no se reducen a un linter.
