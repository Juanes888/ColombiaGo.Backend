using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IGuiaRepository : IRepository<Guia, int>
{
    Task<IReadOnlyList<Guia>> SearchAsync(string? especialidad, string? ciudad, CancellationToken ct = default);

    Task<Guia?> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default);
}
