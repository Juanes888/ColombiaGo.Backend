using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

public sealed class RutaIaService : IRutaIaService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IUnitOfWork _uow;
    private readonly IGeminiClient _gemini;

    public RutaIaService(IUnitOfWork uow, IGeminiClient gemini)
    {
        _uow = uow;
        _gemini = gemini;
    }

    public async Task<Result<RutaIaDto>> GenerarConIaAsync(GenerarRutaIaRequest request, CancellationToken ct = default)
    {
        if (!_gemini.IsConfigured)
            return Result<RutaIaDto>.Failure(
                Error.Unexpected("RutaIa.SinApiKey", "El planificador con IA no está configurado todavía."));

        var lugares = new List<Lugar>();
        foreach (var slug in request.LugarSlugs.Distinct())
        {
            var lugar = await _uow.Lugares.GetBySlugWithDetailsAsync(slug, ct);
            if (lugar is not null) lugares.Add(lugar);
        }

        if (lugares.Count == 0)
            return Result<RutaIaDto>.Failure(
                Error.Validation("RutaIa.SinLugares", "Ninguno de los lugares enviados existe."));

        string? json;
        try
        {
            json = await _gemini.GenerarJsonAsync(ConstruirPrompt(lugares, request.Presupuesto), ct);
        }
        catch (Exception ex)
        {
            return Result<RutaIaDto>.Failure(
                Error.Unexpected("RutaIa.Error", $"No se pudo generar la ruta: {ex.Message}"));
        }

        var plan = ParsearPlan(json);
        if (plan is null || plan.OrdenSlugs.Count == 0)
            return Result<RutaIaDto>.Failure(
                Error.Unexpected("RutaIa.RespuestaInvalida", "La IA no devolvió una ruta válida. Intenta de nuevo."));

        var porSlug = lugares.ToDictionary(l => l.Slug);
        var paradas = plan.OrdenSlugs
            .Where(porSlug.ContainsKey)
            .Select(slug => porSlug[slug].ToResumenDto())
            .ToList();

        if (paradas.Count == 0)
            return Result<RutaIaDto>.Failure(
                Error.Unexpected("RutaIa.RespuestaInvalida", "La IA no devolvió una ruta válida. Intenta de nuevo."));

        return Result<RutaIaDto>.Success(new RutaIaDto(paradas, plan.CostoEstimado, plan.Explicacion));
    }

    private static string ConstruirPrompt(IReadOnlyList<Lugar> lugares, decimal presupuesto)
    {
        var lineas = lugares.Select(l =>
            $"- slug: \"{l.Slug}\" | {l.Nombre} ({l.Ciudad}, {l.Categoria.Nombre}) | costo aproximado: {CostoTexto(l)} | {l.Eslogan}");

        return $$"""
            Eres un planificador de viajes para la app ColombiaGO. El usuario tiene un presupuesto de
            {{presupuesto.ToString("N0", CultureInfo.InvariantCulture)}} COP y quiere visitar algunos de estos lugares:

            {{string.Join('\n', lineas)}}

            Elige el subconjunto y el orden de visita que mejor aproveche el presupuesto (puedes dejar
            fuera lugares si no caben todos). Responde ÚNICAMENTE con JSON válido, sin texto adicional
            ni markdown, con esta forma exacta:
            {"ordenSlugs": ["slug1", "slug2"], "costoEstimado": 123000, "explicacion": "una frase breve en español explicando la elección"}
            Usa solo los slugs de la lista de arriba, en el orden sugerido de visita.
            """;
    }

    private static string CostoTexto(Lugar lugar)
    {
        if (lugar.RangoPrecio == RangoPrecio.Gratis) return "gratis";
        if (lugar.CostoEntrada is > 0) return $"~{lugar.CostoEntrada.Value.ToString("N0", CultureInfo.InvariantCulture)} COP";
        return lugar.RangoPrecio.ToString().ToLowerInvariant();
    }

    private static GeminiPlan? ParsearPlan(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<GeminiPlan>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class GeminiPlan
    {
        [JsonPropertyName("ordenSlugs")] public List<string> OrdenSlugs { get; set; } = [];
        [JsonPropertyName("costoEstimado")] public decimal CostoEstimado { get; set; }
        [JsonPropertyName("explicacion")] public string Explicacion { get; set; } = "";
    }
}
