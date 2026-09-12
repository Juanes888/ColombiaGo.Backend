using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class CategoriaRepository : Repository<Categoria, int>, ICategoriaRepository
{
    public CategoriaRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Categoria>> GetActivasAsync(CancellationToken ct = default) =>
        await Set.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync(ct);

    public Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(c => c.Slug == slug, ct);
}
