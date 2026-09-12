using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IHistorialInteraccionRepository : IRepository<HistorialInteraccion, int>
{
    Task<IReadOnlyList<HistorialInteraccion>> GetRecientesByUsuarioAsync(
        Guid usuarioId, int take, CancellationToken ct = default);
}
