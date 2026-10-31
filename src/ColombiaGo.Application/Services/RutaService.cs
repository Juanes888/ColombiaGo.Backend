using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

public sealed class RutaService : IRutaService
{
    /// <summary>Average minutes budgeted per stop (visit time + travel to the next one). A
    /// documented placeholder heuristic, not a real travel-time estimate — replacing it with a
    /// routing-API-backed calculation is a natural follow-up once one is wired in.</summary>
    private const int MinutosPorParada = 45;

    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ILugarLocalizador _localizador;

    public RutaService(IUnitOfWork uow, ICurrentUser currentUser, ILugarLocalizador localizador)
    {
        _uow = uow;
        _currentUser = currentUser;
        _localizador = localizador;
    }

    public async Task<RutaDto> GetMisParadasAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var draft = await _uow.Rutas.GetDraftWithParadasAsync(userId, ct);
        return draft is null ? EmptyRutaDto() : await ToRutaDtoAsync(draft, ct);
    }

    public async Task<Result<RutaDto>> AgregarParadaAsync(string lugarSlug, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var lugar = await _uow.Lugares.GetBySlugAsync(lugarSlug, ct);
        if (lugar is null)
            return Result<RutaDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarSlug}'."));

        var draft = await _uow.Rutas.GetDraftWithParadasAsync(userId, ct);
        if (draft is null)
        {
            draft = new Ruta { UsuarioId = userId, FechaGeneracion = null };
            await _uow.Rutas.AddAsync(draft, ct);
        }

        if (draft.Paradas.All(p => p.LugarId != lugar.Id))
        {
            var siguienteOrden = draft.Paradas.Count == 0 ? 1 : draft.Paradas.Max(p => p.OrdenOptimo) + 1;
            draft.Paradas.Add(new RutaLugar { LugarId = lugar.Id, Lugar = lugar, OrdenOptimo = siguienteOrden });
        }

        await _uow.SaveChangesAsync(ct);
        return Result<RutaDto>.Success(await ToRutaDtoAsync(draft, ct));
    }

    public async Task<Result<RutaDto>> QuitarParadaAsync(string lugarSlug, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var lugar = await _uow.Lugares.GetBySlugAsync(lugarSlug, ct);
        if (lugar is null)
            return Result<RutaDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarSlug}'."));

        var draft = await _uow.Rutas.GetDraftWithParadasAsync(userId, ct);
        if (draft is null)
            return Result<RutaDto>.Success(EmptyRutaDto());

        var parada = draft.Paradas.FirstOrDefault(p => p.LugarId == lugar.Id);
        if (parada is not null)
            draft.Paradas.Remove(parada);

        if (draft.Paradas.Count == 0)
            _uow.Rutas.Remove(draft);

        await _uow.SaveChangesAsync(ct);
        return Result<RutaDto>.Success(draft.Paradas.Count == 0 ? EmptyRutaDto() : await ToRutaDtoAsync(draft, ct));
    }

    public async Task<Result<RutaDto>> GenerarAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var draft = await _uow.Rutas.GetDraftWithParadasAsync(userId, ct);
        if (draft is null || draft.Paradas.Count == 0)
            return Result<RutaDto>.Failure(Error.Validation("Ruta.SinParadas", "Agrega al menos una parada antes de generar la ruta."));

        var ordenadas = OrdenarPorCercania(draft.Paradas.ToList());
        for (var i = 0; i < ordenadas.Count; i++)
            ordenadas[i].OrdenOptimo = i + 1;

        draft.FechaGeneracion = DateTimeOffset.UtcNow;
        draft.TiempoEstimadoMin = draft.Paradas.Count * MinutosPorParada;

        await _uow.SaveChangesAsync(ct);
        return Result<RutaDto>.Success(await ToRutaDtoAsync(draft, ct));
    }

    /// <summary>Greedy nearest-neighbor ordering over (latitud, longitud) treated as a flat plane
    /// — a reasonable approximation at intra-city scale, not geodesic distance.</summary>
    private static List<RutaLugar> OrdenarPorCercania(List<RutaLugar> paradas)
    {
        var restantes = new List<RutaLugar>(paradas);
        var resultado = new List<RutaLugar>(paradas.Count);

        var actual = restantes[0];
        restantes.RemoveAt(0);
        resultado.Add(actual);

        while (restantes.Count > 0)
        {
            var siguiente = restantes.OrderBy(p => DistanciaPlana(actual.Lugar, p.Lugar)).First();
            resultado.Add(siguiente);
            restantes.Remove(siguiente);
            actual = siguiente;
        }

        return resultado;
    }

    private static double DistanciaPlana(Lugar a, Lugar b)
    {
        var dLat = (double)(a.Latitud - b.Latitud);
        var dLng = (double)(a.Longitud - b.Longitud);
        return Math.Sqrt(dLat * dLat + dLng * dLng);
    }

    private async Task<RutaDto> ToRutaDtoAsync(Ruta ruta, CancellationToken ct)
    {
        var lugares = ruta.Paradas.OrderBy(p => p.OrdenOptimo).Select(p => p.Lugar).ToList();
        var paradas = await _localizador.ResumenAsync(lugares, ct);
        return new RutaDto(ruta.Id, ruta.FechaGeneracion is not null, ruta.FechaGeneracion, ruta.TiempoEstimadoMin, paradas);
    }

    private static RutaDto EmptyRutaDto() => new(0, false, null, null, Array.Empty<Application.DTOs.Lugares.LugarResumenDto>());
}
