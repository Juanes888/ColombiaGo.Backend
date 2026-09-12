using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface ILogroRepository : IRepository<Logro, int>
{
    Task<IReadOnlyList<Logro>> GetAllAsync(CancellationToken ct = default);
}
