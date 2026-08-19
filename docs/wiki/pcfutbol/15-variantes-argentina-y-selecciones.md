# 15 · Las variantes: Argentina, Italia y las selecciones

Dinamic publicaba **tres versiones de cada entrega** — España, Italia y
Argentina — y además una línea aparte para competiciones de selecciones. `[prensa]`
Comparten motor y gramática con la versión objetivo, así que sirven para lo que
ninguna captura de 6.0 puede darnos: **ver la misma gramática resuelta de otra
manera**, y saber por tanto qué parte es esencial y qué parte es decoración.

> Recordatorio: el objetivo sigue siendo 6.0/7.0
> ([`00`](00-objetivo-y-alcance.md)). Lo de este capítulo es contexto y catálogo
> de ideas, no un segundo objetivo.

---

> **Alcance ampliado (2026-08-19).** Este capítulo se escribió conociendo sólo
> PC Fútbol Argentina 4.0 y PC Selección '96. El catálogo preservado
> ([`01`](01-historia-y-versiones.md) §"El catálogo preservado") demuestra que
> ambas líneas siguieron: la argentina llegó a **5.0 Apertura '97** y **6.0
> Apertura '98**, y hubo un segundo título de selecciones, **PC Selección
> Mundial '98**. Lo que sigue vale para las versiones que sí se han leído; las
> otras cuatro están sin describir y son trabajo pendiente.

## 1. La línea argentina

| Entrega | Torneo | Notas `[wiki]` `[comunidad]` |
|---|---|---|
| PC Fútbol Argentina Clausura '95 (3.40) | Clausura 1995 | Primera argentina de la saga, ya con Pro-Manager |
| **PC Fútbol Argentina 4.0** | Apertura '96 | La que mejor documentada tenemos |
| PC Fútbol 5.0 — Liga Argentina | Apertura '97 | Narración local |
| PC Fútbol 6 — Apertura '98 | Apertura '98 | Contemporánea de nuestra versión objetivo |
| Ediciones de club (p. ej. PC River) | Clausura 2000 | Reempaquetados por club |

### 1.1 El menú principal argentino — la misma gramática, otra forma

```
┌───────────────────────────────────────────────────────────┬─────────┐
│ ⚽  P C   F U T B O L   (logotipo grande, banda de color) ⚽│         │
├──────────────┬────────────────────────────────────────────┤ ▣ ▣ ▣ ▣ │
│  ╔════════╗  │ ┌──── ARGENTINA (banda con los colores ───┐ │ ▣ ▣ ▣ ▣ │
│  ║  4.0   ║  │ │        de la bandera del país)          │ │ ▣ ▣ ▣ ▣ │
│  ║APERTURA║  │ ⚽ ①  Base de datos ───────────────────── │ │ ▣ ▣ ▣ ▣ │
│  ╚════════╝  │ ⚽ ②  Seguimiento del Torneo ──────────── │ │ ▣ ▣ ▣ ▣ │
│              │ ⚽ ③  Partido amistoso ────────────────── │ │ (rejilla│
│   🏆 trofeo  │ ⚽ ④  Desafío América-Europa ──────────── │ │ de      │
│   grande     │ ⚽ ⑤  Torneo Arcade ───────────────────── │ │ escudos)│
│              │ ⚽ ⑥  Torneo D.T. ─────────────────────── │ │         │
│ [CONFIGURAC.]│ ⚽ ⑦  Torneo Manager ──────────────────── │ │   ⚽    │
│ [INFORMACIÓN]│ ⚽ ⑧  Torneo Pro-Manager ──────────────── │ │  grande │
│ [SALIR]      │ ⚽ ⑨  InfoFútbol ──────────────────────── │ │         │
└──────────────┴────────────────────────────────────────────┴─────────┘
                              editora        ver. 4.0
```

Lo que enseña, y es mucho: `[capturas]`

1. **Los destinos van numerados y la cifra lleva color de familia.** ① ② en azul
   (consultar), ③ a ⑧ en rojo (jugar), ⑨ en verde (información). El número da
   atajo de teclado y el color da la agrupación **sin necesidad de separadores ni
   de títulos de grupo**. Es la misma idea que nuestro color por área, aplicada a
   una lista en vez de a una rejilla.
2. **La rejilla de escudos vive en el raíl derecho del menú principal**, no dentro
   de una pantalla de selección. El menú ya dice de qué liga va esto.
3. **La banda del país usa los colores de su bandera.** La identidad del mercado
   es un cambio de paleta sobre la misma estructura.
4. **Lo auxiliar baja a botones pequeños en una esquina fija**: configuración,
   información y salir, siempre abajo a la izquierda.
5. **La versión, visible y discreta**, en dos sitios.

> Comparado con el menú de mosaicos de 6.0
> ([`13` §2](13-version-objetivo-6-0-y-7-0.md)), esto demuestra que **la regla es
> "todos los destinos visibles y agrupados por color", no "mosaicos"**. Los
> mosaicos son la forma; la regla es lo que hay que respetar.

### 1.2 La pantalla HISTORIAL — una tabla preparada para comparar

