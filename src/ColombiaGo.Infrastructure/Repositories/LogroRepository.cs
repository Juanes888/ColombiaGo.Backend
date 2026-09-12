using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class LogroRepository : Repository<Logro, int>, ILogroRepository
{
    public LogroRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Logro>> GetAllAsync(CancellationToken ct = default) =>
        await Set.OrderBy(l => l.Id).ToListAsync(ct);
}
