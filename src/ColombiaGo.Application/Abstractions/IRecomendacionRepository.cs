using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IRecomendacionRepository : IRepository<Recomendacion, int>
{
    /// <summary>Most recent recommendations for a user, with <see cref="Lugar"/> eagerly loaded.</summary>
    Task<IReadOnlyList<Recomendacion>> GetByUsuarioAsync(Guid usuarioId, int take, CancellationToken ct = default);
}
