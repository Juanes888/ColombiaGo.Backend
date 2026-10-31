using System.Text.Json;
using System.Text.Json.Serialization;
using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

public sealed class LugarLocalizador : ILugarLocalizador
{
    private const int TamanoLote = 12;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IUnitOfWork _uow;
    private readonly IIdiomaActual _idioma;
    private readonly IGeminiClient _gemini;

    public LugarLocalizador(IUnitOfWork uow, IIdiomaActual idioma, IGeminiClient gemini)
    {
        _uow = uow;
        _idioma = idioma;
        _gemini = gemini;
    }

    public string Idioma => _idioma.Codigo;

    public async Task<IReadOnlyList<LugarResumenDto>> ResumenAsync(
        IReadOnlyCollection<Lugar> lugares, CancellationToken ct = default)
    {
        var traducciones = await ObtenerAsync(lugares, Idioma, ct);
        return lugares
            .Select(l => l.ToResumenDto(Idioma, traducciones.GetValueOrDefault(l.Id)))
            .ToList();
    }

    public async Task<LugarDetalleDto> DetalleAsync(Lugar lugar, bool isFavorite, bool isInRoute, CancellationToken ct = default)
    {
        var traducciones = await ObtenerAsync([lugar], Idioma, ct);
        return lugar.ToDetalleDto(isFavorite, isInRoute, Idioma, traducciones.GetValueOrDefault(lugar.Id));
    }

    public async Task<int> PrecalentarAsync(string idioma, CancellationToken ct = default)
    {
        idioma = Idiomas.Normalizar(idioma);
        if (idioma == Idiomas.Espanol) return 0;

        var todos = new List<Lugar>();
        for (var page = 1; ; page++)
        {
            var pagina = await _uow.Lugares.SearchAsync(new LugarQueryParams { Page = page, PageSize = 100 }, ct);
            todos.AddRange(pagina.Items);
            if (page >= pagina.TotalPages) break;
        }

        var ids = todos.Select(l => l.Id).ToList();
        var antes = (await _uow.LugarTraducciones.GetAsync(ids, idioma, ct)).Count;
        var despues = await ObtenerAsync(todos, idioma, ct);
        return Math.Max(0, despues.Count - antes);
    }

    private async Task<Dictionary<int, LugarTraduccion>> ObtenerAsync(
        IReadOnlyCollection<Lugar> lugares, string idioma, CancellationToken ct)
    {
        var resultado = new Dictionary<int, LugarTraduccion>();
        if (idioma == Idiomas.Espanol || lugares.Count == 0) return resultado;

        var ids = lugares.Select(l => l.Id).Distinct().ToList();
        foreach (var t in await _uow.LugarTraducciones.GetAsync(ids, idioma, ct))
            resultado[t.LugarId] = t;

        var faltantes = lugares.Where(l => !resultado.ContainsKey(l.Id)).DistinctBy(l => l.Id).ToList();
        if (faltantes.Count == 0 || !_gemini.IsConfigured) return resultado;

        foreach (var lote in faltantes.Chunk(TamanoLote))
        {
            var nuevas = await TraducirLoteAsync(lote, idioma, ct);
            if (nuevas.Count == 0) continue;

            await _uow.LugarTraducciones.GuardarAsync(nuevas, ct);
            foreach (var t in nuevas) resultado[t.LugarId] = t;
        }

        return resultado;
    }

    private async Task<List<LugarTraduccion>> TraducirLoteAsync(Lugar[] lote, string idioma, CancellationToken ct)
    {
        var entrada = lote.Select(l => new
        {
            slug = l.Slug,
            nombre = l.Nombre,
            descripcion = l.Descripcion,
            eslogan = l.Eslogan,
            comoLlegar = l.ComoLlegar,
            horario = l.HorarioAtencion,
        });

        var prompt = $$"""
            Translate this tourism content about places in Colombia from Spanish into {{Idiomas.NombreParaPrompt(idioma)}}.
            Keep proper nouns (place, neighborhood and brand names) in their original form unless they have a
            widely used translation. Keep numbers, times and currency as they are. Keep the same tone: warm and
            inviting. If a field is null, keep it null.
            Respond ONLY with valid JSON, no markdown, with exactly this shape:
            {"traducciones":[{"slug":"...","nombre":"...","descripcion":"...","eslogan":"...","comoLlegar":"...","horario":"..."}]}
            Return one item per input item, using the same slug.

            Input:
            {{JsonSerializer.Serialize(entrada)}}
            """;

        try
        {
            var json = await _gemini.GenerarJsonAsync(prompt, ct);
            if (string.IsNullOrWhiteSpace(json)) return [];

            var respuesta = JsonSerializer.Deserialize<RespuestaTraduccion>(json, JsonOptions);
            var porSlug = lote.ToDictionary(l => l.Slug);
            var ahora = DateTimeOffset.UtcNow;

            return (respuesta?.Traducciones ?? [])
                .Where(t => porSlug.ContainsKey(t.Slug)
                            && !string.IsNullOrWhiteSpace(t.Nombre)
                            && !string.IsNullOrWhiteSpace(t.Descripcion))
                .Select(t => new LugarTraduccion
                {
                    LugarId = porSlug[t.Slug].Id,
                    Idioma = idioma,
                    Nombre = Recortar(t.Nombre, 300)!,
                    Descripcion = Recortar(t.Descripcion, 4000)!,
                    Eslogan = Recortar(t.Eslogan, 300),
                    ComoLlegar = Recortar(t.ComoLlegar, 300),
                    HorarioAtencion = Recortar(t.Horario, 300),
                    FechaCreacion = ahora,
                })
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or InvalidOperationException)
        {
            // La traducción es una mejora, no un requisito: ante un fallo de la IA se muestra el español.
            return [];
        }
    }

    private static string? Recortar(string? valor, int max) =>
        valor is null ? null : valor.Length <= max ? valor : valor[..max];

    private sealed class RespuestaTraduccion
    {
        [JsonPropertyName("traducciones")] public List<ItemTraduccion> Traducciones { get; set; } = [];
    }

    private sealed class ItemTraduccion
    {
        [JsonPropertyName("slug")] public string Slug { get; set; } = "";
        [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
        [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = "";
        [JsonPropertyName("eslogan")] public string? Eslogan { get; set; }
        [JsonPropertyName("comoLlegar")] public string? ComoLlegar { get; set; }
        [JsonPropertyName("horario")] public string? Horario { get; set; }
    }
}
