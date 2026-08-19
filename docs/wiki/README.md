# Wiki — el manager clásico como referencia de diseño

> ## El objetivo es **PC Fútbol 6.0** (con las mejoras de **7.0**)
>
> Todo lo demás que hay aquí es **contexto**: de dónde salieron las ideas (4.0 y
> 5.0), cómo evolucionaron después (2000, 2001) y qué errores no vamos a repetir
> (2005 en adelante).
>
> Antes de decidir nada de interfaz o de alcance, lee
> **[`00-objetivo-y-alcance.md`](pcfutbol/00-objetivo-y-alcance.md)** — es
> normativo — y luego
> **[`13-version-objetivo-6-0-y-7-0.md`](pcfutbol/13-version-objetivo-6-0-y-7-0.md)**,
> que describe la versión objetivo pantalla a pantalla.

Esta carpeta es la **base de conocimiento** de PC Dynasty sobre los managers de
fútbol clásicos de los noventa, con PC Fútbol (Dinamic Multimedia, 1992–2001) como
caso de estudio. No es documentación de nuestro código: es la referencia que
cualquier agente debe leer **antes** de diseñar una pantalla, un flujo o un
sistema de juego, para que las decisiones tengan un origen documentado en vez de
salir de la intuición de quien toque el fichero ese día.

> **Aviso de propiedad intelectual.** Aquí se documenta *cómo funcionaba* un
> producto ajeno: su estructura de pantallas, su gramática de interacción y sus
> reglas de juego. Nada de lo que se describe autoriza a copiar material: ni
> ilustraciones, ni fuentes, ni sonidos, ni capturas, ni nombres de marcas, clubes,
> competiciones o personas reales. PC Dynasty reproduce **gramática**, nunca
> **activos**. Ver [`99-fuentes.md`](pcfutbol/99-fuentes.md).

## Cómo la usa un agente

| Si vas a… | Lee primero |
|---|---|
| Cualquier cosa, la primera vez | [`00-objetivo-y-alcance.md`](pcfutbol/00-objetivo-y-alcance.md) — **normativo**: a qué nos parecemos |
| Diseñar o retocar **cualquier** pantalla | [`04-gramatica-visual.md`](pcfutbol/04-gramatica-visual.md) — **normativo** — y [`13`](pcfutbol/13-version-objetivo-6-0-y-7-0.md) §7 |
| Reproducir una pantalla concreta del objetivo | [`13-version-objetivo-6-0-y-7-0.md`](pcfutbol/13-version-objetivo-6-0-y-7-0.md) |
| Buscar la captura original de referencia | [`14-catalogo-visual.md`](pcfutbol/14-catalogo-visual.md) |
| Decidir dónde vive un botón o cómo se navega | [`05-interaccion-y-controles.md`](pcfutbol/05-interaccion-y-controles.md) |
| Crear una pantalla nueva | [`03-mapa-de-pantallas.md`](pcfutbol/03-mapa-de-pantallas.md) |
| Implementar una regla de simulación | [`06-sistemas-deportivos.md`](pcfutbol/06-sistemas-deportivos.md) |
| Tocar dinero, estadio, crédito o primas | [`07-economia-y-club.md`](pcfutbol/07-economia-y-club.md) |
| Tocar fichajes, cláusulas u ofertas | [`08-mercado-y-fichajes.md`](pcfutbol/08-mercado-y-fichajes.md) |
| Tocar calendario, copas o Europa | [`09-competiciones-y-calendario.md`](pcfutbol/09-competiciones-y-calendario.md) |
| Tocar base de datos, fichas o seguimiento | [`10-datos-seguimiento-y-base-de-datos.md`](pcfutbol/10-datos-seguimiento-y-base-de-datos.md) |
| Justificar por qué NO copiamos algo | [`11-recepcion-y-lecciones.md`](pcfutbol/11-recepcion-y-lecciones.md) |
| Proponer alcance nuevo | [`12-mejoras-para-pc-dynasty.md`](pcfutbol/12-mejoras-para-pc-dynasty.md) |
| Saber qué pantalla nuestra corresponde a cuál | [`pc-dynasty/mapa-de-equivalencias.md`](pc-dynasty/mapa-de-equivalencias.md) |

