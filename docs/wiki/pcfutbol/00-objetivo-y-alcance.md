# 00 · Objetivo y alcance — **documento normativo**

> **El objetivo de PC Dynasty es un manager con la forma y la sensación de
> PC Fútbol 6.0, incorporando lo que 7.0 mejoró sobre él.**
>
> Todo lo demás que documenta esta wiki es **contexto**: de dónde salieron las
> ideas, cómo evolucionaron después y qué errores no vamos a repetir.

Este fichero fija a qué nos parecemos y a qué no. Cuando una decisión de diseño
admita varias lecturas, gana la de 6.0/7.0.

---

## 1. La versión objetivo

**PC Fútbol 6.0** (temporada 97/98, Dinamic Multimedia, noviembre de 1997), con
las mejoras de **PC Fútbol 7** (temporada 98/99).

Por qué esas dos y no otras:

| Motivo | Detalle |
|---|---|
| **Es el punto de equilibrio de la saga** | 6.0 tiene la profundidad de gestión completa (fichajes, finanzas, cantera, estadio, ver rival) sin la inestabilidad de las entregas posteriores |
| **Su interfaz es la más imitada** | Los sucesores espirituales — FX Fútbol, Unifútbol — copian deliberadamente esta estética, no la de 2001 ni la de 2005 `[prensa]` |
| **Es la que más vendió** | ~600.000 copias `[prensa]` |
| **Sigue viva** | La comunidad mantiene actualizadas 7 y 2001 veinte años después `[comunidad]` |
| **Es reproducible** | Su gramática es de disposición y densidad, no de tecnología: se puede hacer hoy con activos propios y sin depender de un motor 3D |

De **7.0** tomamos lo que mejoró sin romper nada: la integración de varias ligas
en un solo paquete (Euro PC Fútbol), el **Desafío Mundial** como torneo
configurable, y las condiciones explícitas de destitución (tres semanas de
déficit, plantilla por debajo de 16 jugadores).

## 2. Qué papel juega cada versión en esta wiki

| Versión | Papel | Qué le pedimos |
|---|---|---|
| 4.0 (1995) | **Cantera de ideas y única fuente primaria** | El manual oficial documenta reglas exactas: 18 roles, moral y forma, 12 % de interés, tres semanas de números rojos, ingresos por competición, marcajes por líneas `D`/`M`. Casi todo lo mecánico que sabemos con precisión viene de aquí |
| 5.0 (1996) | **Estructura del producto** | Las once secciones del disco y los cuatro perfiles de responsabilidad (Entrenador / Manager / Presidente / Total) |
| **6.0 (1997)** | **OBJETIVO** | Forma, aspecto, navegación, densidad, alcance de la gestión |
| **7.0 (1998)** | **OBJETIVO (mejoras)** | Multiliga, Desafío Mundial, reglas de destitución |
| 2000 / 2001 | **Evolución deseable** | EuroManager, empleados con función propia, mercado de invierno, envejecimiento y estadísticas persistentes. Ideas a incorporar **sin** heredar su interfaz |
| 2005–2007 | **Aviso** | Qué destruye un manager: regresión de funciones, datos falsos, inestabilidad. Ver [`11`](11-recepcion-y-lecciones.md) |
| 2018 / 8 | **Aviso** | Qué pasa al simplificar la densidad para llegar a más público |

## 3. Reglas de decisión

1. **Ante la duda, 6.0.** Si 6.0 y una entrega posterior resuelven lo mismo de
   forma distinta, se implementa la de 6.0 salvo que exista una razón escrita.
2. **Lo posterior se añade, no sustituye.** Una idea de 2001 (empleados con
   mecánica propia, mercado de invierno) entra **encima** de la estructura de
   6.0, nunca reemplazando su navegación.
3. **Lo anterior aporta números, no forma.** Las reglas de 4.0 son la mejor
   referencia mecánica que tenemos, pero su disposición de pantalla es de 1995 y
   6.0 ya la superó.
4. **Nada de activos.** Ni arte, ni fuentes, ni sonidos, ni capturas, ni marcas,
   ni nombres reales. La semejanza es de **gramática**; el material es propio.
   Ver [`14-catalogo-visual.md`](14-catalogo-visual.md) sobre cómo trabajamos
   con referencias visuales sin copiarlas.
5. **Resolución.** El original vivía en 640×480. Nosotros heredamos su
   **densidad**, no su tamaño: todo se ancla y escala, y una pantalla debe
   funcionar de 1024×600 a 2560×1440.

## 4. Qué significa "similar a PC Fútbol 6.0" en la práctica

Cuando alguien juzgue si una pantalla nuestra cumple el objetivo, estas son las
preguntas, en orden:

1. ¿Hay una **banda superior** con la identidad de modo a la izquierda, el
   **nombre de la pantalla centrado en versales anchas** y la **placa de
   calendario** con la jornada a la derecha?
2. ¿Está el contenido dentro de **placas con barra de rótulo de color**, sin
   nada flotando sobre el fondo?
3. ¿La información numérica está en una **tabla densa** con abreviaturas de
   dos o tres letras y una fila de cabecera de otro color?
4. ¿Existen **destinos permanentes en un raíl lateral** en vez de menús
   desplegables?
5. ¿Está la **salida abajo a la derecha**, siempre en el mismo sitio?
6. ¿Se puede **pinchar un dato** para ver de dónde sale?
7. ¿Cabe todo sin desplazar, o al menos la parte que sostiene la decisión?

Si la respuesta a todas es sí, la pantalla cumple. La lista larga está en
[`04-gramatica-visual.md`](04-gramatica-visual.md) §6.

## 5. Dónde está cada cosa

- La descripción a fondo de la versión objetivo: [`13-version-objetivo-6-0-y-7-0.md`](13-version-objetivo-6-0-y-7-0.md).
- El catálogo visual con referencias y wireframes: [`14-catalogo-visual.md`](14-catalogo-visual.md).
- El inventario de pantallas de toda la saga: [`03-mapa-de-pantallas.md`](03-mapa-de-pantallas.md).
- Lo que decidimos añadir por nuestra cuenta: [`12-mejoras-para-pc-dynasty.md`](12-mejoras-para-pc-dynasty.md).
