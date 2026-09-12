using System.Linq.Expressions;
using ColombiaGo.Application.Abstractions;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

/// <summary>
/// Shared EF Core implementation of <see cref="IRepository{TEntity,TId}"/>. Specific
/// repositories (see <c>LugarRepository</c> etc.) derive from this and add only the query
/// methods their aggregate actually needs (LSP: a specific repository is always usable wherever
/// the base contract is expected).
/// </summary>
public class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class
{
    protected ColombiaGoDbContext Context { get; }

    protected DbSet<TEntity> Set { get; }

    public Repository(ColombiaGoDbContext context)
    {
        Context = context;
        Set = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default) =>
        await Set.FindAsync([id!], ct);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Set.AnyAsync(predicate, ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Update(TEntity entity) => Set.Update(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);
}
