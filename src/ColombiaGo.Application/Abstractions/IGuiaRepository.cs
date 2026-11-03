using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IGuiaRepository : IRepository<Guia, int>
{
    Task<IReadOnlyList<Guia>> SearchAsync(string? especialidad, string? ciudad, CancellationToken ct = default);

    Task<Guia?> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Perfiles que aún no verifica un admin (no salen en la app hasta que se verifiquen).</summary>
    Task<IReadOnlyList<Guia>> GetSinVerificarAsync(CancellationToken ct = default);
}
