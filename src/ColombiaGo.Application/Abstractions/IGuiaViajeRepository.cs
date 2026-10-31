using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IGuiaViajeRepository : IRepository<GuiaViaje, int>
{
    Task<IReadOnlyList<GuiaViaje>> GetByUsuarioAsync(Guid usuarioId, int take, CancellationToken ct = default);

    /// <summary>Cuántas guías generó la persona desde <paramref name="desde"/> (límite de uso de la IA).</summary>
    Task<int> CountDesdeAsync(Guid usuarioId, DateTimeOffset desde, CancellationToken ct = default);
}
