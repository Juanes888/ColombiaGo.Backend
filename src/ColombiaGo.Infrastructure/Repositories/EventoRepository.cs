using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class EventoRepository : Repository<Evento, int>, IEventoRepository
{
    public EventoRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Evento>> SearchAsync(
        string? ciudad, int page, int pageSize, CancellationToken ct = default)
    {
        var filtered = Set.Include(e => e.Lugar).AsQueryable();

        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            var pattern = $"%{ciudad}%";
            filtered = filtered.Where(e =>
                (e.Lugar != null && EF.Functions.ILike(e.Lugar.Ciudad, pattern)) ||
                EF.Functions.ILike(e.DireccionEvento, pattern));
        }

        var totalCount = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(e => e.FechaEvento).ThenBy(e => e.HoraEvento)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Evento>(items, page, pageSize, totalCount);
    }
}
