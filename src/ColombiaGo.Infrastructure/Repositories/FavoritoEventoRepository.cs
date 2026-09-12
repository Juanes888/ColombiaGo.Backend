using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class FavoritoEventoRepository : Repository<FavoritoEvento, int>, IFavoritoEventoRepository
{
    public FavoritoEventoRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<FavoritoEvento>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await Set
            .Include(f => f.Evento).ThenInclude(e => e.Lugar)
            .Where(f => f.UsuarioId == usuarioId)
            .ToListAsync(ct);

    public Task<FavoritoEvento?> FindAsync(Guid usuarioId, int eventoId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.EventoId == eventoId, ct);
}
