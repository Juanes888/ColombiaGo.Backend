using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface ILugarService
{
    Task<PagedResult<LugarResumenDto>> BuscarAsync(LugarQueryParams query, CancellationToken ct = default);

    Task<IReadOnlyList<LugarResumenDto>> GetDestacadosAsync(int take, CancellationToken ct = default);

    Task<Result<LugarDetalleDto>> GetPorSlugAsync(string slug, CancellationToken ct = default);
}
