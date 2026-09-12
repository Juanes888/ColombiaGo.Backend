using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IEventoRepository : IRepository<Evento, int>
{
    Task<PagedResult<Evento>> SearchAsync(string? ciudad, int page, int pageSize, CancellationToken ct = default);
}
