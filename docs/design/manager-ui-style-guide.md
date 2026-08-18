# Sistema visual de PC Dynasty

Este sistema busca la rapidez, densidad informativa y navegación directa de un
manager de escritorio clásico, con una identidad propia. No replica pantallas,
recursos, tipografías, proporciones ni composición comercial de otros juegos.

## Principios

1. **La decisión primero.** Cada vista comienza con el dato y la acción que el
   jugador necesita; el contexto secundario se abre sin abandonar la pantalla.
2. **Densidad legible.** Tablas compactas, alineación estable y jerarquías claras,
   sin reducir objetivos interactivos por debajo de 40 px.
3. **Respuesta inmediata.** Navegar y desplegar detalle no inicia trabajo pesado.
   Los modelos de lectura se preparan antes de mostrarse y el último estado válido
   permanece visible durante una actualización.
4. **Una misma gramática.** Navegación, tarjetas, tablas y paneles de detalle usan
   los tokens compartidos; no se introducen colores ni espaciados locales.

## Paleta y semántica

| Token | Valor | Uso |
|---|---:|---|
| Canvas | `#0B1724` | Fondo general |
| Navigation | `#102C40` | Navegación persistente |
| Surface | `#163348` | Paneles y filas |
| Surface raised | `#1C4057` | Selección y detalle elevado |
| Border | `#31576B` | Separadores y contornos |
| Accent | `#E3B341` | Acción principal y estado activo |
| Positive | `#58C69A` | Resultado favorable confirmado |
| Warning | `#F09558` | Riesgo o dato incompleto |
| Negative | `#EB6B73` | Error o tendencia desfavorable |
| Text primary | `#F1F6F9` | Texto principal |
| Text secondary | `#A8C0CD` | Metadatos y ayuda |
| Focus | `#70C7E8` | Foco de teclado inequívoco |

El color nunca es el único portador de significado: se acompaña de texto, icono
o forma. Texto normal mantiene un contraste mínimo de 4.5:1.

## Tipografía y espaciado

Se usa la fuente de interfaz del sistema hasta disponer de una familia propia con
licencia redistribuible. La escala recomendada es 13/15/18/24/32 px para metadato,
cuerpo, subtítulo, título de panel y título de vista. Números tabulares se alinean
a la derecha. La rejilla de espaciado es 2, 4, 8, 12, 16, 24, 32 y 48 px; el radio
común es 6 px.

## Navegación

- La barra lateral persistente contiene un máximo de siete áreas principales.
- La zona activa combina fondo, acento lateral y texto; conserva foco de teclado.
- Una cabecera contextual ofrece fecha, entidad activa y acciones globales.
- Pestañas o filtros refinan la vista actual; nunca duplican la navegación global.
- `Esc` cierra el nivel de detalle superior y devuelve el foco a su origen.

## Revelado progresivo

La primera capa presenta indicadores, excepciones y siguientes acciones. Una
tarjeta abre un panel lateral con explicación y desglose; desde allí puede abrirse
una vista completa. El panel conserva el contexto de origen, tiene título accesible
y una única acción clara de cierre. No se encadenan más de dos capas superpuestas.

## Estados y rendimiento percibido

Cada componente define estado vacío, cargando, disponible, obsoleto y error. Las
actualizaciones breves usan una señal no bloqueante y conservan datos anteriores;
solo el primer arranque muestra un esqueleto. Búsqueda y filtros responden en el
mismo frame sobre el modelo ya cargado. Simulación, persistencia e importación se
ejecutan fuera de los nodos visuales y publican modelos de lectura inmutables.

## Componentes

- **SidebarNavigator:** navegación por destinos suministrados, con selección y foco.
- **MetricCard:** etiqueta, valor, contexto y tendencia opcional; no calcula reglas.
- **DetailDrilldown:** panel de detalle con cierre, título y contenido reemplazable.

Los componentes solo representan datos recibidos y emiten intención del usuario.
La aplicación decide qué consultar o ejecutar.
