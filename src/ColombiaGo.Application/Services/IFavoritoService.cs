using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Application.DTOs.Favoritos;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

/// <summary>
/// Add/remove are each idempotent (POST when already favorited, or DELETE when not, is a no-op
/// that still returns success) — deliberately not a single "toggle" flip, which would misbehave
/// on a client retry (a duplicated POST would silently undo the very favorite it just added).
/// </summary>
public interface IFavoritoService
{
    Task<IReadOnlyList<LugarResumenDto>> GetLugaresFavoritosAsync(CancellationToken ct = default);

    Task<Result<FavoritoEstadoDto>> AgregarLugarAsync(string lugarSlug, CancellationToken ct = default);

    Task<Result<FavoritoEstadoDto>> QuitarLugarAsync(string lugarSlug, CancellationToken ct = default);

    Task<IReadOnlyList<EventoDto>> GetEventosFavoritosAsync(CancellationToken ct = default);

    Task<Result<FavoritoEstadoDto>> AgregarEventoAsync(int eventoId, CancellationToken ct = default);

    Task<Result<FavoritoEstadoDto>> QuitarEventoAsync(int eventoId, CancellationToken ct = default);
}
