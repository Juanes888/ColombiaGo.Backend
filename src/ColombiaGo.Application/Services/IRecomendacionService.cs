using ColombiaGo.Application.DTOs.Recomendaciones;

namespace ColombiaGo.Application.Services;

public interface IRecomendacionService
{
    Task<IReadOnlyList<RecomendacionDto>> GetRecomendacionesAsync(int take, CancellationToken ct = default);
}
