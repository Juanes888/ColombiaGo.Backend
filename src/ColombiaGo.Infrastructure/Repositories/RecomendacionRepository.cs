using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class RecomendacionRepository : Repository<Recomendacion, int>, IRecomendacionRepository
{
    public RecomendacionRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Recomendacion>> GetByUsuarioAsync(
        Guid usuarioId, int take, CancellationToken ct = default) =>
        await Set
            .Include(r => r.Lugar).ThenInclude(l => l.Categoria)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaGenerada)
            .Take(take)
            .ToListAsync(ct);
}
