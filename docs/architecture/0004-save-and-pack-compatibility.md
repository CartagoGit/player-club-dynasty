# Persistencia, packs y carga externa de datos

## Decisión

El juego separa estrictamente tres cosas:

1. **contenido importable**: países, clubes, personas, competiciones, assets y
   definiciones de reglas;
2. **partida persistida**: hechos de una carrera, snapshots y referencias
   exactas al contenido que la creó;
3. **proyecciones derivadas**: índices, medias, rankings y read models.

El Core consume contratos compilados; no conoce CSV, JSON, SQL ni rutas de
archivo. Así una base de datos externa puede alimentar al juego sin convertirse
en una dependencia del motor ni contaminar las reglas de negocio.

## Contrato de contenido externo

Todos los proveedores implementarán el mismo puerto de importación:

```text
Content source → Reader → Normalized import model → Validator → Compiler → Pack
```

- **Reader:** adaptador de solo lectura para SQLite, CSV, JSON, una API o una
  fuente futura. No transmite sus tipos al Core.
- **Normalized import model:** DTO versionado con IDs externos, procedencia y
  campos explícitos; no es el modelo de dominio.
- **Validator:** detecta identidad duplicada, referencias ausentes, rangos
  inválidos, incompatibilidades de licencia y reglas incompletas.
- **Compiler:** resuelve IDs estables, strings, relaciones y nombres de
  atributos a estructuras internas compactas.
- **Pack:** artefacto inmutable, versionado y con hash, apto para iniciar una
  partida o distribuirse sin ejecutar código arbitrario.

El importador genera un informe reproducible con versión de esquema, origen,
conteos, advertencias, errores y hash del resultado. Un error bloqueante no
produce un pack parcial.

## Formatos y extensibilidad

La primera implementación debe priorizar archivos locales reproducibles:

- manifiesto de pack en JSON o YAML;
- tablas de contenido en CSV/JSON y assets referenciados;
- adaptador SQLite para bases de datos comunitarias grandes.

Una fuente remota es opcional y se descarga a una caché verificable antes de
importarse; no se consulta desde un hot loop ni durante una simulación. La
interfaz del Reader permite añadir PostgreSQL u otro proveedor sin que
FootballPack, Godot o Core cambien.

Los datos reales o comunitarios deben llevar origen y licencia. PC Dynasty no
incluye material protegido por el mero hecho de soportar su importación.

## Identidad y compatibilidad

Cada entidad importada conserva un `ExternalId` namespaced por proveedor y un
`StableId` interno. El primero permite reimportar y diagnosticar; el segundo
no cambia durante la vida de una partida. Nunca se usa el nombre visible como
clave.

Un pack declara:

- `packId`, versión semántica y hash de contenido;
- dependencias de otros packs con rangos compatibles;
- versión del esquema de importación;
- versión del compilador y de los contratos Core que requiere;
- capacidades declarativas que aporta (deporte, competición, base, assets o
  presentación).

La carga resuelve el grafo completo de dependencias antes de abrir una partida.
Los conflictos de versión se explican al usuario; no se eligen de forma
silenciosa.

## Formato de save

Una partida contiene un manifiesto, snapshots periódicos y un diario de
hechos. El manifiesto fija la versión de save, la semilla raíz y el conjunto
exacto de packs/hashes. Los snapshots aceleran carga; los hechos preservan
historia, auditoría y migraciones.

Se persisten hechos con consecuencias: contratos, resultados, lesiones,
fichajes, cambios de regla, títulos, instalaciones y decisiones del usuario.
Se recalculan proyecciones de UI y valores derivados cuando sea necesario.

Un `IWorldStore` ofrece operaciones atómicas de carga, append y checkpoint.
El adaptador inicial puede usar SQLite local por su integridad, portabilidad y
capacidad de inspección, pero ésta es una decisión de infraestructura: el
contrato no debe exigir SQLite ni impedir un backend futuro.

## Migración y recuperación

Cada migración es una transformación explícita de `n → n+1`, probada con saves
reales de referencia. Antes de mutar una partida se crea una copia recuperable;
si falla la validación, se conserva el save anterior y se explica la causa.

No se permite abrir una partida si faltan packs o sus hashes no coinciden.
Las rutas posibles son restaurar el pack original, instalar una migración
publicada o abrir una copia de sólo lectura para exportar datos. Nunca se
«adivinan» reglas nuevas sobre hechos antiguos.

## Seguridad

Los packs son datos declarativos por defecto. El cargador limita tamaño,
profundidad, rutas de assets y tipos permitidos; no ejecuta C#, procesos, red
ni scripts del pack. Un futuro escape hatch requerirá sandbox, permisos y una
propuesta separada.

## Pruebas obligatorias

1. Importar el mismo origen produce el mismo pack/hash.
2. Un Reader alternativo con el mismo modelo normalizado produce el mismo
   resultado compilado.
3. Referencias, dependencias y versiones incompatibles se rechazan con un
   diagnóstico útil.
4. Save + hechos se carga al mismo estado que el snapshot original.
5. Toda migración mantiene los invariantes y puede recuperarse tras un fallo.
