using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class GuiaRepository : Repository<Guia, int>, IGuiaRepository
{
    public GuiaRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Guia>> SearchAsync(
        string? especialidad, string? ciudad, CancellationToken ct = default)
    {
        var filtered = Set.Where(g => g.Verificado).AsQueryable();

        if (!string.IsNullOrWhiteSpace(especialidad))
            filtered = filtered.Where(g => EF.Functions.ILike(g.Especialidad, $"%{especialidad}%"));

        if (!string.IsNullOrWhiteSpace(ciudad))
            filtered = filtered.Where(g => EF.Functions.ILike(g.Lugar, $"%{ciudad}%"));

        return await filtered
            .OrderByDescending(g => g.Destacado)
            .ThenByDescending(g => g.CalificacionPromedio)
            .ToListAsync(ct);
    }

    public Task<Guia?> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(g => g.UsuarioId == usuarioId, ct);
}
