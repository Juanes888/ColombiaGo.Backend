# Contrato de API — ColombiaGO

Todos los endpoints viven bajo `/api/v1`. Autenticación: JWT de Supabase en el header
`Authorization: Bearer <access_token>`. Endpoints sin `🔒` son públicos (anónimos); `🔒` requiere
sesión; `🔒admin` / `🔒guía` requieren el rol correspondiente (`RequireAdmin` / `RequireGuia`).

Todas las respuestas de error siguen `application/problem+json` (RFC 7807): `{ title, status,
detail, instance }`.

---

## Categorías

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/categorias` | — | Lista de categorías activas. Backing de `HOME_CATEGORIES`/`CATEGORIES` del front. |

## Lugares

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/lugares` | — | Búsqueda paginada. Query: `q, categoria, ciudad, region, rangoPrecio, page, pageSize`. Backing de `ExplorePage` y de "Imperdibles de Medellín" en `HomePage` (`?ciudad=medellin&pageSize=4`). |
| GET | `/lugares/destacados?take=5` | — | Lugares con `destacado=true`. Backing del carrusel `FEATURED` de `HomePage`. |
| GET | `/lugares/{slug}` | — | Detalle completo. `{slug}` es el mismo `id` que usa `Destination` en el front. 404 si no existe o no está `aprobado`. |
| GET | `/lugares/{slug}/calificaciones?page&pageSize` | — | Reseñas **aprobadas** de un lugar. |
| POST | `/lugares/{slug}/calificaciones` | 🔒 | Crea una reseña. Body: `{ puntuacion: 1-5, comentario? }`. 409 si el usuario ya calificó ese lugar. |

## Favoritos

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/favoritos/lugares` | 🔒 | Lugares favoritos del usuario. Backing de `FavoritesContext`/`FavoritesPage`. |
| POST / DELETE | `/favoritos/lugares/{slug}` | 🔒 | Agregar/quitar — **idempotente** (repetir la misma llamada no falla ni revierte el estado). |
| GET | `/favoritos/eventos` | 🔒 | Eventos favoritos. |
| POST / DELETE | `/favoritos/eventos/{eventoId}` | 🔒 | Idempotente, igual que lugares. |

## Rutas ("Mis rutas")

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/rutas/mis-paradas` | 🔒 | El borrador de ruta abierto del usuario (paradas aún no optimizadas). Backing de `SavedRoutesContext`/`SavedRoutesPage`. |
| POST / DELETE | `/rutas/mis-paradas/{slug}` | 🔒 | Agrega/quita una parada del borrador. Idempotente. |
| POST | `/rutas/generar` | 🔒 | Cierra el borrador: calcula un orden optimizado (heurística *nearest-neighbor* sobre lat/lng, ver nota abajo) y marca `fechaGeneracion`. La siguiente parada que se agregue inicia un borrador nuevo. |

**Nota — heurística de optimización:** `RutaService` usa un *nearest-neighbor* voraz sobre
latitud/longitud tratadas como plano cartesiano (no distancia geodésica), y estima el tiempo total
como `paradas × 45 min`. Es una aproximación razonable a escala intra-ciudad, documentada como tal
en el código — no un cálculo de tiempo de viaje real. Reemplazarla por una API de ruteo real es un
follow-up natural, no incluido en este alcance.

## Guías

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/guias?especialidad&ciudad` | — | Backing de `GuidesPage`. Solo guías `verificado=true`. |
| GET | `/guias/{id}` | — | Detalle de un guía. |

**Nota:** un `Guia` es una ficha de directorio, no necesariamente ligada a una cuenta real de
Supabase (`UsuarioId` es nullable) — así se pueden sembrar los 4 guías demo del mock del front
(Mateo, Sofía, Andrés, Lucía) sin necesitar una cuenta autenticada real para cada uno. Un guía
puede vincularse después a una cuenta para autogestionar su perfil.

## Perfil

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/perfil` | 🔒 | Backing del bloque `USER` + stat card de `ProfilePage`. |
| GET | `/perfil/logros` | 🔒 | Backing de "Mis logros". |
| GET / PUT | `/perfil/configuracion` | 🔒 | Backing de `SettingsPage` (hoy solo en `localStorage` en el front). |

