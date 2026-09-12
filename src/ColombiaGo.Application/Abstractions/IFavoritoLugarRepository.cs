using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IFavoritoLugarRepository : IRepository<FavoritoLugar, int>
{
    /// <summary>Loads the user's favorited places with <see cref="Lugar"/> eagerly loaded.</summary>
    Task<IReadOnlyList<FavoritoLugar>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    Task<FavoritoLugar?> FindAsync(Guid usuarioId, int lugarId, CancellationToken ct = default);

    Task<int> CountByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}
