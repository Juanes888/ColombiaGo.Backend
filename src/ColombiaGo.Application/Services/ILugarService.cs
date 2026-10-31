using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface ILugarService
{
    Task<PagedResult<LugarResumenDto>> BuscarAsync(LugarQueryParams query, CancellationToken ct = default);

    Task<IReadOnlyList<LugarResumenDto>> GetDestacadosAsync(int take, CancellationToken ct = default);

    Task<Result<LugarDetalleDto>> GetPorSlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Detalle sin datos de la persona (favorito / en ruta siempre en false) y sin registrar la vista: se puede cachear.</summary>
    Task<Result<LugarDetalleDto>> GetContenidoAsync(string slug, CancellationToken ct = default);

    /// <summary>Registra que la persona autenticada abrió el detalle del lugar (alimenta las recomendaciones).</summary>
    Task<Result> RegistrarVistaAsync(string slug, CancellationToken ct = default);
}