```
┌ ⚽ HISTORIAL ─┬───────── NOMBRE DEL CLUB ─────────┬── 🏆 ──┐
├──────────────┴───────────────────────────────────┴────────┤
│ ▣escudo   ┌─────────────────────┬─ CAMPEÓN ─┐             │
│           │ COMPETICIÓN A       │     0     │   logotipo  │
│           │ COMPETICIÓN B       │     0     │   pequeño   │
│           │ COMPETICIÓN C       │     0     │             │
│           └─────────────────────┴───────────┘             │
│ ┌───────────────────────────────────────────┐ ┌─────────┐ │
│ │  🏆      ┌ ▣ Nombre del club ──────────┐  │ │[Comparar]│ │
│ │ trofeo   ├──────┬──────────────────┬───┤  │ │[Torn. XX]│ │
│ │ grande   │   65 │ TEMPORADAS       │   │  │ │[Imprimir]│ │
│ │          │    1 │ CAMPEONATOS      │   │  │ │[Volver]  │ │
│ │          │ 1997 │ PARTIDOS JUGADOS │   │  │ └─────────┘ │
│ │          │  644 │ PARTIDOS GANADOS │   │  │             │
│ │          │ 2935 │ GOLES A FAVOR    │   │  │   Ⓜ Ⓢ ?    │
│ │          └──────┴──────────────────┴───┘  │             │
│ └───────────────────────────────────────────┘             │
└───────────────────────────────────────────────────────────┘
                              ↑ columna vacía: ahí entra el rival
```

Dos ideas que merecen entrar en nuestra gramática: `[capturas]`

- **La tabla ya tiene el hueco de la comparación.** La columna derecha está
  dibujada y vacía desde el principio; pulsar `Comparar` la rellena. La pantalla
  no cambia de tamaño ni de forma: es el mismo principio que las ranuras vacías
  de «Estado de las ofertas» ([`04` §3.19](04-gramatica-visual.md)).
- **Etiqueta al centro, cifras a los lados.** Con dos entidades, poner el rótulo
  en medio y un número a cada lado se lee sin mover los ojos en zigzag.

### 1.3 Y una lección de paleta

Esta entrega usa un cromo **gris claro** donde 6.0 usa azul marino, y la
gramática aguanta sin un solo cambio estructural: bandas, chips de color, tablas
densas, raíl de botones con icono, salida abajo a la derecha.

> Es la mejor prueba disponible de que nuestras paletas alternativas (incluida
> «Blanco Papel») son legítimas: **el estilo no es el color, es la disposición**.

---

## 2. La línea de selecciones

**PC Selección Española de Fútbol — Eurocopa '96** (Dinamic Multimedia, 1996) fue
la primera de la casa basada en una competición de selecciones. `[wiki]`

| Aspecto | Detalle `[wiki]` |
|---|---|
| Modos | Partido preparatorio · Eurocopa '96 · **Seleccionador Nacional** |
| Seleccionador | Eliges 22 jugadores de entre todos los de Primera de la temporada 95/96, y defines formación y táctica |
| Otras selecciones | Se puede dirigir cualquiera de las 16 participantes, pero sin elegir convocatoria |
| Histórico | Todas las ediciones desde 1960: fases finales, alineaciones, goleadores y tarjetas |
| Simulador | Estadios adaptados: gradas con columnas, la ciudad asomando por las esquinas, público pegado a la banda |

Lo aprovechable para nosotros, y es más de lo que parece:

1. **Un torneo corto es otro producto dentro del mismo motor.** Sin mercado, sin
   finanzas, sin temporada: **una convocatoria, un cuadro y siete partidos**. Es
   exactamente la forma de un modo secundario barato de construir sobre lo que ya
   tenemos.
2. **La convocatoria es una mecánica en sí misma.** Elegir 22 de entre cientos,
   con criterio propio y sin dinero de por medio, es una decisión distinta a
   fichar y merece su propia pantalla.
3. **El archivo histórico como contenido de primera.** Desde 1960, con alineaciones
   y goleadores: la misma apuesta que la base de datos de la línea principal.
4. **El estadio tiene carácter.** Que las gradas cambien de forma y se vea la
   ciudad detrás es la clase de detalle que no toca la simulación y sí la
   credibilidad — la misma lección que el árbitro con jueces de línea de 4.0
   ([`06` §7](06-sistemas-deportivos.md)).

---

## 3. Qué nos llevamos

| # | Idea | Origen | Destino |
|---|---|---|---|
| V1 | Destinos numerados con la cifra coloreada por familia | Menú argentino | [`04` §3.14](04-gramatica-visual.md), menú principal |
| V2 | Rejilla de escudos en el propio menú principal | Menú argentino | [`04` §3.12](04-gramatica-visual.md) |
| V3 | Identidad de mercado = cambio de paleta sobre la misma estructura | Menú argentino | Confirma nuestras paletas alternativas |
| V4 | La tabla dibuja el hueco de la comparación antes de tener con qué llenarlo | Historial argentino | [`12`](12-mejoras-para-pc-dynasty.md) C12 |
| V5 | Etiqueta al centro, cifras a los lados, al comparar dos entidades | Historial argentino | [`04` §3.3](04-gramatica-visual.md) |
| V6 | Un torneo corto como modo secundario sobre el mismo motor | PC Selección | [`12`](12-mejoras-para-pc-dynasty.md), candidato nuevo |
| V7 | La convocatoria como mecánica propia, distinta del fichaje | PC Selección | [`12`](12-mejoras-para-pc-dynasty.md), candidato nuevo |
