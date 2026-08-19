# 14 · Catálogo visual

Índice de las referencias gráficas que sostienen esta wiki: dónde está cada
captura original, qué pantalla muestra, de qué versión es y dónde vive su
descripción estructural y su wireframe propio.

---

## 1. Política: por qué aquí hay enlaces y no imágenes

**En este repositorio no entra ninguna captura de PC Fútbol.** Las capturas son
obra ajena, y la regla que gobierna todo el proyecto es que reproducimos
*gramática*, no *material* (ver [`00`](00-objetivo-y-alcance.md) §3.4 y
[`99-fuentes.md`](99-fuentes.md)).

Lo que sí hacemos, y da el mismo resultado práctico:

1. **Enlazar** la captura en su fuente original, con su URL exacta.
2. **Describirla estructuralmente**: zonas, jerarquía, colores por función,
   controles, estados. Un agente puede reconstruir la pantalla desde la
   descripción sin haber visto la imagen.
3. **Dibujar un wireframe propio** en el capítulo correspondiente. El wireframe
   es obra nuestra y sí vive en el repositorio.

Para trabajar con una referencia: descárgala a un directorio temporal fuera del
repositorio, míralas, escribe lo que veas, y no la commitees.

```bash
# Ejemplo — directorio temporal, NUNCA dentro del repo
mkdir -p /tmp/pcd-refs && cd /tmp/pcd-refs
curl -sO https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-4.png
```

---

## 2. Referencias catalogadas

### 2.1 PC Fútbol 6.0 — la versión objetivo

