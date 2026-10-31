# Desplegar la API (para que la app funcione sin tu PC)

Con la API en internet, la app ya no depende de que tu computador esté encendido ni de estar en el mismo Wi-Fi: en
`mobile/.env` pones la URL pública y listo. Hay una `Dockerfile` y un `render.yaml` listos.

## Opción recomendada: Render (gratis para probar)

1. Sube este repositorio a GitHub (ya está en `Juanes888/ColombiaGo.Backend`).
2. En [render.com](https://render.com) → **New + → Blueprint** → elige el repositorio. Lee `render.yaml` y crea el servicio
   `colombiago-api` en la región **Ohio** (la misma de tu Supabase).
3. En el panel del servicio → **Environment**, pega los dos valores que son secretos:
   | Variable | Valor |
   |---|---|
   | `ConnectionStrings__ColombiaGoDb` | `Host=aws-0-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.ozbvwugphvmbbrwceyzo;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true` |
   | `Gemini__ApiKey` | tu clave de Google AI Studio |
   `Supabase__Url` y `ASPNETCORE_ENVIRONMENT` ya vienen en el blueprint.
4. Espera el primer despliegue y abre `https://<tu-servicio>.onrender.com/health`: debe decir **Healthy**.

> **Plan gratis (cámbialo en `render.yaml` a `plan: free`):** Render apaga el servicio tras 15 minutos sin tráfico y la primera petición después tarda 30-60 s en
> despertarlo. Para una demo o para usuarios reales conviene el plan *Starter* (7 USD/mes), que no se duerme.

Otras opciones equivalentes (usan el mismo `Dockerfile`): Railway, Fly.io, Google Cloud Run, Azure Container Apps.
Variables siempre con doble guion bajo (`Supabase__Url`, `Cors__AllowedOrigins__0`…). Elige una región cercana a
Supabase (us-east-2) para que las consultas sean rápidas.

## Conectar la app

1. En `mobile/.env`:
   ```
   EXPO_PUBLIC_API_URL=https://<tu-servicio>.onrender.com
   ```
   Debe ser **https** (iPhone bloquea las peticiones http a servidores públicos). Sin barra final.
2. Para probar con Expo Go: `npx expo start --clear` (las variables `EXPO_PUBLIC_*` se incluyen al empaquetar).
3. Para generar la app instalable (APK / IPA) con EAS Build, el archivo `.env` no se sube a los servidores de Expo, así que
   las variables se registran allí:
   ```bash
   npm i -g eas-cli
   eas login
   eas build:configure
   eas env:create --name EXPO_PUBLIC_API_URL --value https://<tu-servicio>.onrender.com --environment production --visibility plaintext
   eas env:create --name EXPO_PUBLIC_SUPABASE_URL --value https://<project-ref>.supabase.co --environment production --visibility plaintext
   eas env:create --name EXPO_PUBLIC_SUPABASE_KEY --value <publishable-key> --environment production --visibility plaintext
   eas build --platform android --profile production
   ```
   (La clave publishable de Supabase está pensada para ir dentro de la app; la contraseña de la base y la clave de Gemini
   **nunca** van en el front: solo en el servidor.)

## Notas de producción

- El CORS solo importa para la versión web; la app móvil no lo usa. En producción se aceptan solo los orígenes de
  `Cors__AllowedOrigins__0`, `__1`… (en desarrollo se acepta cualquiera).
- Las respuestas del catálogo se guardan 5 minutos en memoria del servidor; si el servicio se reinicia, se vuelve a llenar.
- La clave de Gemini y la contraseña de la base se rotan desde sus paneles; después se actualiza la variable en Render.
