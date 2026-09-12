using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

/// <summary>
/// Manages the user's open route/trip draft — the backend counterpart of the frontend's
/// <c>SavedRoutesContext</c> (today a flat set of ids in localStorage).
/// </summary>
public interface IRutaService
{
    Task<RutaDto> GetMisParadasAsync(CancellationToken ct = default);

    Task<Result<RutaDto>> AgregarParadaAsync(string lugarSlug, CancellationToken ct = default);

    Task<Result<RutaDto>> QuitarParadaAsync(string lugarSlug, CancellationToken ct = default);

    /// <summary>Closes the current draft: computes an optimized visiting order and an estimated
    /// duration, stamping <c>FechaGeneracion</c>. The next added stop starts a new draft.</summary>
    Task<Result<RutaDto>> GenerarAsync(CancellationToken ct = default);
}
