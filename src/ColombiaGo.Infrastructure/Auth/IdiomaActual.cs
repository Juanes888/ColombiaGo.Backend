using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using Microsoft.AspNetCore.Http;

namespace ColombiaGo.Infrastructure.Auth;

/// <summary>
/// Idioma de la petición según la cabecera <c>Accept-Language</c>. Respeta el orden de preferencia del
/// cliente (por ejemplo "zh-CN,zh;q=0.9,en;q=0.8") y se queda con el primero que la app soporta;
/// si ninguno coincide, español.
/// </summary>
public sealed class IdiomaActual : IIdiomaActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdiomaActual(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string Codigo
    {
        get
        {
            var cabecera = _httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();
            if (string.IsNullOrWhiteSpace(cabecera)) return Idiomas.Espanol;

            var preferidos = cabecera
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Parsear)
                .OrderByDescending(x => x.Calidad);

            foreach (var (codigo, _) in preferidos)
            {
                var corto = codigo.ToLowerInvariant().Split('-', '_')[0];
                if (Idiomas.EsSoportado(corto)) return corto;
            }

            return Idiomas.Espanol;
        }
    }

    private static (string Codigo, double Calidad) Parsear(string item)
    {
        var partes = item.Split(';', 2, StringSplitOptions.TrimEntries);
        var calidad = 1.0;
        if (partes.Length == 2 && partes[1].StartsWith("q=", StringComparison.OrdinalIgnoreCase))
            double.TryParse(partes[1][2..], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out calidad);
        return (partes[0], calidad);
    }
}