**Nota — `PerfilDto.guides` siempre es `0`:** no existe todavía una tabla de "reservas/uso de
guías" en el esquema — solo agregamos guías, logros y configuración (lo aprobado explícitamente).
Cuando exista esa función, este contador queda listo para poblarse.

## Eventos

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/eventos?ciudad&page&pageSize` | — | El front aún no tiene una pantalla de eventos; el ER sí los modela. |
| GET | `/eventos/{id}` | — | Detalle. |
| POST | `/eventos` | 🔒guía | Crear un evento (el creador queda como `admin_id`). |

## Recomendaciones

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/recomendaciones?take=10` | 🔒 | Heurística explicable (no ML): lugares mejor calificados en las categorías preferidas del usuario, excluyendo ya-favoritos; si no tiene preferencias, cae a los mejor calificados en general. Cada llamada persiste las recomendaciones generadas (auditoría/caché). |

## Admin

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/admin/lugares/pendientes?page&pageSize` | 🔒admin | Cola de moderación de lugares. |
| PATCH | `/admin/lugares/{id}/moderar` | 🔒admin | Body: `{ aprobar: bool, motivoRechazo? }`. |
| GET | `/admin/calificaciones/pendientes?page&pageSize` | 🔒admin | Cola de moderación de reseñas. |
| PATCH | `/admin/calificaciones/{id}/moderar` | 🔒admin | Body: `{ aprobar: bool }`. |
| POST | `/admin/reportes` | 🔒admin | Registra la solicitud de un reporte. **`archivoUrl` siempre vuelve `null`** — la generación real del archivo (PDF/Excel/CSV) está fuera de alcance; este endpoint solo deja la fila de auditoría/tracking lista para que un job de exportación la complete después. |

---

## Forma del DTO clave: `LugarResumenDto`

Mapeado 1:1 al `Destination` que el front ya consume en `src/data/destinations.ts`:

```jsonc
{
  "id": "comuna-13",            // slug — el front rutea por él (/destino/:id)
  "name": "Comuna 13: Arte y Transformación",
  "region": "Antioquia",
  "city": "Medellín",
  "rating": 4.9,
  "reviewsCount": 2000,
  "reviewsLabel": "2k reseñas",
  "category": "culture",        // slug de categoría
  "categoryLabel": "Cultura",
  "image": "/images/comuna13.jpg",
  "tagline": "Arte, historia y transformación"
}
```

`LugarDetalleDto` añade: `description, hours, price, transport, address, latitude, longitude,
gallery[], features[], attributes[], isFavorite, isInRoute`.

## Desviaciones del ER original (resumen — detalle completo en el plan de la sesión)

1. **Columnas nuevas en `lugares`**: `slug, ciudad, region, eslogan, como_llegar, destacado`. Son
   columnas reales (no EAV) porque el front filtra/ordena/rutea por ellas en cada listado.
2. **Columnas nuevas en `categorias`**: `slug, tono`.
3. **Tablas nuevas**: `guias, logros, usuario_logros, usuario_configuracion` — el front ya
   renderiza estas pantallas; el ER original no las cubría.
4. **`usuarios.nivel` y `usuarios.puntos`** — gamification que `ProfilePage` ya muestra.
5. **`lugares.registrado_por` y `guias.usuario_id` son nullable** — contenido sembrado por el
   sistema (los 18 lugares y 4 guías del mock del front) no tiene un usuario real de Supabase
   detrás; se vuelven no-nulos cuando un usuario real registra/reclama el contenido.
6. **`rutas.fecha_generacion` es nullable** — representa el estado "borrador" (acumulando
   paradas) antes de llamar a `/rutas/generar`.
7. **Enums nativos de PostgreSQL** en vez de columnas enteras — mapeados vía
   `Npgsql.HasPostgresEnum<TEnum>()`, con las mismas etiquetas en snake_case que genera Npgsql
   por defecto (`EstadoEvento.EnCurso` → `'en_curso'`). Ver comentarios en `db/001_schema.sql`.
