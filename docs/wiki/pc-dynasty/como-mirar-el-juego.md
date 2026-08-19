# Cómo mirar el juego (y no describirlo)

Esta wiki es normativa sobre **cómo debe verse** PC Dynasty. Este documento es
lo contrario: cómo **comprobar cómo se ve de verdad**, ahora mismo, sin fiarse
de la memoria de nadie.

La regla que lo justifica: un cambio de aspecto se revisa **mirándolo**. Todos
los defectos de interfaz que se han arreglado en este proyecto se encontraron
renderizando una ruta a PNG, no leyendo el código.

---

## 1. Levantar el juego de verdad

En WSL con WSLg no hace falta habilitar nada: `DISPLAY=:0` ya está, y el binario
vive en `.tools/godot/current`.

```bash
bun run game:play                 # ventana normal; bloquea hasta que la cierras
bun run game:play --quit-after 8  # arranca, se ve 8 segundos y sale solo
bun run game:editor               # abre el editor de Godot sobre el proyecto
bun run game:doctor               # qué .NET y qué Godot está usando
```

> **`--quit-after` importa.** Un agente que lanza el juego sin él se queda
> bloqueado esperando a una ventana que nadie va a cerrar. Con él, la ejecución
> tiene principio y fin.

Si el binario no aparece: `bun run game:resolve` lo descarga y refresca el
symlink `current`.

---

## 2. Recorrer rutas y fotografiarlas

`bun run game:explore` camina el juego por un plan de pasos y guarda un PNG en
cada parada. El plan es **dato**, no código, así que explorar una pantalla nueva
cuesta una línea de comando y no una edición de GDScript.

```bash
# El recorrido por defecto de las rutas que existen hoy
bun run game:explore --tour

# Un recorrido propio
bun run game:explore '[{"shot":"menu"},{"press":"NewCareer"},{"shot":"setup"}]'

# Desde un fichero, y sin reconstruir (más rápido si ya compilaste)
bun run game:explore --plan docs/routes/mercado.json --no-build
```

Los PNG van a `.generate/shots/` salvo que pases `--out <dir>`.

### Pasos disponibles

| Paso | Qué hace |
|---|---|
| `{"shot": "nombre"}` | guarda `nombre.png` |
| `{"press": "NodeName"}` | activa un `Button`, un tile, o cualquier cosa con `Press()` |
| `{"wait": 30}` | deja pasar N fotogramas |
| `{"dump": "NodeName"}` | imprime el subárbol con tipos (`"*"` = todo) |
| `{"find": "NodeName"}` | dice si el nodo existe y dónde |
| `{"text": "NodeName"}` | imprime su texto |
| `{"resize": [1280, 720]}` | cambia el tamaño de ventana |

Los dos primeros que conviene usar cuando **no sabes cómo se llama nada**:

```bash
bun run game:explore '[{"dump":"*"}]'          # el árbol entero
bun run game:explore '[{"find":"Board"}]'      # ¿existe? ¿dónde?
```

### Por qué `press` acepta tres cosas distintas

Un `Button` tiene la señal `pressed`. Nuestras placas (`DestinationTile`,
`SelectableTile`) **no son botones** — un `Button` no es un contenedor, y darle
hijos hace que se coman el clic y se dibujen fuera del marco — así que exponen
un método `Press()`. El explorador prueba `Press()` primero y la señal después,
para que el plan no tenga que saber de qué clase es cada cosa.

---

## 3. Lo que esto sí y no permite

**Sí**: ver cualquier pantalla tal como se dibuja, a cualquier resolución,
comprobar que un panel no se sale, leer el árbol de nodos, y comparar un antes y
un después mirando dos PNG.

**No**: mover el ratón por una ventana viva y reaccionar a lo que pasa. El hover,
el foco al pasar y los tiempos de transición hay que provocarlos desde el plan o
mirarlos en persona.

Para lo que un plan no alcanza está `bun run game:play`: lo abres tú y lo miras.

---

## 4. Las otras dos formas de mirar

| Herramienta | Para qué |
|---|---|
| `bun run game:smoke` | recorre el juego entero y **falla** si algo se rompió; es la red de seguridad, no la lupa |
| `game/godot/Tests/CaptureShots.gd` | el recorrido fijo que acompaña a cada cambio de chrome; `game:explore` es su versión libre |

---

## 5. Mirar el original

Las capturas de PC Fútbol que sostienen las decisiones de esta wiki están
catalogadas en [`../pcfutbol/14-catalogo-visual.md`](../pcfutbol/14-catalogo-visual.md)
y leídas una por una en [`../pcfutbol/16-lectura-de-capturas.md`](../pcfutbol/16-lectura-de-capturas.md).

**No se descargan al repositorio.** Para trabajar con una:

```bash
mkdir -p /tmp/pcd-refs && cd /tmp/pcd-refs
curl -sO https://cdn.devuego.es/capturas/624_5.jpg
```

Míralas, escribe lo que veas en el capítulo que toque, y no las commitees.
