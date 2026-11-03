# Correos de verificación con código (Supabase Auth)

La app pide un **código de 6 dígitos** al registrarse y al recuperar la contraseña. Supabase manda por defecto un
*enlace*; hay que cambiar las plantillas para que el correo traiga `{{ .Token }}` (el código). El MCP de Supabase no
puede editar plantillas ni SMTP, así que se hace una sola vez en el dashboard:

**Dashboard → Authentication → Email Templates** (proyecto `ColombiaGo`, ref `ozbvwugphvmbbrwceyzo`).

La app guarda el idioma elegido en el registro (`user_metadata.idioma`: `es|en|pt|fr|zh`), por eso las plantillas usan
`{{ .Data.idioma }}` para escribir el correo en ese idioma. Si no viene, se usa español.

## 0. Que los enlaces no apunten a localhost (URL Configuration)

Si el correo trae un enlace que abre `localhost`, es porque Supabase usa su **Site URL** por defecto
(`http://localhost:3000`). La app ya pide que los enlaces vuelvan a la página `/auth/confirmado` del backend, pero
Supabase solo lo acepta si esa URL está permitida:

**Dashboard → Authentication → URL Configuration**

| Campo | Valor |
|---|---|
| **Site URL** | `https://colombiago-api.onrender.com/auth/confirmado` |
| **Redirect URLs** (Add URL) | `https://colombiago-api.onrender.com/auth/confirmado` |

Esa página dice "¡Listo! Tu correo quedó verificado, vuelve a la app" en el idioma del teléfono. Con las plantillas de
abajo el correo trae además el **código de 6 dígitos**, que es lo que la app pide.

> **Importante:** Supabase solo deja editar las plantillas después de configurar un **SMTP propio**
> (Authentication → Emails → SMTP Settings; por ejemplo Resend, gratis hasta 3.000 correos/mes). Mientras tanto se
> envían las plantillas por defecto, que traen un **enlace** en vez del código. La app lo soporta: la persona toca el
> enlace (que ahora abre `/auth/confirmado`, no localhost) y en la app presiona **"Ya confirmé con el enlace"**.

## 1. Confirm signup

**Subject:** `ColombiaGO · Tu código / Your code / Seu código / Votre code / 验证码`

**Message body (HTML):**

```html
<div style="font-family:Arial,Helvetica,sans-serif;max-width:480px;margin:auto;padding:24px;color:#003B7D">
  <h2 style="margin:0 0 8px">ColombiaGO 🇨🇴</h2>

  {{ if eq .Data.idioma "en" }}
    <p>Welcome! Enter this code in the app to verify your email:</p>
  {{ else if eq .Data.idioma "pt" }}
    <p>Bem-vindo! Digite este código no app para verificar seu e-mail:</p>
  {{ else if eq .Data.idioma "fr" }}
    <p>Bienvenue ! Saisissez ce code dans l'application pour vérifier votre e-mail :</p>
  {{ else if eq .Data.idioma "zh" }}
    <p>欢迎！请在应用中输入此验证码以验证您的邮箱：</p>
  {{ else }}
    <p>¡Bienvenido! Escribe este código en la app para verificar tu correo:</p>
  {{ end }}

  <p style="font-size:34px;font-weight:bold;letter-spacing:10px;background:#FFD100;border-radius:12px;padding:14px 18px;text-align:center">{{ .Token }}</p>

  {{ if eq .Data.idioma "en" }}
    <p style="font-size:12px;color:#6B7C93">The code expires in 1 hour. If you didn't create an account, ignore this email.</p>
  {{ else if eq .Data.idioma "pt" }}
    <p style="font-size:12px;color:#6B7C93">O código expira em 1 hora. Se você não criou uma conta, ignore este e-mail.</p>
  {{ else if eq .Data.idioma "fr" }}
    <p style="font-size:12px;color:#6B7C93">Le code expire dans 1 heure. Si vous n'avez pas créé de compte, ignorez cet e-mail.</p>
  {{ else if eq .Data.idioma "zh" }}
    <p style="font-size:12px;color:#6B7C93">验证码 1 小时后失效。如果您没有注册账号，请忽略此邮件。</p>
  {{ else }}
    <p style="font-size:12px;color:#6B7C93">El código vence en 1 hora. Si no creaste una cuenta, ignora este correo.</p>
  {{ end }}
</div>
```

## 2. Reset password (recuperación)

**Subject:** `ColombiaGO · Recupera tu contraseña / Reset your password / Redefinir senha / Réinitialiser / 重置密码`

**Message body (HTML):**

```html
<div style="font-family:Arial,Helvetica,sans-serif;max-width:480px;margin:auto;padding:24px;color:#003B7D">
  <h2 style="margin:0 0 8px">ColombiaGO 🇨🇴</h2>

  {{ if eq .Data.idioma "en" }}
    <p>Use this code in the app to reset your password:</p>
  {{ else if eq .Data.idioma "pt" }}
    <p>Use este código no app para redefinir sua senha:</p>
  {{ else if eq .Data.idioma "fr" }}
    <p>Utilisez ce code dans l'application pour réinitialiser votre mot de passe :</p>
  {{ else if eq .Data.idioma "zh" }}
    <p>请在应用中输入此验证码以重置密码：</p>
  {{ else }}
    <p>Usa este código en la app para restablecer tu contraseña:</p>
  {{ end }}

  <p style="font-size:34px;font-weight:bold;letter-spacing:10px;background:#FFD100;border-radius:12px;padding:14px 18px;text-align:center">{{ .Token }}</p>

  {{ if eq .Data.idioma "en" }}
    <p style="font-size:12px;color:#6B7C93">If you didn't request this, you can ignore this email.</p>
  {{ else if eq .Data.idioma "pt" }}
    <p style="font-size:12px;color:#6B7C93">Se você não solicitou isso, ignore este e-mail.</p>
  {{ else if eq .Data.idioma "fr" }}
    <p style="font-size:12px;color:#6B7C93">Si vous n'êtes pas à l'origine de cette demande, ignorez cet e-mail.</p>
  {{ else if eq .Data.idioma "zh" }}
    <p style="font-size:12px;color:#6B7C93">如果这不是您本人的操作，请忽略此邮件。</p>
  {{ else }}
    <p style="font-size:12px;color:#6B7C93">Si no lo pediste tú, ignora este correo.</p>
  {{ end }}
</div>
```

## 3. Entrega a usuarios reales (SMTP propio)

El SMTP integrado de Supabase **solo entrega a correos de miembros de tu organización** y con un límite muy bajo por hora:
sirve para probar con tu propio correo, no para usuarios reales. Para producción:

1. Crea una cuenta en un proveedor SMTP (p. ej. **Resend**, gratis hasta 3.000 correos/mes) y verifica tu dominio.
2. **Authentication → SMTP Settings → Enable Custom SMTP** y pega host, puerto, usuario, contraseña y remitente.
3. **Authentication → Rate Limits**: ajusta "emails sent per hour" a lo que necesites.

## 4. Comprobación

1. En la app, elige un idioma en la pantalla de acceso y regístrate con tu correo.
2. Debe llegar un correo con un código de 6 dígitos **en ese idioma**; escríbelo en la pantalla "Verifica tu correo".
3. Si llega un enlace en vez de un código, la plantilla "Confirm signup" no quedó guardada con `{{ .Token }}`.
