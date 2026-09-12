using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class HistorialInteraccionRepository : Repository<HistorialInteraccion, int>, IHistorialInteraccionRepository
{
    public HistorialInteraccionRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<HistorialInteraccion>> GetRecientesByUsuarioAsync(
        Guid usuarioId, int take, CancellationToken ct = default) =>
        await Set
            .Where(h => h.UsuarioId == usuarioId)
            .OrderByDescending(h => h.Fecha)
            .Take(take)
            .ToListAsync(ct);
}
