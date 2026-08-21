# In-game — observado ejecutando el original, no leído de una captura

Esta carpeta es distinta de [`../pcfutbol/14-catalogo-visual.md`](../pcfutbol/14-catalogo-visual.md)
(capturas de archivo, enlazadas) y de [`../pcfutbol/16-lectura-de-capturas.md`](../pcfutbol/16-lectura-de-capturas.md)
(esas mismas capturas, leídas). Aquí no hay una imagen fija de otro sitio: el
original se **arrancó de verdad**, bajo QEMU, a partir de las imágenes de disco
del archivo de preservación de `dinamicmultimedia.es`, y lo que sigue es lo que
esa sesión mostró. Etiqueta `[jugado]` según [`../pcfutbol/99-fuentes.md`](../pcfutbol/99-fuentes.md) §2 —
la de mayor confianza, porque cubre lo que ninguna captura estática puede: qué
aparece al pulsar, y en qué orden.

**Excepción deliberada a la política de [`14`](../pcfutbol/14-catalogo-visual.md) §1.**
Esa política prohíbe commitear capturas porque son de un archivo ajeno (prensa,
sitios de fans) cuyo derecho de reproducción no es nuestro. Aquí es distinto:
son capturas **propias**, generadas arrancando nosotros mismos el disco
preservado — el mismo tipo de evidencia que un pantallazo de nuestro propio
Godot. Por indicación explícita (2026-08-20), estas sí se guardan en
`{juego}/screenshots/**`, junto a la descripción y el wireframe de siempre. Se
mantiene igualmente la regla de fondo: nunca activos de otro (fuentes, sonidos,
ilustraciones) fuera de la captura de pantalla en sí.

