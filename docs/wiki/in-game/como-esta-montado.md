# Cómo está montada la exploración in-game

Esta carpeta documenta títulos reales, arrancados de verdad, no leídos de una
captura de archivo (ver [`README.md`](README.md) para la política de
capturas). Este fichero es la otra mitad de esa promesa: cómo se levanta el
entorno para que, en el futuro, se pueda repetir sin redescubrir nada de lo
que sigue.

## Por qué VirtualBox, no QEMU ni Wine

Las dos rutas iniciales (documentadas en el preámbulo de [`README.md`](README.md))
llegaron a un límite real, no una limitación de herramienta:

- **QEMU** (absoluto y relativo por QMP) y un **VNC real** (`vncdo`) mueven el
  cursor y aceptan teclado, pero ningún clic produce cambio alguno dentro del
  menú animado ni del despacho.
- **Wine**, en modo `wine explorer /desktop=`, sí acepta clics reales vía
  `python-xlib`+XTest — pero solo en pantallas de utilidad (`DBASEWIN.EXE`).
  El menú animado (`PCF6.EXE`) y el despacho (`MANAGER.EXE`) pintan sobre una
  superficie DirectDraw a pantalla completa que **no compone bajo WSLg**
  (pantalla negra), igual que en QEMU.
- **El chequeo de CD-ROM de `MANAGER.EXE`** no es un chequeo de fichero: es
  una enumeración real de hardware SetupAPI (`SetupDiCreateDeviceInfoList`,
  confirmado con `WINEDEBUG=+all`). Solo una unidad ATAPI genuina — la que
  VirtualBox emula — la satisface de forma consistente; un ISO montado a
  mano en Wine/QEMU no siempre cuela.

VirtualBox resuelve ambos problemas a la vez: emula un CD-ROM real (el
chequeo pasa) y, vía su **API COM**, ofrece una vía de entrada que no depende
de que DirectDraw componga en un servidor X — se inyecta directo en el
controlador virtual PS/2, por debajo de cualquier ventana.

## Dónde vive

