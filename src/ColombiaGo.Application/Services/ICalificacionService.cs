using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Calificaciones;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface ICalificacionService
{
    Task<Result<PagedResult<CalificacionDto>>> GetByLugarAsync(
        string lugarSlug, int page, int pageSize, CancellationToken ct = default);

    Task<Result<CalificacionDto>> CrearAsync(
        string lugarSlug, CrearCalificacionRequest request, CancellationToken ct = default);
}
