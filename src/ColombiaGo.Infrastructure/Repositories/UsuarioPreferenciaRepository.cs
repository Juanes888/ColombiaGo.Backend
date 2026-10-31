using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
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

    public async Task ReemplazarAsync(Guid usuarioId, IReadOnlyCollection<int> categoriaIds, CancellationToken ct = default)
    {
        var actuales = await _context.UsuarioPreferencias.Where(p => p.UsuarioId == usuarioId).ToListAsync(ct);
        _context.UsuarioPreferencias.RemoveRange(actuales.Where(p => !categoriaIds.Contains(p.CategoriaId)));

        var existentes = actuales.Select(p => p.CategoriaId).ToHashSet();
        foreach (var categoriaId in categoriaIds.Where(id => !existentes.Contains(id)))
            _context.UsuarioPreferencias.Add(new UsuarioPreferencia { UsuarioId = usuarioId, CategoriaId = categoriaId });
    }
}
