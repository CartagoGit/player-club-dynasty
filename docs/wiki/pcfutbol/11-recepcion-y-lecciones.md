# 11 · Recepción y lecciones

Qué dijeron la crítica y la comunidad, y qué se deduce para PC Dynasty. Todo lo de
este fichero es `[prensa]` o `[comunidad]` salvo donde se marca otra cosa.

## 1. Lo que se elogió, siempre

- **La gestión.** En el análisis de PC Fútbol 2001 el sistema de gestión se calificó
  de *insuperable*, con una profundidad adictiva capaz de dar «horas y horas de
  entretenimiento» sólo con finanzas y fichajes.
- **La accesibilidad.** 2.0 se recuerda como ideal para quien nunca había tocado un
  manager. La interfaz de iconos se describía como «muy intuitiva».
- **La base de datos.** Reconocida como enciclopedia del fútbol español; usada por
  periodistas como herramienta de trabajo. `[manual 4.0]`
- **El diseño gráfico de 2001**, considerado el mejor de la saga.
- **La densidad.** El propio análisis crítico de la versión móvil de 2018 lo dice al
  revés: en PC, «el manejo con ratón permitía una saturación de información en
  pantalla que no es posible en móviles». La saturación **era la virtud**.

## 2. Lo que la hundió

| Problema | Versión | Detalle |
|---|---|---|
| Motor de partido 3D | 2001 | «pésimo», «jugabilidad lamentable»; portero deficiente, física irreal, disparo desequilibrado |
| Motor de partido 3D | 2007 | «monigotes grotescos recorriendo el campo» |
| Calidad de datos | 2005 | ~40 defectos: fichas erróneas, entrenadores mal colocados, equipaciones incorrectas pese a la licencia |
| Reglas mal implementadas | 2005 | **ascensos resueltos por orden alfabético** en categorías inferiores; fechas de competiciones no ligueras ausentes del calendario |
| Estabilidad | 2005 | caídas de la aplicación, fallos de lectura |
| Online | 2001 / 2005 | «tirones constantes» y ventaja del anfitrión; resultados divergentes entre jugadores |
| Amputación de funciones | 2005 | fuera Pro-Manager, empleados (ojeador, segundo entrenador, psicólogo), merchandising, nacionalizaciones y partido interactivo |
| Relación con la comunidad | 2005/2006 | cierre de foros oficiales y de servidores ante las quejas |
| Interfaz | 2006 | «rojos irritantes» corregidos en 2007 volviendo a los azules |
| Interfaz | 2018 | menús que «no responden como deberían» en móvil |

## 3. Las seis lecciones, en orden de importancia para nosotros

### L1 · Un manager se juzga por su peor componente visible

Gestión *insuperable* + 3D *pésimo* = juego suspendido. No compensa. Si no podemos
hacer bien una pieza, **es mejor no enseñarla** que enseñarla mal: el modo
`Resultado` y el `Visionado` textual eran perfectamente respetados; el 3D malo no.

> Aplicación directa: mientras nuestro visor de partido no esté a la altura, el
> camino honesto es el informe de partido bien hecho.

### L2 · La regresión de funciones es imperdonable

Quitar en la versión N algo que existía en la N-1 destruye más reputación que no
haberlo tenido nunca. Aplicado a un repositorio: **una migración de UI que deja
pantallas peor que antes es una regresión**, aunque la arquitectura sea mejor.

### L3 · Un error de datos vale por diez errores de código

Los ascensos por orden alfabético hicieron más daño que cualquier caída. El jugador
perdona un fallo técnico; no perdona que el mundo simulado sea **falso**. Esto
justifica el peso de nuestro `PackValidator` y de las pruebas de reglas.

### L4 · La interfaz es contenido

Los cambios de interfaz aparecen en los análisis con el mismo peso que las mecánicas
(«rojos irritantes», «menús que no responden», «la interfaz totalmente renovada de
2000»). Una pantalla que se sale del marco o un botón que no se puede pulsar no son
detalles cosméticos: **son el análisis**.

### L5 · Densidad, no simplificación

Cuando la saga intentó simplificar para llegar a más público (móvil, 2018) perdió lo
que la hacía especial. El público de un manager **quiere** la tabla llena de números.
Simplificar el aspecto está bien; simplificar la información, no.

### L6 · La comunidad es infraestructura

Que las ediciones 7 y 2001 sigan actualizadas por aficionados veinte años después es
consecuencia de que los datos fueran **abiertos y editables**. Nuestro formato de
paquetes declarativos y el editor son, en ese sentido, una decisión estratégica, no
una comodidad de desarrollo.

## 4. Qué eligió la gente cuando pudo elegir

Con toda la saga disponible, la comunidad se concentró en **7.0 y 2001** — las dos
más completas. No en la más bonita ni en la más reciente. **Completitud gana.**
