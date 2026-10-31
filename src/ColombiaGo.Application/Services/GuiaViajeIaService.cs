using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.GuiaViaje;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

public sealed class GuiaViajeIaService : IGuiaViajeIaService
{
    private const int MaxGuiasPorHora = 10;
    private const int MaxCandidatos = 40;
    private const int MaxGuiasListadas = 30;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IGeminiClient _gemini;
    private readonly IIdiomaActual _idioma;
    private readonly ILugarLocalizador _localizador;

    public GuiaViajeIaService(
        IUnitOfWork uow, ICurrentUser currentUser, IGeminiClient gemini,
        IIdiomaActual idioma, ILugarLocalizador localizador)
    {
        _uow = uow;
        _currentUser = currentUser;
        _gemini = gemini;
        _idioma = idioma;
        _localizador = localizador;
    }

    public async Task<Result<GuiaViajeDto>> GenerarAsync(GenerarGuiaViajeRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        if (!_gemini.IsConfigured)
            return Result<GuiaViajeDto>.Failure(
                Error.Unexpected("GuiaIa.SinApiKey", "La guía con IA no está configurada todavía."));

        var recientes = await _uow.GuiasViaje.CountDesdeAsync(userId, DateTimeOffset.UtcNow.AddHours(-1), ct);
        if (recientes >= MaxGuiasPorHora)
            return Result<GuiaViajeDto>.Failure(Error.TooManyRequests(
                "GuiaIa.LimiteAlcanzado", "Generaste muchas guías en poco tiempo. Intenta de nuevo en un rato."));

        var intereses = (request.Intereses ?? []).Distinct().ToList();
        var ciudad = request.Ciudad.Trim();

        var candidatos = await _uow.Lugares.GetPorCiudadAsync(ciudad, intereses, MaxCandidatos, ct);
        if (intereses.Count > 0 && candidatos.Count < 3)
            candidatos = await _uow.Lugares.GetPorCiudadAsync(ciudad, null, MaxCandidatos, ct);

        if (candidatos.Count == 0)
            return Result<GuiaViajeDto>.Failure(Error.Validation(
                "GuiaIa.CiudadSinLugares", $"Todavía no tenemos lugares en '{ciudad}'."));

        var idioma = _idioma.Codigo;
        string? json;
        try
        {
            json = await _gemini.GenerarJsonAsync(
                ConstruirPrompt(ciudad, request.Dias, request.Presupuesto, intereses, candidatos, idioma), ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return Result<GuiaViajeDto>.Failure(
                Error.Unexpected("GuiaIa.Error", $"No se pudo generar la guía: {ex.Message}"));
        }

        var contenido = Validar(Parsear(json), candidatos, request.Dias);
        if (contenido is null)
            return Result<GuiaViajeDto>.Failure(Error.Unexpected(
                "GuiaIa.RespuestaInvalida", "La IA no devolvió una guía válida. Intenta de nuevo."));

        var guia = new GuiaViaje
        {
            UsuarioId = userId,
            Ciudad = ciudad,
            Dias = request.Dias,
            Presupuesto = request.Presupuesto,
            Intereses = intereses,
            Idioma = idioma,
            Contenido = JsonSerializer.Serialize(contenido, JsonOptions),
            FechaCreacion = DateTimeOffset.UtcNow,
        };

        await _uow.GuiasViaje.AddAsync(guia, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<GuiaViajeDto>.Success(await ADtoAsync(guia, ct));
    }

    public async Task<IReadOnlyList<GuiaViajeResumenDto>> ListarAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var guias = await _uow.GuiasViaje.GetByUsuarioAsync(userId, MaxGuiasListadas, ct);

        return guias
            .Select(g => new GuiaViajeResumenDto(
                g.Id,
                Deserializar(g.Contenido)?.Titulo is { Length: > 0 } titulo ? titulo : g.Ciudad,
                g.Ciudad, g.Dias, g.Presupuesto, g.FechaCreacion))
            .ToList();
    }

    public async Task<Result<GuiaViajeDto>> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var guia = await _uow.GuiasViaje.GetByIdAsync(id, ct);

        // Una guía ajena se reporta como inexistente: no se revela que existe.
        if (guia is null || guia.UsuarioId != userId)
            return Result<GuiaViajeDto>.Failure(Error.NotFound("GuiaIa.NotFound", $"No existe la guía '{id}'."));

        return Result<GuiaViajeDto>.Success(await ADtoAsync(guia, ct));
    }