Todo el disco de la VM está en `V:\pcd-reference-games\`, no en `C:` ni bajo
WSL — deliberado: son VDIs de varios GB y el `C:` del host de referencia
tenía un solo dígito de GB libres. La VM registrada se llama `PCF6Test`
(constante `VBOX_VM_NAMES.pcf6` en
[`tools/contracts/constants/reference/vbox.constants.ts`](../../../tools/contracts/constants/reference/vbox.constants.ts)).

Los discos se crearon convirtiendo las imágenes RAW del archivo de
preservación con `VBoxManage convertfromraw ... --format VDI`, y se
adjuntaron a un controlador IDE PIIX4 vía `VBoxManage storagectl`/`storageattach`.

## Arrancar y controlar la VM (`bun run vm:*`)

Todo pasa por [`tools/scripts/reference/vm.script.ts`](../../../tools/scripts/reference/vm.script.ts),
nunca por `sh`/`bash` sueltos:

```
bun run vm:list                                    # VMs registradas + ids conocidos
bun run vm:status --game=pcf6
bun run vm:start  --game=pcf6                       # headless — sin ventana
bun run vm:start  --game=pcf6 --visible             # ventana normal de VirtualBox
bun run vm:stop   --game=pcf6                       # savestate; seguro aunque ya esté parada
bun run vm:screenshot --game=pcf6 --out shots/01.png
bun run vm:move   --game=pcf6 --reset --dx 150 --dy 300   # mover sin pulsar
bun run vm:click  --game=pcf6 --reset --dx 150 --dy 300   # mover y pulsar
bun run vm:key    --game=pcf6 38 0f 8f b8                  # scancodes crudos (Alt+Tab)
```

`--visible` abre la ventana normal de VirtualBox en el host Windows — útil
para que una persona mire en directo mientras el agente pilota por API. Si
la VM ya está corriendo headless, **no** se le puede añadir una ventana con
`startvm --type gui` (VirtualBox lo rechaza: "already locked by a session");
para verla hay que abrir el gestor (`VirtualBox.exe`, sin argumentos) y usar
"Mostrar" sobre la VM en marcha, que sí sabe adjuntar pantalla a una sesión
headless existente.

## Inyección de ratón y teclado: el mecanismo, y el bug que costó una sesión entera

### El mecanismo

`VBoxManage` no tiene un comando de ratón (sólo `keyboardputscancode` para
teclado). El ratón se inyecta vía la **API COM/scripting** de VirtualBox,
accesible desde PowerShell:

```powershell
$vbox = New-Object -ComObject VirtualBox.VirtualBox
$machine = $vbox.FindMachine($VmName)
$session = New-Object -ComObject VirtualBox.Session
$machine.LockMachine($session, 1)   # 1 = LockType_Shared — debe ser entero, no "Shared"
$mouse = $session.Console.Mouse
$mouse.PutMouseEvent($dx, $dy, 0, 0, $buttonState)   # bit0=izq, bit1=der, bit2=medio
$session.UnlockMachine()
```

Esto escribe directamente en el controlador PS/2 virtual — **nunca toca el
cursor real del host ni le roba el foco de ventana**, el requisito que hizo
descartar cualquier ruta con SendInput/xdotool/mstsc contra la sesión real
de Windows del usuario. El script parametrizado vive en
[`tools/scripts/reference/vbox-mouse.ps1`](../../../tools/scripts/reference/vbox-mouse.ps1),
invocado desde `moveMouse()` en
[`tools/helpers/reference/vbox.helper.ts`](../../../tools/helpers/reference/vbox.helper.ts) —
la decisión (a qué delta moverse, cuándo pulsar) vive en TS; el `.ps1` sólo
ejecuta, igual que `VBoxManage.exe` es sólo ejecutado por el resto de la capa.

`AbsoluteSupported` de `IMouse` da `False` con este invitado (Windows 98
nunca negocia HID USB de tableta, ni con `--mouse usbtablet`), así que
`PutMouseEventAbsolute` no sirve aquí — todo movimiento es **relativo**, en
pasos pequeños (3 unidades, ~8ms entre pasos) para no disparar la
aceleración de puntero de Windows con un salto grande.

RDP (VRDE) **no sirve para este invitado**: renderiza vídeo pero no entrega
ningún input a Windows 98 (confirmado con `xfreerdp`+XTest y con `mstsc.exe`,
que siempre da error 0x708/disconnect reason 1800 — Terminal Services de
Windows 98 no es compatible con lo que VRDE espera). No hace falta: la API
COM ya resuelve la inyección sin RDP de por medio.

### El bug: un "reset" de una sola llamada no aterriza donde crees

Para tener un punto de partida reproducible antes de cada movimiento, la
técnica es resetear el cursor a la esquina superior-izquierda con un delta
negativo enorme en una sola llamada — `PutMouseEvent(-5000, -5000, 0, 0, 0)`.
**Esto es la causa de horas de deriva de coordenadas irreproducible.** Un
paquete PS/2 real codifica `dx`/`dy` en un campo con signo pequeño por
paquete; un valor tan grande en una sola llamada no se satura limpiamente en
(0,0) — se trunca/envuelve de forma no determinista, y el cursor aterriza en
un punto casi arbitrario que **parece** funcionar unas veces (por eso las
primeras calibraciones de la sesión sí funcionaron) y falla otras sin
ningún cambio aparente en el procedimiento.

**Cómo se diagnosticó**: arrastrando con el botón pulsado desde el
"reset" (`PutMouseEvent` con `buttonState=1` en cada paso del arrastre) se
dibuja un rectángulo de selección real en el escritorio — visible en la
captura de pantalla **incluso cuando el propio cursor no se compone en
ella** (la flecha del cursor resultó ser una señal nada fiable: a veces no
aparece en el PNG aunque la posición sea correcta). El rectángulo probó que
el "reset" de una sola llamada, algunas veces, ni siquiera llegaba cerca de
la esquina (0,0) real.

**La corrección**: caminar hasta la esquina en los mismos pasos pequeños
y seguros por paquete que cualquier otro movimiento (`Move-Walked` en
`vbox-mouse.ps1`, deltas de 20 unidades hasta cubrir -3000,-3000 — muy por
encima de cualquier resolución real, pero cada paso individual dentro de
cualquier límite de paquete plausible), en vez de una única llamada con un
valor gigante. Desde entonces el reset aterriza en (0,0) todas las veces.

### Calibración: dispositivo → píxel de pantalla

Tras un reset correcto, un delta de dispositivo conocido no corresponde 1:1
con píxeles de pantalla — depende de la velocidad de puntero configurada en
el invitado. Se midió empíricamente, con la misma técnica del rectángulo de
arrastre (visible independientemente de si el cursor se compone), una escala
consistente de **≈2.0 píxeles de pantalla por unidad de delta de
dispositivo**, en ambos ejes, tanto en el escritorio de Windows (800×600)
como en el menú del juego (640×480) — la escala parece depender de la
velocidad de puntero de Windows, no de la resolución activa.

**Receta para clicar una posición conocida en píxeles de pantalla `(px, py)`
tras un reset**:

```
dx_dispositivo = round(px / 2.0)
dy_dispositivo = round(py / 2.0)
bun run vm:click --game=pcf6 --reset --dx <dx_dispositivo> --dy <dy_dispositivo>
```

Verificar siempre con una captura tras el clic — cambios de pantalla (un
diálogo modal, volver al menú) pueden usar un área de contenido desplazada
dentro de la misma resolución, así que conviene una foto de comprobación
más que confiar ciegamente en la fórmula cuando el objetivo es pequeño.

### El foco de ventana: rara vez el problema real

Durante la sesión se sospechó (y parcialmente confirmó) que los clics
dejaban de registrarse por pérdida de foco real de Windows tras un Alt+Tab
involuntario, y que `bun run vm:key --game=pcf6 38 0f 8f b8` (Alt+Tab) lo
"reenganchaba". Esto **puede pasar** y merece probarse si un clic no hace
nada pese a coordenadas correctas — pero la causa dominante de fallos en
esta sesión fue el bug de calibración de arriba, no el foco. No asumir foco
perdido sin antes descartar una coordenada mal calculada con el método del
rectángulo de arrastre.

## Qué queda abierto

- El botón "SEGUIR" de la pantalla de personalización de Liga Manager
  (`pcf6/06-liga-manager-personalizacion.md`) no respondió al primer
  cálculo de coordenadas — probablemente un desajuste fino, no el mecanismo
  en sí (que ya navegó con éxito: submenú Seguimiento, Volver, menú Start de
  Windows, la propia loseta de Liga Manager, un diálogo de aviso). Pendiente
  de recalibrar con el método del rectángulo antes de seguir hacia el
  despacho real.
- No se ha repetido esta misma exploración en Liga Promanager todavía.
