using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class FavoritoLugarRepository : Repository<FavoritoLugar, int>, IFavoritoLugarRepository
{
    public FavoritoLugarRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<FavoritoLugar>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await Set
            .Include(f => f.Lugar).ThenInclude(l => l.Categoria)
            .Where(f => f.UsuarioId == usuarioId)
            .ToListAsync(ct);

    public Task<FavoritoLugar?> FindAsync(Guid usuarioId, int lugarId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.LugarId == lugarId, ct);

    public Task<int> CountByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        Set.CountAsync(f => f.UsuarioId == usuarioId, ct);
}
