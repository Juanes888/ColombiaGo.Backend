using ColombiaGo.Application.DTOs.Negocio;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface INegocioService
{
    Task<IReadOnlyList<MiLugarDto>> GetMisLugaresAsync(CancellationToken ct = default);

    Task<Result<MiLugarDto>> CrearAsync(GuardarLugarRequest request, CancellationToken ct = default);

    Task<Result<MiLugarDto>> ActualizarAsync(int id, GuardarLugarRequest request, CancellationToken ct = default);
}
