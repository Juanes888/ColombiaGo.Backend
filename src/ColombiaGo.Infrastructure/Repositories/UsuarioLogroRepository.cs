using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class UsuarioLogroRepository : Repository<UsuarioLogro, int>, IUsuarioLogroRepository
{
    public UsuarioLogroRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<UsuarioLogro>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await Set.Include(ul => ul.Logro).Where(ul => ul.UsuarioId == usuarioId).ToListAsync(ct);
}
