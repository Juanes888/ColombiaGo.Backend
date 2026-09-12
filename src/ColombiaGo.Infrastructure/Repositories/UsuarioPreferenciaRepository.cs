using ColombiaGo.Application.Abstractions;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class UsuarioPreferenciaRepository : IUsuarioPreferenciaRepository
{
    private readonly ColombiaGoDbContext _context;

    public UsuarioPreferenciaRepository(ColombiaGoDbContext context) => _context = context;

    public async Task<IReadOnlyList<int>> GetCategoriaIdsByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await _context.UsuarioPreferencias
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => p.CategoriaId)
            .ToListAsync(ct);
}
