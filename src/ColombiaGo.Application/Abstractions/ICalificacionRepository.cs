using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface ICalificacionRepository : IRepository<Calificacion, int>
{
    /// <summary>Reviews for a place. <paramref name="onlyAprobadas"/> is true for the public
    /// detail page (only moderated reviews are shown) and false for the admin queue.</summary>
    Task<PagedResult<Calificacion>> GetByLugarAsync(
        int lugarId, bool onlyAprobadas, int page, int pageSize, CancellationToken ct = default);

    Task<bool> ExistsForUsuarioLugarAsync(Guid usuarioId, int lugarId, CancellationToken ct = default);

    Task<PagedResult<Calificacion>> GetPendientesAsync(int page, int pageSize, CancellationToken ct = default);

    Task<int> CountByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}
