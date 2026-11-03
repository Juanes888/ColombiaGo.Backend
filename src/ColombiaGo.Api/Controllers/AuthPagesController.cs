using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

/// <summary>
/// Páginas a las que llega la persona cuando toca el enlace de un correo de Supabase (confirmar cuenta, recuperar
/// contraseña). Supabase verifica el enlace y redirige aquí; sin esta página redirigía a "localhost", que en el
/// teléfono no abre nada. La app usa el código de 6 dígitos, así que esta página solo confirma y pide volver a la app.
/// </summary>
[ApiController]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthPagesController : ControllerBase
{
    [HttpGet("/auth/confirmado")]
    public ContentResult Confirmado() => Content(Html, "text/html; charset=utf-8");

    // Texto en los 5 idiomas de la app; se muestra el del navegador (el resto queda oculto).
    private const string Html = """
        <!doctype html>
        <html lang="es">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>ColombiaGO</title>
          <style>
            body { margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #FFF8EC;
                   font-family: -apple-system, Roboto, 'Segoe UI', Arial, sans-serif; color: #003B7D; padding: 24px; box-sizing: border-box; }
            .card { max-width: 420px; background: #fff; border-radius: 28px; padding: 32px 28px; text-align: center; box-shadow: 0 10px 30px rgba(0,0,0,.08); }
            .flag { height: 6px; width: 90px; margin: 0 auto 22px; border-radius: 3px; background: linear-gradient(90deg, #FFD100 50%, #003B7D 50% 75%, #E63946 75%); }
            .ok { width: 70px; height: 70px; border-radius: 50%; background: #FFD100; margin: 0 auto 18px; display: flex; align-items: center; justify-content: center; font-size: 36px; }
            h1 { font-size: 24px; margin: 0 0 10px; }
            p { color: #4A6580; line-height: 1.5; margin: 0; }
            [data-l] { display: none; }
          </style>
        </head>
        <body>
          <div class="card">
            <div class="flag"></div>
            <div class="ok">&#10003;</div>
            <div data-l="es"><h1>¡Listo!</h1><p>Tu correo quedó verificado. Vuelve a la app ColombiaGO e inicia sesión.</p></div>
            <div data-l="en"><h1>All set!</h1><p>Your email is verified. Go back to the ColombiaGO app and sign in.</p></div>
            <div data-l="pt"><h1>Pronto!</h1><p>Seu e-mail foi verificado. Volte ao app ColombiaGO e entre na sua conta.</p></div>
            <div data-l="fr"><h1>C'est fait !</h1><p>Votre e-mail est vérifié. Revenez dans l'application ColombiaGO et connectez-vous.</p></div>
            <div data-l="zh"><h1>完成！</h1><p>您的邮箱已验证。请返回 ColombiaGO 应用并登录。</p></div>
          </div>
          <script>
            var l = (navigator.language || 'es').slice(0, 2).toLowerCase();
            var el = document.querySelector('[data-l="' + l + '"]') || document.querySelector('[data-l="es"]');
            el.style.display = 'block';
          </script>
        </body>
        </html>
        """;
}
