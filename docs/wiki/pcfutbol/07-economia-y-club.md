# 07 · Economía y club

## 1. Caja: el modelo completo `[manual 4.0]`

Caja inicial **proporcional al presupuesto real del club**.

### Ingresos (columna izquierda, orden descendente)

| Concepto | Detalle |
|---|---|
| Taquilla | Recaudación del partido en casa |
| Publicidad estática | Vallas del estadio |
| Retransmisión por TV | **Por competición**; en Copa de Europa se sustituye por importe **por punto conseguido** |
| Traspasos | Cualquier venta de la plantilla |

### Gastos (columna derecha)

| Concepto | Detalle |
|---|---|
| Generales (fijos, semanales) | Empleados + fichas de jugadores + desplazamientos |
| Créditos | Devolución semanal + **12 % de intereses** |
| Primas | Lo pactado por ganar o empatar |
| Compras | Fichajes del periodo |

**Balance** = caja + crédito solicitado.

En Pro-Manager la pantalla añade un **selector de semana** (revisar cualquier jornada
pasada) y las **reformas del estadio** entre los pagos.

## 2. Quiebra

Se permiten **tres semanas en números rojos**. Pasado el plazo sin arreglarlo:

- con otros managers en la partida, el ordenador se hace cargo del club;
- en solitario, **no se puede continuar la Liga**: hay que empezar de nuevo.

`[manual 4.0]` En 7.0 el despido llega por 3+ semanas consecutivas en déficit **o**
por bajar de 16 jugadores en plantilla. `[wiki]`

> Diseño: la ruina no es una pantalla de "Game Over" abstracta, es una **cuenta atrás
> visible de tres semanas**. El jugador ve venir el desastre y puede pelearlo.

## 3. Decisiones económicas `[manual 4.0]`

### Precios

Entradas y vallas publicitarias. El interés del partido depende del **rival, su
posición en la tabla y la tuya**. Y la advertencia explícita del manual: «a veces se
recauda más con una mayor entrada y un menor precio que viceversa».

Es un problema de optimización real con información parcial: exactamente el tipo de
decisión que hace que una pantalla de finanzas sea interesante.

### Crédito

- Importe máximo dependiente del **presupuesto real del club**.
- **Semanas de devolución** acotadas por las jornadas restantes: 42 al empezar en
  Primera, 38 en Segunda, 1 en la última jornada.
- Amortización anticipada para reducir intereses.

### Primas

Por ganar o por empatar, con techo por partido según el presupuesto real. **A mayor
prima, mayor motivación**. Único contrapeso a un bajón colectivo de moral.

## 4. Estadio `[manual 4.0]`

Exclusivo de Pro-Manager en 4.0; general después.

Al pedir un número concreto de asientos el programa devuelve **de inmediato** el
presupuesto completo:

- precio total de la obra
- semanas de duración
- desembolso semanal
- semanas restantes
- dinero pendiente de pago
- aforo resultante

Y la panorámica del estadio a la izquierda **cambia** al ganar aforo.

> Norma que extraemos: **simulación de la decisión antes de tomarla**, con todos los
> números y una representación visual del resultado. Es aplicable a cualquier
> inversión de PC Dynasty.

## 5. Ingresos por competición `[manual 4.0]`

| Competición | Ingreso |
|---|---|
| Copa del Rey | Recaudación íntegra en casa; **mitad de la taquilla** en la final |
| Copa de Europa | **200 M fijos** por participar (antes del primer partido de liguilla) + **45 M por empate** + **90 M por victoria** + taquilla |
| Recopa | Taquilla en casa; mitad en la final; TV empieza a comprar derechos al superar rondas |
| UEFA | Igual que Recopa |

Cifras en pesetas de 1995; el valor está en la **forma**: un fijo por participar, un
variable por rendimiento y un ingreso propio por afluencia. Los tres ejes deben
existir en nuestro modelo.

## 6. Empleados y cuerpo técnico `[prensa]`

En PC Fútbol 2001 el club tenía personal con función mecánica distinta:

| Puesto | Función |
|---|---|
| **Secretario técnico** | Informa sobre fichajes y finanzas |
| **Encargado del campo** | Mantiene el estadio en condiciones |
| **Segundo entrenador** | Entrenamiento y preparación física |
| **Entrenador de juveniles** | Cantera |
| **Ojeador** | Busca jugadores juveniles |
| *(2005 añadió)* psicólogo, y quitó varios de los anteriores | |

En 2001 el modo **manager** incluía además merchandising y **servicio de cafetería**;
2005 los eliminó y fue una de las quejas más repetidas. `[prensa]` `[wiki]`

> Lección: cada empleado debe tener **una** mecánica que sólo él afecta. Si contratar
> a alguien no cambia un número visible, es decoración y el jugador lo nota.

## 7. Cantera `[wiki]`

Desde 6.0: contratar juveniles para crear cantera. En 2005 se pueden mover atletas
entre primer equipo y filial. PC Fútbol 8 declara segundo equipo y **hasta dos
equipos juveniles**.

## 8. Objetivos de la directiva `[manual 4.0]`

En Pro-Manager la Directiva fija objetivos a principio de temporada, y de cumplirlos
depende conservar el puesto y recibir mejores ofertas. Es lo que convierte una liga
en una **carrera**.
