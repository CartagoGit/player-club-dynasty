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

### 2.3 Pantallas de la línea Liga Manager (6.0/7.0 y contemporáneas)

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

---

## 3. Cuidado: dos linajes de interfaz distintos

Circulan capturas de dos familias que se confunden con facilidad:

| Linaje | Aspecto | Versiones | ¿Es nuestro objetivo? |
|---|---|---|---|
| **Liga Manager clásico** | Banda cromada con título centrado, tabla maestra con raíl derecho, placa de calendario | 4.0 → 7.0 | **Sí** |
| **Despacho por cuadrantes** | Fondo negro, cuatro grupos de tres botones, raíles verticales con el nombre del área, barra inferior `SALIR / GRABAR / NOTICIAS / SEGUIR` | PC Fútbol Argentina, 2000, 2005+ | **No**, pero su idea de agrupar destinos por área con color propio sí se adopta |

Lo tomamos así: **la piel es la del Liga Manager de 6.0**; de los cuadrantes
tomamos únicamente el principio de **agrupar los destinos por área y darle a cada
área su color**, que es lo que ya aplicamos al raíl del despacho.

---

## 4. Cómo añadir una referencia nueva

1. Localiza la captura y **anota su URL exacta**.
2. Descárgala fuera del repositorio y míralas.
3. Escribe en el capítulo que corresponda: zonas, jerarquía, qué color significa
   qué, qué controles hay y en qué estados.
4. Dibuja el wireframe en ASCII, con la misma notación que el resto de la wiki.
5. Añade la fila a las tablas de §2 con el enlace y el destino del wireframe.
6. Si la fuente es nueva, regístrala también en [`99-fuentes.md`](99-fuentes.md).
7. **No commitees la imagen.**
