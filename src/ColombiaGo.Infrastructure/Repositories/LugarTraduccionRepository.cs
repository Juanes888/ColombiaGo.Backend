using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class LugarTraduccionRepository : Repository<LugarTraduccion, int>, ILugarTraduccionRepository
{
    public LugarTraduccionRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<LugarTraduccion>> GetAsync(
        IReadOnlyCollection<int> lugarIds, string idioma, CancellationToken ct = default) =>
        await Set.Where(t => t.Idioma == idioma && lugarIds.Contains(t.LugarId)).ToListAsync(ct);

    public async Task GuardarAsync(IReadOnlyCollection<LugarTraduccion> traducciones, CancellationToken ct = default)
    {
        if (traducciones.Count == 0) return;

        await Set.AddRangeAsync(traducciones, ct);
        try
        {
            await Context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Otra petición guardó la misma (lugar, idioma) primero: no es un error, la caché ya existe.
            // Se sueltan las filas fallidas para no contaminar el siguiente SaveChanges de esta petición.
            foreach (var entry in Context.ChangeTracker.Entries<LugarTraduccion>()
                         .Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;
        }
    }
}
