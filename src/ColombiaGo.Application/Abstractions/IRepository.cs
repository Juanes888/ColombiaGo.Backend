using System.Linq.Expressions;

namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Minimal CRUD contract shared by every repository. Specific repositories (see
/// <c>ILugarRepository</c>, <c>IFavoritoLugarRepository</c>, …) extend this with the queries
/// their aggregate actually needs — this base never grows to fit any one of them (ISP).
/// </summary>
public interface IRepository<TEntity, in TId> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
