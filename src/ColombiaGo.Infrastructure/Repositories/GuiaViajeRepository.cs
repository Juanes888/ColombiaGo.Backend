using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class GuiaViajeRepository : Repository<GuiaViaje, int>, IGuiaViajeRepository
{
    public GuiaViajeRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<GuiaViaje>> GetByUsuarioAsync(Guid usuarioId, int take, CancellationToken ct = default) =>
        await Set.Where(g => g.UsuarioId == usuarioId)
            .OrderByDescending(g => g.FechaCreacion)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> CountDesdeAsync(Guid usuarioId, DateTimeOffset desde, CancellationToken ct = default) =>
        Set.CountAsync(g => g.UsuarioId == usuarioId && g.FechaCreacion >= desde, ct);
}