| # | Pantalla | Fuente | Wireframe / descripción |
|---|---|---|---|
| V1 | **Menú principal** (9 mosaicos + 4 iconos) | [CEZ 177-pc-futbol-60-pc-4.png](https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-4.png) | [`13` §2](13-version-objetivo-6-0-y-7-0.md) |
| V2 | **Menú principal, edición X2** (con PCFútbol Plus 98-99) | [CEZ 1449-pc-futbol-60-1.jpg](https://computeremuzone.com/fichas-pantallas/p/1449-pc-futbol-60-1.jpg) | ídem — muestra cómo el menú lista las extensiones instaladas |
| V3 | **Ficha de jugador**, base de datos, doble pestaña | [CEZ 1448-pc-futbol-60-2.jpg](https://computeremuzone.com/fichas-pantallas/p/1448-pc-futbol-60-2.jpg) | [`13` §4.6](13-version-objetivo-6-0-y-7-0.md) |
| V4 | **Estado de las ofertas**, con placa de empleado | [CEZ 1449-pc-futbol-60-2.jpg](https://computeremuzone.com/fichas-pantallas/p/1449-pc-futbol-60-2.jpg) | [`13` §4.5](13-version-objetivo-6-0-y-7-0.md) |
| V5 | **Las últimas 10 ligas** — comparativa histórica | [CEZ 177-pc-futbol-60-pc-5.png](https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-5.png) | [`13` §4.7](13-version-objetivo-6-0-y-7-0.md) |
| V6 | **Salida al campo** — onces y estadio | [CEZ 177-pc-futbol-60-pc-6.png](https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-6.png) | [`13` §4.9](13-version-objetivo-6-0-y-7-0.md) |
| V6b | **Ficha de club** — estadio, presupuesto, socios, patrocinador | [CEZ 177-pc-futbol-60-pc-3.png](https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-3.png) | [`13` §4.8](13-version-objetivo-6-0-y-7-0.md) |
| V7 | **Pretemporada Promanager** — mapa de Europa | [CEZ 177-pc-futbol-60-pc-2.png](https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-2.png) | [`13` §5](13-version-objetivo-6-0-y-7-0.md) |
| V8 | **Simulador 3D** en partido | [CEZ 177-pc-futbol-60-pc-1.png](https://computeremuzone.com/fichas-pantallas/p/177-pc-futbol-60-pc-1.png) | Sin wireframe: fuera de alcance a corto plazo, ver [`12`](12-mejoras-para-pc-dynasty.md) B1 |
| V9 | **Simulador**, extensión | [CEZ 1448-pc-futbol-60-1.jpg](https://computeremuzone.com/fichas-pantallas/p/1448-pc-futbol-60-1.jpg) | ídem |

Página de la ficha con todas las capturas y las notas de prensa de la época:
<https://computeremuzone.com/ficha/177/pc-futbol-60-temporada-97-98>

### 2.2 PC Fútbol 7

| # | Pantalla | Fuente |
|---|---|---|
| V10 | **Alineación** — la pantalla eje, en vista `CALIFICACIÓN` | [CEZ 253-pc-futbol-7-1.jpg](https://computeremuzone.com/fichas-pantallas/p/253-pc-futbol-7-1.jpg) — descrita en [`13` §4.3](13-version-objetivo-6-0-y-7-0.md) |
| V11 | **Simulador** | [CEZ 253-pc-futbol-7-4.jpg](https://computeremuzone.com/fichas-pantallas/p/253-pc-futbol-7-4.jpg) |

Ficha completa: <https://computeremuzone.com/ficha/253/pc-futbol-7-temporada-98-99>

### 2.3 Variantes: Argentina y selecciones

Contexto, no objetivo. Descritas en [`15`](15-variantes-argentina-y-selecciones.md).

| # | Pantalla | Fuente | Wireframe |
|---|---|---|---|
| V19 | **Menú principal argentino** — destinos numerados con cifra coloreada por familia, rejilla de escudos en el raíl | [CEZ 848-pc-futbol-argentina-40-2.png](https://computeremuzone.com/fichas-pantallas/p/848-pc-futbol-argentina-40-2.png) | [`15` §1.1](15-variantes-argentina-y-selecciones.md) |
| V20 | **Historial de club** — tabla con el hueco de la comparación ya dibujado, cromo gris claro | [CEZ 848-pc-futbol-argentina-40-1.png](https://computeremuzone.com/fichas-pantallas/p/848-pc-futbol-argentina-40-1.png) | [`15` §1.2](15-variantes-argentina-y-selecciones.md) |
| V21 | **Simulador argentino** | [CEZ 848-pc-futbol-argentina-40-3.png](https://computeremuzone.com/fichas-pantallas/p/848-pc-futbol-argentina-40-3.png) | Fuera de alcance |

Fichas: <https://computeremuzone.com/ficha/848/pc-futbol-argentina-40-apertura--96> ·
<https://computeremuzone.com/ficha/627/pc-seleccion-espanola-de-futbol-eurocopa--96>

### 2.4 Pantallas de la línea Liga Manager (6.0/7.0 y contemporáneas)

Referencias aportadas al proyecto y descritas en [`03`](03-mapa-de-pantallas.md).
No tienen URL pública catalogada; su descripción estructural es la fuente.

| # | Pantalla | Descripción |
|---|---|---|
| V12 | **Pretemporada Liga Manager** — rejilla de escudos, España/Países, tabla de amistosos | [`13` §4.2](13-version-objetivo-6-0-y-7-0.md) |
| V13 | **Alineación** — tabla maestra + raíl derecho reactivo | [`13` §4.3](13-version-objetivo-6-0-y-7-0.md) |
| V14 | **Introduce nombre y equipo** — 20 ranuras + rejilla de escudos | [`13` §4.1](13-version-objetivo-6-0-y-7-0.md) |
| V15 | **Ficha de jugador + Hacer oferta** — diálogo de negociación en dos mitades | [`03` §H, §I](03-mapa-de-pantallas.md) |
| V16 | **Opciones de configuración** — seis placas rotuladas en una pantalla | [`03` §Q](03-mapa-de-pantallas.md) |
| V17 | **Campeón / subcampeón** — pantalla ceremonial | [`03` §R](03-mapa-de-pantallas.md) |
| V18 | **Despacho por cuadrantes** (línea posterior: Argentina / 2000 / 2005) | [`03` §D](03-mapa-de-pantallas.md) — **no es la referencia objetivo**, ver §3 |
### 2.5 Capturas leídas componente a componente

Las siguientes se descargaron, se miraron una por una y se describieron en
[`16-lectura-de-capturas.md`](16-lectura-de-capturas.md). Son la base de las
correcciones de §3.

| # | Pantalla | Versión | Fuente | Lectura |
|---|---|---|---|---|
| V22 | **Menú principal** — rejilla 3×3, extensiones en gris, raíl inferior | 6.0 | [devuego 624_1.jpg](https://cdn.devuego.es/capturas/624_1.jpg) | [`16` §1](16-lectura-de-capturas.md) |
| V23 | **Pretemporada Promanager** — mapa, cinco ranuras fijas, comandos por intención | 6.0 | [devuego 624_2.jpg](https://cdn.devuego.es/capturas/624_2.jpg) | [`16` §7](16-lectura-de-capturas.md) |
| V24 | **Ficha de club** — el patrón `FieldPair` en toda su extensión | 6.0 | [devuego 624_3.jpg](https://cdn.devuego.es/capturas/624_3.jpg) | [`16` §6](16-lectura-de-capturas.md) |
| V25 | **Simulador** — marcador permanente arriba, glifo de evento | 6.0 | [devuego 624_4.jpg](https://cdn.devuego.es/capturas/624_4.jpg) | Fuera de alcance |
| V26 | **Despacho por cuadrantes** — cintillo de noticias, columnas de retratos | 6.0 | [devuego 624_5.jpg](https://cdn.devuego.es/capturas/624_5.jpg) | [`16` §3](16-lectura-de-capturas.md) |
| V27 | **Alineación** — bloques, mapa de calor, raíl reactivo completo | 6.0 | [devuego 624_6.jpg](https://cdn.devuego.es/capturas/624_6.jpg) | [`16` §4](16-lectura-de-capturas.md) |
| V28 | **Menú Manager** — la misma agrupación con otra paleta | 5.0 | [myabandonware pc-futbol-5-0_2.png](https://www.myabandonware.com/media/screenshots/p/pc-futbol-5-0-q6n/pc-futbol-5-0_2.png) | [`16` §2](16-lectura-de-capturas.md) |
| V29 | **Ver rival** — conmutador de vista, tarjeta de jugador, placa de asistente | 5.0 | [myabandonware pc-futbol-5-0_8.png](https://www.myabandonware.com/media/screenshots/p/pc-futbol-5-0-q6n/pc-futbol-5-0_8.png) | [`16` §5](16-lectura-de-capturas.md) |

Fichas con la galería completa:
<https://www.devuego.es/presskit/pc-futbol-60> ·
<https://www.myabandonware.com/game/pc-futbol-5-0-gzl> (30 capturas) ·
<https://www.myabandonware.com/game/pc-futbol-6-0-gzu>

---

## 3. Los dos registros conviven en la misma versión

> **Corrección (2026-08-19).** Este apartado decía que el «despacho por
> cuadrantes» pertenecía a una línea posterior (Argentina, 2000, 2005+) y que
> **no** era nuestra referencia. Es falso. Al leer las capturas originales de
> 5.0 y 6.0 ([`16`](16-lectura-de-capturas.md) §2 y §3) se ve que el despacho
> por cuadrantes **es** el eje de la carrera en ambas, con su barra inferior
> `SALIR / GRABAR LIGA / NOTICIAS / SEGUIR`. Se corrige aquí para que nadie
> construya el despacho a partir de la premisa equivocada.

No son dos linajes rivales: son **dos registros de una misma interfaz**, y cada
uno tiene su trabajo.

| Registro | Aspecto | Dónde se usa | Su trabajo |
|---|---|---|---|
| **Despacho por cuadrantes** | Cuatro áreas alrededor del próximo partido, raíles con el nombre del área, barra inferior `SALIR / GRABAR LIGA / NOTICIAS / SEGUIR` | El **eje** de la carrera, 5.0 y 6.0 | Elegir a dónde vas, y recordarte qué viene después |
| **Liga Manager clásico** | Banda cromada con título centrado, tabla maestra densa, raíl derecho reactivo, placa de calendario | Cada **pantalla de trabajo** que el despacho abre | Enseñar y dejar decidir |

La consecuencia práctica: PC Dynasty necesita **los dos**. Hoy sólo tiene el
segundo — un raíl lateral que abre pantallas de trabajo — y le falta el primero,
que es la pantalla a la que el jugador vuelve entre decisión y decisión.

Lo que sí varía entre versiones es la **paleta**: los colores de las cuatro áreas
cambiaron de 5.0 a 6.0 mientras la agrupación se mantuvo intacta
([`16`](16-lectura-de-capturas.md) §3). Por eso el invariante que defendemos es
«cuatro áreas separadas por color», no una paleta concreta.

## 4. Cómo añadir una referencia nueva

1. Localiza la captura y **anota su URL exacta**.
2. Descárgala fuera del repositorio y míralas.
3. Escribe en el capítulo que corresponda: zonas, jerarquía, qué color significa
   qué, qué controles hay y en qué estados.
4. Dibuja el wireframe en ASCII, con la misma notación que el resto de la wiki.
5. Añade la fila a las tablas de §2 con el enlace y el destino del wireframe.
6. Si la fuente es nueva, regístrala también en [`99-fuentes.md`](99-fuentes.md).
7. **No commitees la imagen.**
