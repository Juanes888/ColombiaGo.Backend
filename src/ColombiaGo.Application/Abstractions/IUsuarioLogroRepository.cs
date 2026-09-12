using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IUsuarioLogroRepository : IRepository<UsuarioLogro, int>
{
    /// <summary>The user's unlocked achievements, with <see cref="Logro"/> eagerly loaded.</summary>
    Task<IReadOnlyList<UsuarioLogro>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}
