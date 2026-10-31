using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IUsuarioLogroRepository : IRepository<UsuarioLogro, int>
{
    /// <summary>The user's unlocked achievements, with <see cref="Logro"/> eagerly loaded.</summary>
    Task<IReadOnlyList<UsuarioLogro>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>
    /// Evaluates the unlock rules for the user and records the achievements they already meet
    /// (the rules live in the database function <c>sincronizar_logros</c>, see db/006). Never removes one.
    /// </summary>
    Task SincronizarAsync(Guid usuarioId, CancellationToken ct = default);
}