## Índice completo

### Objetivo

0. [Objetivo y alcance](pcfutbol/00-objetivo-y-alcance.md) — **normativo**: la versión objetivo, qué papel juega cada entrega y las reglas de decisión.
13. [La versión objetivo: 6.0 y 7.0](pcfutbol/13-version-objetivo-6-0-y-7-0.md) — pantalla a pantalla, con wireframes.
14. [Catálogo visual](pcfutbol/14-catalogo-visual.md) — referencias originales enlazadas, y la política de por qué no se copian.

### Referencia del clásico

1. [Historia y versiones](pcfutbol/01-historia-y-versiones.md) — la saga entera, qué añadió cada entrega y por qué murió.
2. [Modos de juego](pcfutbol/02-modos-de-juego.md) — Arcade, Entrenador, Manager, Pro-Manager, EuroManager y el resto del disco.
3. [Mapa de pantallas](pcfutbol/03-mapa-de-pantallas.md) — inventario pantalla a pantalla con su estructura.
4. [Gramática visual](pcfutbol/04-gramatica-visual.md) — **normativo**: placas, biseles, barras de rótulo, tablas densas, raíles, barra de mando.
5. [Interacción y controles](pcfutbol/05-interaccion-y-controles.md) — ratón, teclado, ventanas de detalle, iconos, tooltips, latencia.
6. [Sistemas deportivos](pcfutbol/06-sistemas-deportivos.md) — parámetros, roles, moral, forma, energía, lesiones, sanciones, entrenamiento, tácticas.
7. [Economía y club](pcfutbol/07-economia-y-club.md) — caja, ingresos, gastos, crédito, primas, precios, estadio, empleados.
8. [Mercado y fichajes](pcfutbol/08-mercado-y-fichajes.md) — transferibles, cláusula, ofertas, representantes, cantera, cesiones.
9. [Competiciones y calendario](pcfutbol/09-competiciones-y-calendario.md) — liga, copa, europeas, formatos, ingresos por ronda, ligas consecutivas.
10. [Datos, seguimiento y base de datos](pcfutbol/10-datos-seguimiento-y-base-de-datos.md) — fichas, comparativas, clasificaciones, actualizaciones.
11. [Recepción y lecciones](pcfutbol/11-recepcion-y-lecciones.md) — qué elogió y qué destrozó la crítica, versión a versión.
12. [Mejoras para PC Dynasty](pcfutbol/12-mejoras-para-pc-dynasty.md) — qué conservar, qué corregir, qué añadir.
99. [Fuentes](pcfutbol/99-fuentes.md) — de dónde sale cada dato y qué fiabilidad tiene.

### Aplicación a este proyecto

- [Mapa de equivalencias](pc-dynasty/mapa-de-equivalencias.md) — cada pantalla del clásico contra la nuestra, con el hueco pendiente.

## Reglas de esta wiki

1. **Cada afirmación de hecho lleva origen.** Si un dato viene del manual oficial se
   marca `[manual 4.0]`; si viene de una enciclopedia, `[wiki]`; si es una opinión
   recogida de prensa o comunidad, `[prensa]` / `[comunidad]`. Lo que sea deducción
   nuestra va marcado como **inferencia**.
2. **Separar hecho de propuesta.** Los ficheros 01–11 describen el producto ajeno.
   El 12 y la carpeta `pc-dynasty/` contienen lo que *nosotros* decidimos hacer.
3. **La gramática visual es normativa.** Un cambio de UI que la contradiga es un
   defecto, no una variante de estilo.
4. **Ante la duda, 6.0.** Cuando dos entregas resuelven lo mismo de forma
   distinta, gana la versión objetivo salvo razón escrita.
5. **Nunca material ajeno.** Ni un byte de arte, audio, fuente, captura o dato con
   licencia entra en el repositorio. Las referencias visuales se **enlazan** y se
   **describen**; los wireframes son propios. Ver [`14`](pcfutbol/14-catalogo-visual.md) §1.
