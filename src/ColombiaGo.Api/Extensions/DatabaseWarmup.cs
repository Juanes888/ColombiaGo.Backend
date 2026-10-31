using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Api.Extensions;

/// <summary>
/// Trabajo en segundo plano al arrancar, para que la app no espere en la primera visita:
///  1. Abre la conexión a la base y ejecuta una consulta (EF construye su modelo y el pool queda listo; sin esto la
///     primera petición tarda ~2 s más que las siguientes).
///  2. Si hay clave de Gemini, traduce al inglés, portugués, francés y chino los lugares que aún no estén traducidos.
///     Se guardan en la base, así que solo la primera vez que se enciende el backend toma unos segundos; después no cuesta nada.
/// </summary>
public sealed class DatabaseWarmup : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<DatabaseWarmup> _logger;

    public DatabaseWarmup(IServiceScopeFactory scopes, ILogger<DatabaseWarmup> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopes.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ColombiaGoDbContext>();
                if (!db.Database.IsRelational()) return;

                var lugares = await db.Set<Lugar>().AsNoTracking().CountAsync(stoppingToken);
                var guias = await db.Set<Guia>().AsNoTracking().CountAsync(stoppingToken);
                _logger.LogInformation("Base de datos lista ({Lugares} lugares, {Guias} guías).", lugares, guias);
            }

            if (!_scopes.CreateScope().ServiceProvider.GetRequiredService<IGeminiClient>().IsConfigured) return;

            foreach (var idioma in Idiomas.Soportados.Where(i => i != Idiomas.Espanol))
            {
                using var scope = _scopes.CreateScope();
                var localizador = scope.ServiceProvider.GetRequiredService<ILugarLocalizador>();
                var nuevas = await localizador.PrecalentarAsync(idioma, stoppingToken);
                if (nuevas > 0) _logger.LogInformation("Traducidos {Nuevas} lugares al idioma '{Idioma}'.", nuevas, idioma);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Es una mejora: si falla, la primera petición lo reportará con su propio error y el arranque sigue.
            _logger.LogWarning(ex, "No se pudo completar el calentamiento inicial.");
        }
    }
}
