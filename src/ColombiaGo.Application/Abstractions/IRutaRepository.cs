using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IRutaRepository : IRepository<Ruta, int>
{
    /// <summary>The user's current open (not-yet-generated) route, with its stops and each
    /// stop's <see cref="Lugar"/> eagerly loaded. Null if the user has no draft yet.</summary>
    Task<Ruta?> GetDraftWithParadasAsync(Guid usuarioId, CancellationToken ct = default);
}
