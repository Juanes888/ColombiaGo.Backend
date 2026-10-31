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

    public async Task SincronizarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        // La función es de PostgreSQL; con un proveedor que no es relacional (tests con InMemory) no aplica.
        if (!Context.Database.IsRelational())
            return;

        await Context.Database.ExecuteSqlInterpolatedAsync($"select sincronizar_logros({usuarioId})", ct);
    }
}
