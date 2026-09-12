# Integración con el frontend (colombia_go)

El front en [JOSED0824/colombia_go](https://github.com/JOSED0824/colombia_go) hoy funciona 100%
con datos mock (`src/data/destinations.ts`, `src/data/guides.ts`) y `localStorage`
(`FavoritesContext`, `SavedRoutesContext`, `SettingsPage`). Esta guía muestra cómo reemplazar cada
pieza por una llamada real a esta API, sin cambiar la forma en que las pantallas ya consumen los
datos (los DTOs están diseñados para eso — ver `docs/api-contract.md`).

## 1. Cliente API mínimo

Crea `src/lib/api.ts` en el proyecto del front:

```ts
import { createClient } from '@supabase/supabase-js'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7000'

export const supabase = createClient(
  import.meta.env.VITE_SUPABASE_URL!,
  import.meta.env.VITE_SUPABASE_ANON_KEY!,
)

async function authHeader(): Promise<HeadersInit> {
  const { data } = await supabase.auth.getSession()
  const token = data.session?.access_token
  return token ? { Authorization: `Bearer ${token}` } : {}
}

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, { headers: await authHeader() })
  if (!res.ok) throw new Error(`GET ${path} -> ${res.status}`)
  return res.json()
}

export async function apiSend<T>(path: string, method: 'POST' | 'PUT' | 'DELETE' | 'PATCH', body?: unknown): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: { 'Content-Type': 'application/json', ...(await authHeader()) },
    body: body ? JSON.stringify(body) : undefined,
  })
  if (!res.ok) throw new Error(`${method} ${path} -> ${res.status}`)
  return res.status === 204 ? (undefined as T) : res.json()
}
```

Instala el SDK de Supabase en el front: `npm install @supabase/supabase-js`. Agrega a `.env.local`:

```
VITE_API_BASE_URL=https://localhost:7000
VITE_SUPABASE_URL=https://<project-ref>.supabase.co
VITE_SUPABASE_ANON_KEY=<anon-key>
```

## 2. Reemplazos pantalla por pantalla

| Archivo del front | Cambio |
|---|---|
| `src/data/destinations.ts` | Elimina `DESTINATIONS`, `CATEGORIES`, `HOME_CATEGORIES`. Reemplaza sus usos por `apiGet('/api/v1/lugares?...')` y `apiGet('/api/v1/categorias')`. El tipo `Destination` ya coincide con `LugarResumenDto` — no hace falta tocar los componentes que lo consumen. |
| `src/pages/ExplorePage.tsx` | El `useMemo` que filtra `DESTINATIONS` en memoria se reemplaza por un `useEffect` que llama `apiGet<PagedResult<LugarResumenDto>>(\`/api/v1/lugares?q=${query}&categoria=${category}&ciudad=${city}\`)`. |
| `src/pages/HomePage.tsx` | `FEATURED` (carrusel) → `apiGet('/api/v1/lugares/destacados?take=3')`. Los `spots` de "Imperdibles de Medellín" → `apiGet('/api/v1/lugares?ciudad=medellin&pageSize=4')`. |
| `src/pages/DestinationPage.tsx` | `DESTINATIONS.find(...)` → `apiGet(\`/api/v1/lugares/${id}\`)`, que ya trae `isFavorite`/`isInRoute`. |
| `src/context/FavoritesContext.tsx` | `toggle(id)` deja de escribir a `localStorage` y en su lugar llama `apiSend(\`/api/v1/favoritos/lugares/${id}\`, has(id) ? 'DELETE' : 'POST')`. `has(id)` puede seguir siendo estado local hidratado desde `apiGet('/api/v1/favoritos/lugares')` al montar. |
| `src/context/SavedRoutesContext.tsx` | Mismo patrón contra `/api/v1/rutas/mis-paradas/{id}` (POST/DELETE) y `GET /api/v1/rutas/mis-paradas` para hidratar. El botón "Generar ruta" (nuevo, no existe aún en el front) llamaría `POST /api/v1/rutas/generar`. |
| `src/data/guides.ts` | `GUIDES` → `apiGet('/api/v1/guias')`. El tipo `Guide` ya coincide con `GuiaDto`. |
| `src/pages/ProfilePage.tsx` | `USER` → `apiGet('/api/v1/perfil')`. `ACHIEVEMENTS` (hardcodeado) → `apiGet('/api/v1/perfil/logros')`. |
| `src/pages/SettingsPage.tsx` | `readSettings()`/`localStorage.setItem` → `apiGet('/api/v1/perfil/configuracion')` y `apiSend('/api/v1/perfil/configuracion', 'PUT', settings)`. |
| `src/pages/DestinationPage.tsx` (reseñas) | No existe aún UI de reseñas en el front — `GET/POST /api/v1/lugares/{id}/calificaciones` quedan listos para cuando se agregue. |

## 3. Login con Supabase

El front necesita implementar el login real (`AuthScreen.tsx` en `mobile/` ya tiene una versión
de referencia, aunque es la app React Native, no la web). Patrón mínimo con el SDK:

```ts
await supabase.auth.signInWithPassword({ email, password })
// o signInWithOAuth, signUp, etc. — cualquier método del SDK de Supabase Auth sirve;
// la API solo valida el JWT resultante, no le importa cómo se obtuvo.
```

Tras el login, `authHeader()` (arriba) ya adjunta el token a cada llamada automáticamente.

## 4. CORS

`appsettings.json` de la API trae `Cors:AllowedOrigins: ["http://localhost:5173"]` (el puerto por
defecto de Vite). Si el front corre en otro puerto/dominio, agrégalo ahí (o vía la variable de
entorno `Cors__AllowedOrigins__0`, etc., en producción).

## 5. Qué falta del lado del front (fuera de este alcance)

- UI para reseñas/calificaciones (el endpoint ya existe).
- Botón "Generar ruta" en `SavedRoutesPage` (el endpoint ya existe).
- Pantalla de eventos (el endpoint ya existe; el ER lo modela, el front aún no).
- Panel de administración (moderación de lugares/reseñas, reportes) — los endpoints ya existen
  bajo `/api/v1/admin/...`, pero no hay ninguna pantalla en el front que los use todavía.
