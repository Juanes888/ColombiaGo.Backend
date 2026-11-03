# ColombiaGO — Backend

API REST en C#/.NET 9 con Clean Architecture y principios SOLID para
[ColombiaGO](https://github.com/JOSED0824/colombia_go), sobre PostgreSQL en Supabase.

## Estructura

```
src/
  ColombiaGo.Domain/          Entidades, enums, Result/Error — sin dependencias externas
  ColombiaGo.Application/     DTOs, contratos de repositorio, servicios de caso de uso, validadores
  ColombiaGo.Infrastructure/  EF Core, repositorios, autenticación JWT de Supabase
  ColombiaGo.Api/             Controllers, middleware, Program.cs
tests/
  ColombiaGo.UnitTests/       xUnit + FluentAssertions + EF Core InMemory
db/
  001_schema.sql              Tipos enum, tablas, índices, triggers
  002_seed.sql                Roles, categorías, logros, y los 18 lugares + 4 guías del mock del front
  003_rls.sql                 Políticas de Row Level Security (defensa para el acceso directo a Supabase)
  004_i18n_onboarding_guias.sql  Idioma + onboarding por usuario, caché de traducciones, guías de viaje IA, más lugares de Medellín
  005_hardening.sql           Permisos de funciones y extensión pg_trgm fuera del esquema público (avisos de seguridad de Supabase)
  006_logros_insignias.sql    10 logros con insignia + reglas reales de desbloqueo (función sincronizar_logros)
  007_guias_perfil.sql        Perfil completo de los guías: presentación en 5 idiomas, años de experiencia y tours
  008_roles_negocio.sql       Rol "negocio", rol según el tipo de cuenta al registrarse y bucket de fotos
docs/
  api-contract.md             Cada endpoint ↔ qué pantalla del front lo consume
  deploy.md                   Cómo desplegar la API (Render/Docker) y conectar la app
  frontend-integration.md     Cómo reemplazar los mocks del front por llamadas reales
  supabase-email-templates.md Plantillas de correo (código de 6 dígitos, 5 idiomas) y SMTP propio
```

## Requisitos

- .NET SDK 9.0 (el proyecto apunta a `net9.0`; ver nota de migración a .NET 10 más abajo)
- Un proyecto de Supabase (o cualquier PostgreSQL 14+) para correr contra datos reales

## Levantar el proyecto

```bash
dotnet restore
dotnet build ColombiaGo.Backend.sln
```

### Base de datos

En el **SQL Editor de Supabase** (o vía `psql`), en este orden:

```bash
# 1. Esquema (tablas, enums, triggers)
# 2. Datos semilla (roles, categorías, lugares, guías)
# 3. Row Level Security (opcional, recomendado si expones la API de datos de Supabase directamente)
```
Pega el contenido de `db/001_schema.sql`, luego `002_seed.sql`, `003_rls.sql`, `004_i18n_onboarding_guias.sql`,
`005_hardening.sql`, `006_logros_insignias.sql`, `007_guias_perfil.sql` y `008_roles_negocio.sql` (la 004 es necesaria para idiomas, onboarding y la guía de viaje con IA).

### Correo con código de verificación

Al registrarse, la app pide un código de 6 dígitos que Supabase manda por correo. Hay que cambiar las plantillas
una vez en el dashboard: ver [`docs/supabase-email-templates.md`](docs/supabase-email-templates.md).

### Secretos (nunca en `appsettings.json`)

```bash
cd src/ColombiaGo.Api
dotnet user-secrets set "ConnectionStrings:ColombiaGoDb" "Host=db.<project-ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<tu-password>;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Supabase:Url" "https://<project-ref>.supabase.co"
# Solo si tu proyecto usa el JWT secret legado (HS256) en vez de firma asimétrica:
# dotnet user-secrets set "Supabase:JwtSecret" "<tu-jwt-secret>"

# Gemini (Google AI Studio): traduce los lugares al idioma del usuario, arma la guía de viaje y la ruta con IA.
# Sin esta clave la API funciona, pero esos tres endpoints responden con un error claro.
dotnet user-secrets set "Gemini:ApiKey" "<tu-api-key>"
```

### Idiomas

La API lee `Accept-Language` (`es`, `en`, `pt`, `fr`, `zh`; por defecto `es`). El contenido de los lugares se traduce con
Gemini la primera vez que se pide en ese idioma y queda guardado en `lugar_traducciones` (las siguientes peticiones no
llaman a la IA). Para dejar todo traducido de antemano: `POST /api/v1/admin/traducciones/precalentar` (rol admin).

### Correr

```bash
dotnet run --project src/ColombiaGo.Api
```

Swagger queda en `https://localhost:<puerto>/swagger` — incluye el botón **Authorize** para pegar
un `access_token` de Supabase y probar los endpoints protegidos. Health check en `/health`.

### Tests

```bash
dotnet test
```

## Nota sobre la versión de .NET

El usuario pidió **.NET 10**, pero esta máquina solo tiene los SDKs 6.0.428 y 9.0.313 instalados
(no hay SDK de .NET 10 disponible). Se generó todo en **net9.0** para poder compilar, correr tests
y verificar de punta a punta hoy mismo. Migrar a net10.0 cuando instales ese SDK es:

1. Cambiar `<TargetFramework>net9.0</TargetFramework>` → `net10.0` en `Directory.Build.props`.
2. Actualizar los paquetes de EF Core / Npgsql / JwtBearer a sus versiones 10.x (verificadas
   disponibles en NuGet al momento de escribir esto).
3. `dotnet build` y `dotnet test` para confirmar que nada se rompió.

## Documentación

- [`docs/api-contract.md`](docs/api-contract.md) — cada endpoint, con qué pantalla del front lo
  consume, y las desviaciones deliberadas respecto al modelo ER original.
- [`docs/frontend-integration.md`](docs/frontend-integration.md) — cómo reemplazar los mocks del
  front (`src/data/*.ts`, `localStorage`) por llamadas reales a esta API.
