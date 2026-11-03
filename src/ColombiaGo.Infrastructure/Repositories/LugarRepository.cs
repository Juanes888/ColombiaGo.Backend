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

    // ToLower()/Contains sin cultura: EF las traduce a lower(...) LIKE '%...%' en Postgres y también corren en
    // el proveedor InMemory de los tests (EF.Functions.ILike solo existe en Npgsql).
#pragma warning disable CA1304, CA1311, CA1862
    public async Task<IReadOnlyList<Lugar>> GetPorCiudadAsync(
        string ciudad, IReadOnlyCollection<string>? categoriaSlugs, int take, CancellationToken ct = default)
    {
        var buscada = ciudad.ToLower();
        var filtered = Set
            .Include(l => l.Categoria)
            .Where(l => l.EstadoPublicacion == EstadoPublicacion.Aprobado && l.Ciudad.ToLower().Contains(buscada));

        if (categoriaSlugs is { Count: > 0 })
            filtered = filtered.Where(l => categoriaSlugs.Contains(l.Categoria.Slug));

        return await filtered
            .OrderByDescending(l => l.CalificacionPromedio)
            .ThenBy(l => l.Nombre)
            .Take(take)
            .ToListAsync(ct);
    }

#pragma warning restore CA1304, CA1311, CA1862

    public async Task<IReadOnlyList<Lugar>> GetBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken ct = default) =>
        await Set.Include(l => l.Categoria).Where(l => slugs.Contains(l.Slug)).ToListAsync(ct);

    public async Task<IReadOnlyList<(string Ciudad, int Total)>> GetCiudadesAsync(CancellationToken ct = default)
    {
        var filas = await Set
            .Where(l => l.EstadoPublicacion == EstadoPublicacion.Aprobado)
            .GroupBy(l => l.Ciudad)
            .Select(g => new { Ciudad = g.Key, Total = g.Count() })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Ciudad)
            .ToListAsync(ct);
        return filas.Select(f => (f.Ciudad, f.Total)).ToList();
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

    public async Task<IReadOnlyList<Lugar>> GetByRegistradorAsync(Guid usuarioId, CancellationToken ct = default) =>
        await Set
            .Include(l => l.Categoria)
            .Where(l => l.RegistradoPor == usuarioId)
            .OrderByDescending(l => l.FechaActualizacion)
            .ToListAsync(ct);
}
