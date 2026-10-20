using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface IRutaIaService
{
    /// <summary>Asks Gemini to pick and order the subset of <paramref name="request"/>'s
    /// candidate places that best fits the given budget.</summary>
    Task<Result<RutaIaDto>> GenerarConIaAsync(GenerarRutaIaRequest request, CancellationToken ct = default);
}
