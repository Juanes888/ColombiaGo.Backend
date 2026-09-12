using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IFavoritoEventoRepository : IRepository<FavoritoEvento, int>
{
    Task<IReadOnlyList<FavoritoEvento>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    Task<FavoritoEvento?> FindAsync(Guid usuarioId, int eventoId, CancellationToken ct = default);
}
