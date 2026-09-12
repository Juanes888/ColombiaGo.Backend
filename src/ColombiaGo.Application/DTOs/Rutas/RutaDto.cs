using ColombiaGo.Application.DTOs.Lugares;

namespace ColombiaGo.Application.DTOs.Rutas;

/// <summary>
/// The user's route/trip plan. While <see cref="TiempoEstimadoMin"/>/<see cref="Generated"/> are
/// unset it's an open draft (mirrors <c>SavedRoutesContext</c>); after <c>POST /rutas/generar</c>
/// it carries an optimized stop order and a generation timestamp.
/// </summary>
public sealed record RutaDto(
    int Id,
    bool Generated,
    DateTimeOffset? FechaGeneracion,
    int? TiempoEstimadoMin,
    IReadOnlyList<LugarResumenDto> Paradas);
