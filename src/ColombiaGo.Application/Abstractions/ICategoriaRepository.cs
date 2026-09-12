using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface ICategoriaRepository : IRepository<Categoria, int>
{
    Task<IReadOnlyList<Categoria>> GetActivasAsync(CancellationToken ct = default);

    Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