> **Cómo se consiguió.** `bun run download` trae las imágenes de disco a
> `.generate/games/` (ver [`../pc-dynasty/como-mirar-el-juego.md`](../pc-dynasty/como-mirar-el-juego.md) §5.2);
> se arrancan con `qemu-system-i386 -display none -qmp unix:...` y se
> fotografían por QMP `screendump` (§5.3).
>
> **El teclado funciona; el ratón, no — probado a fondo, no por falta de
> intento.** Ctrl+Esc, flechas e Intro navegaron el menú Inicio de Windows con
> total fiabilidad (así se llegó al PC Fútbol 7, que no arranca solo). Pero
> dentro de las pantallas propias del juego se probaron **tres** mecanismos de
> ratón distintos y ninguno dio control fiable: puntero absoluto por QMP
> (`input-send-event type:abs`, con y sin `-usb -device usb-tablet`), puntero
> relativo por QMP (`type:rel`), y un cliente VNC real (`vncdo` contra
> `-vnc :N`, que sí muestra un cursor de verdad y sí acepta teclado — pero los
> clics no producen ningún cambio visible en las pantallas del juego). La
> hipótesis más plausible: estos títulos leen el ratón en modo exclusivo de
> DirectInput, sondeando el dispositivo directamente en vez de escuchar
> mensajes de ventana — un camino que ninguno de los tres mecanismos anteriores
> reproduce.
>
> **La solución no era mejorar el ratón dentro de la VM: era salir de la VM.**
> Los `.exe` del juego están en el disco de datos (`win98_pcf6_data.bin →
> FUTBOL6\`), extraíbles sin montar nada como root (`pyfatfs`/`pycdlib` en
> Python leen FAT/ISO9660 directamente desde el fichero — ver
> `pcf6/02-liga-manager-ejecutable.md`). Corriendo esos `.exe` con **Wine**
> en este mismo host, en modo `wine explorer /desktop=name,WxH` (un
> "escritorio virtual" de Wine, no pantalla completa), la ventana **sí** es
> una ventana X11 normal, capturable con `import` y — esto es lo que
> cambia todo — **sí acepta clics reales** vía `python-xlib` +
> `Xlib.ext.xtest` (`XTestFakeMotionEvent`/`XTestFakeButtonEvent`), sin
> xdotool ni Xvfb instalados (no hay acceso root; ambos se evitaron:
> WSLg ya pone un `DISPLAY=:0` real, y python-xlib se instala por `pip
> --user`). El modo `/desktop=` importa: DirectDraw a pantalla completa
> (sin ese flag) no compone en WSLg — se probó y da negro, igual que en
> QEMU — pero el modo de escritorio virtual de Wine sí. Con esto se navegó
> de verdad `DBASEWIN.EXE` (Base de Datos): selección → ficha de club →
> plantilla → un diálogo de error real, todo con clics.
>
> **El chequeo de CD sí se resuelve — con el ISO completo, no con un archivo
> suelto.** Un `DISK.ID` copiado a mano y una etiqueta de volumen falsa no
> bastaron (`MANAGER.EXE` seguía pidiendo el disco). Extrayendo el CD-ROM
> **entero** con `pycdlib` (lee ISO9660/Joliet directamente del `.bin`, sin
> montar nada) a la carpeta mapeada como `D:`, y relanzando `PCF6.EXE` (el
> lanzador real — como bien apuntó el usuario, no `MANAGER.EXE` suelto), el
> diálogo "CD-ROM no encontrado" **dejó de aparecer**. El chequeo debía
> comparar tamaños/contenido de varios ficheros reales del disco (candidato:
> `LONGITUD.DAT`), no solo un marcador — nada de sectores crudos.
>
> **Actualización 2026-08-21 — el menú animado sí se pulsa, cambiando de
> entorno otra vez.** Lo de arriba (QEMU, VNC, Wine+XTest) era real, pero
> era un límite de **esos tres entornos**, no del juego: DirectDraw a
> pantalla completa no compone bajo WSLg, así que ninguno podía ni ver el
> menú animado, y por tanto tampoco confirmar si un clic llegaba. La
> solución fue salir también de Wine/QEMU: una VM real de **VirtualBox**
> (necesaria además por el chequeo de CD-ROM, que enumera hardware ATAPI de
> verdad — ver `pcf6/02-liga-manager-ejecutable.md`), pilotada por su **API
> COM** (`IMouse.PutMouseEvent`) en vez de por ventana X11. Esto sí compone
> el menú, y sí acepta clics — se navegó con éxito el submenú "Seguimiento",
> se volvió al menú principal, y se entró en Liga Manager hasta la pantalla
> de personalización (`pcf6/06-liga-manager-personalizacion.md`). El
> mecanismo completo, y un bug de calibración de coordenadas que costó horas
> antes de encontrarlo, están en
> [`como-esta-montado.md`](como-esta-montado.md). La hipótesis de "modo
> exclusivo de DirectInput" queda descartada como explicación del bloqueo:
> el problema real era que ningún entorno anterior componía la superficie en
> absoluto, no que el juego ignorase la entrada sintética.

Ver [`como-esta-montado.md`](como-esta-montado.md) para cómo se levanta y se
pilota el entorno hoy (VirtualBox + `bun run vm:*`), reproducible sin
redescubrir nada de lo de abajo.

## Un folder por título

| Carpeta | Título | Qué se llegó a ver |
|---|---|---|
| [`pcf6/`](pcf6/00-menu-principal.md) | PC Fútbol 6.0 (temporada 97-98) | Menú principal, submenú modal (Seguimiento), Liga Manager completo hasta el despacho (personalización, nombre/equipo, pretemporada, equipos en competiciones, despacho, personal del club, clasificación con Primera/Segunda/2ªB I-IV), Liga Promanager hasta selección de ofertas (personalización propia, clave de acceso, ofertas de trabajo reales), Desafío América-Europa hasta el draft de jugadores, Historia (palmarés desde 1928-29), Base de Datos completa (selección, ficha de club, plantilla), un diálogo de error real |
| [`pcf7/`](pcf7/00-menu-principal.md) | PC Fútbol 7 (temporada 98-99) | Menú principal |
| [`pcbasket65/`](pcbasket65/00-menu-principal.md) | PC Basket 6.5 | Menú principal |

Ver también [`comparacion-shells.md`](comparacion-shells.md) — qué gramática se
repite entre los tres y qué no.