    public async Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken ct = default)
    {
        var ciudades = await _uow.Lugares.GetCiudadesAsync(ct);
        return ciudades.Select(c => new CiudadDto(c.Ciudad, c.Total)).ToList();
    }

    // ---- Prompt ----------------------------------------------------------------------------

    private static string ConstruirPrompt(
        string ciudad, int dias, decimal presupuesto, IReadOnlyList<string> intereses,
        IReadOnlyList<Lugar> candidatos, string idioma)
    {
        var lineas = candidatos.Select(l =>
            $"- slug: \"{l.Slug}\" | {l.Nombre} | {l.Categoria.Nombre} | city: {l.Ciudad} | hours: {l.HorarioAtencion ?? "n/a"} | " +
            $"entry: {CostoTexto(l)} | transport: {l.ComoLlegar ?? "n/a"} | {l.Eslogan} | " +
            $"coords: {l.Latitud.ToString(CultureInfo.InvariantCulture)},{l.Longitud.ToString(CultureInfo.InvariantCulture)}");

        var interesesTexto = intereses.Count == 0 ? "no preference (mix of everything)" : string.Join(", ", intereses);

        return $$"""
            You are an expert local travel planner for the app ColombiaGO. Create a {{dias}}-day travel guide for {{ciudad}}, Colombia.
            The traveler's total budget is {{presupuesto.ToString("N0", CultureInfo.InvariantCulture)}} COP (entry fees, food and local transport, all included).
            Interests (categories): {{interesesTexto}}.
            Write EVERY text value in {{Idiomas.NombreParaPrompt(idioma)}}.

            Available places. Use ONLY these slugs, each at most once:
            {{string.Join('\n', lineas)}}

            Rules:
            - Group nearby places on the same day (use the coordinates) and order them in a logical route.
            - Places whose city is not "{{ciudad}}" are day trips (about 2 hours away): plan at most one per day, with few other stops that day.
            - 2 to 4 stops per day, with realistic start times in 24h "HH:mm" format; respect the opening hours when given.
            - "costoAprox" is COP per person for that stop: entry fee plus a reasonable food/transport estimate (free places can still cost food or transport).
            - Try to keep the total within the budget; if that is impossible, choose fewer or cheaper stops and say so in "resumen".
            - If there are not enough places to fill all {{dias}} days, return fewer days and mention it in "resumen".
            - "consejo" is one practical tip for that stop. "consejosGenerales" has 3 to 6 general tips. "queLlevar" has 3 to 8 items to pack.

            Respond ONLY with valid JSON, no markdown, with exactly this shape:
            {"titulo":"...","resumen":"...","dias":[{"dia":1,"tema":"...","paradas":[{"slug":"...","hora":"09:00","duracionMin":90,"costoAprox":25000,"consejo":"..."}]}],"consejosGenerales":["..."],"queLlevar":["..."]}
            """;
    }

    private static string CostoTexto(Lugar lugar)
    {
        if (lugar.RangoPrecio == RangoPrecio.Gratis) return "free";
        if (lugar.CostoEntrada is > 0) return $"~{lugar.CostoEntrada.Value.ToString("N0", CultureInfo.InvariantCulture)} COP";
        return lugar.RangoPrecio.ToString().ToLowerInvariant();
    }

    // ---- Validación de lo que devuelve la IA -------------------------------------------------

    private static ContenidoGuia? Parsear(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<ContenidoGuia>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ContenidoGuia? Deserializar(string json) => Parsear(json);

    /// <summary>Descarta slugs inventados o repetidos, días vacíos y valores fuera de rango; renumera los
    /// días y calcula el costo total con los costos por parada (no se confía en la suma de la IA).</summary>
    private static ContenidoGuia? Validar(ContenidoGuia? crudo, IReadOnlyList<Lugar> candidatos, int diasMax)
    {
        if (crudo is null) return null;

        var validos = candidatos.Select(l => l.Slug).ToHashSet();
        var usados = new HashSet<string>();
        var dias = new List<DiaGuia>();

        foreach (var dia in crudo.Dias.Take(diasMax))
        {
            var paradas = new List<ParadaGuia>();
            foreach (var p in dia.Paradas)
            {
                if (!validos.Contains(p.Slug) || !usados.Add(p.Slug)) continue;
                paradas.Add(new ParadaGuia
                {
                    Slug = p.Slug,
                    Hora = Recortar(p.Hora, 10) ?? "",
                    DuracionMin = Math.Clamp(p.DuracionMin, 15, 600),
                    CostoAprox = Math.Max(0, p.CostoAprox),
                    Consejo = Recortar(p.Consejo, 400) ?? "",
                });
            }

            if (paradas.Count == 0) continue;
            dias.Add(new DiaGuia { Dia = dias.Count + 1, Tema = Recortar(dia.Tema, 150) ?? "", Paradas = paradas });
        }

        if (dias.Count == 0) return null;

        return new ContenidoGuia
        {
            Titulo = Recortar(crudo.Titulo, 150) ?? "",
            Resumen = Recortar(crudo.Resumen, 800) ?? "",
            Dias = dias,
            CostoTotalEstimado = dias.SelectMany(d => d.Paradas).Sum(p => p.CostoAprox),
            ConsejosGenerales = Limpiar(crudo.ConsejosGenerales, 8, 300),
            QueLlevar = Limpiar(crudo.QueLlevar, 10, 80),
        };
    }

    private static List<string> Limpiar(IEnumerable<string> items, int max, int largo) =>
        items.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => Recortar(i, largo)!).Take(max).ToList();

    private static string? Recortar(string? valor, int max) =>
        valor is null ? null : valor.Length <= max ? valor : valor[..max];

    // ---- Armado del DTO ----------------------------------------------------------------------

    private async Task<GuiaViajeDto> ADtoAsync(GuiaViaje guia, CancellationToken ct)
    {
        var contenido = Deserializar(guia.Contenido) ?? new ContenidoGuia();
        var slugs = contenido.Dias.SelectMany(d => d.Paradas).Select(p => p.Slug).Distinct().ToList();

        var lugares = await _uow.Lugares.GetBySlugsAsync(slugs, ct);
        var resumenes = (await _localizador.ResumenAsync(lugares.ToList(), ct)).ToDictionary(r => r.Id);

        var plan = contenido.Dias
            .Select(d => new GuiaViajeDiaDto(
                d.Dia, d.Tema,
                d.Paradas
                    .Where(p => resumenes.ContainsKey(p.Slug))
                    .Select(p => new GuiaViajeParadaDto(resumenes[p.Slug], p.Hora, p.DuracionMin, p.CostoAprox, p.Consejo))
                    .ToList()))
            .Where(d => d.Paradas.Count > 0)
            .ToList();

        var total = plan.SelectMany(d => d.Paradas).Sum(p => p.CostoAprox);
        var restante = guia.Presupuesto - total;

        return new GuiaViajeDto(
            guia.Id, guia.Ciudad, guia.Dias, guia.Presupuesto, guia.Intereses, guia.Idioma,
            string.IsNullOrWhiteSpace(contenido.Titulo) ? guia.Ciudad : contenido.Titulo,
            contenido.Resumen, plan, total, restante, restante < 0,
            contenido.ConsejosGenerales, contenido.QueLlevar, guia.FechaCreacion);
    }

    // ---- Forma del JSON (la devuelve la IA y también es lo que se guarda en la BD) -----------

    private sealed class ContenidoGuia
    {
        [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
        [JsonPropertyName("resumen")] public string Resumen { get; set; } = "";
        [JsonPropertyName("dias")] public List<DiaGuia> Dias { get; set; } = [];
        [JsonPropertyName("costoTotalEstimado")] public decimal CostoTotalEstimado { get; set; }
        [JsonPropertyName("consejosGenerales")] public List<string> ConsejosGenerales { get; set; } = [];
        [JsonPropertyName("queLlevar")] public List<string> QueLlevar { get; set; } = [];
    }

    private sealed class DiaGuia
    {
        [JsonPropertyName("dia")] public int Dia { get; set; }
        [JsonPropertyName("tema")] public string Tema { get; set; } = "";
        [JsonPropertyName("paradas")] public List<ParadaGuia> Paradas { get; set; } = [];
    }

    private sealed class ParadaGuia
    {
        [JsonPropertyName("slug")] public string Slug { get; set; } = "";
        [JsonPropertyName("hora")] public string Hora { get; set; } = "";
        [JsonPropertyName("duracionMin")] public int DuracionMin { get; set; }
        [JsonPropertyName("costoAprox")] public decimal CostoAprox { get; set; }
        [JsonPropertyName("consejo")] public string Consejo { get; set; } = "";
    }
}
