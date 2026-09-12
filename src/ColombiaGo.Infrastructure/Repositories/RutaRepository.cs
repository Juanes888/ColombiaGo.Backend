using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class RutaRepository : Repository<Ruta, int>, IRutaRepository
{
    public RutaRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public Task<Ruta?> GetDraftWithParadasAsync(Guid usuarioId, CancellationToken ct = default) =>
        Set
            .Include(r => r.Paradas).ThenInclude(p => p.Lugar).ThenInclude(l => l.Categoria)
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.FechaGeneracion == null, ct);
}
