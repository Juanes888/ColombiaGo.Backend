using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class LugarRepository : Repository<Lugar, int>, ILugarRepository
{
    public LugarRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public Task<Lugar?> GetBySlugWithDetailsAsync(string slug, CancellationToken ct = default) =>
        Set
            .Include(l => l.Categoria)
            .Include(l => l.Galeria)
            .Include(l => l.Caracteristicas).ThenInclude(lc => lc.Caracteristica)
            .Include(l => l.Atributos).ThenInclude(la => la.TipoAtributo)
            .FirstOrDefaultAsync(l => l.Slug == slug, ct);

    public Task<Lugar?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(l => l.Slug == slug, ct);

    public async Task<PagedResult<Lugar>> SearchAsync(LugarQueryParams query, CancellationToken ct = default)
    {
        var filtered = Set
            .Include(l => l.Categoria)
            .Where(l => l.EstadoPublicacion == EstadoPublicacion.Aprobado)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.CategoriaSlug))
            filtered = filtered.Where(l => l.Categoria.Slug == query.CategoriaSlug);

        if (!string.IsNullOrWhiteSpace(query.Ciudad))
            filtered = filtered.Where(l => EF.Functions.ILike(l.Ciudad, query.Ciudad));

        if (!string.IsNullOrWhiteSpace(query.Region))
            filtered = filtered.Where(l => EF.Functions.ILike(l.Region, query.Region));

        if (query.RangoPrecio is { } rango)
            filtered = filtered.Where(l => l.RangoPrecio == rango);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{query.Q}%";
            filtered = filtered.Where(l =>
                EF.Functions.ILike(l.Nombre, pattern) ||
                EF.Functions.ILike(l.Descripcion, pattern) ||
                EF.Functions.ILike(l.Ciudad, pattern));
        }

        var totalCount = await filtered.CountAsync(ct);

        var items = await filtered
            .OrderByDescending(l => l.CalificacionPromedio)
            .ThenBy(l => l.Nombre)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Lugar>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<Lugar>> GetDestacadosAsync(int take, CancellationToken ct = default) =>
        await Set
            .Include(l => l.Categoria)
            .Where(l => l.EstadoPublicacion == EstadoPublicacion.Aprobado && l.Destacado)
            .OrderByDescending(l => l.CalificacionPromedio)
            .Take(take)
            .ToListAsync(ct);

    public async Task<PagedResult<Lugar>> GetPendientesAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var filtered = Set
            .Include(l => l.Categoria)
            .Include(l => l.UsuarioRegistrador)
            .Where(l => l.EstadoPublicacion == EstadoPublicacion.Pendiente);

        var totalCount = await filtered.CountAsync(ct);

        var items = await filtered
            .OrderBy(l => l.FechaRegistro)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Lugar>(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<Lugar>> GetTopRatedAsync(
        int take, IReadOnlyCollection<int>? categoriaIds, CancellationToken ct = default)
    {
        var filtered = Set
            .Include(l => l.Categoria)
            .Where(l => l.EstadoPublicacion == EstadoPublicacion.Aprobado);

        if (categoriaIds is { Count: > 0 })
            filtered = filtered.Where(l => categoriaIds.Contains(l.CategoriaId));

        return await filtered
            .OrderByDescending(l => l.CalificacionPromedio)
            .ThenByDescending(l => l.TotalCalificaciones)
            .Take(take)
            .ToListAsync(ct);
    }
}
