using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface IUsuarioRepository : IRepository<Usuario, Guid>
{
    Task<Usuario?> GetByIdWithRolAsync(Guid id, CancellationToken ct = default);
}
