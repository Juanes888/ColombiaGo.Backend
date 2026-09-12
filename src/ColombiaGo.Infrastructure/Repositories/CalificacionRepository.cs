using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Repositories;

public class CalificacionRepository : Repository<Calificacion, int>, ICalificacionRepository
{
    public CalificacionRepository(ColombiaGoDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Calificacion>> GetByLugarAsync(
        int lugarId, bool onlyAprobadas, int page, int pageSize, CancellationToken ct = default)
    {
        var filtered = Set.Include(c => c.Usuario).Where(c => c.LugarId == lugarId);
        if (onlyAprobadas)
            filtered = filtered.Where(c => c.EstadoModeracion == EstadoModeracion.Aprobada);

        var totalCount = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderByDescending(c => c.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Calificacion>(items, page, pageSize, totalCount);
    }

    public Task<bool> ExistsForUsuarioLugarAsync(Guid usuarioId, int lugarId, CancellationToken ct = default) =>
        Set.AnyAsync(c => c.UsuarioId == usuarioId && c.LugarId == lugarId, ct);

    public async Task<PagedResult<Calificacion>> GetPendientesAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var filtered = Set
            .Include(c => c.Usuario)
            .Include(c => c.Lugar)
            .Where(c => c.EstadoModeracion == EstadoModeracion.Pendiente);

        var totalCount = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(c => c.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Calificacion>(items, page, pageSize, totalCount);
    }

    public Task<int> CountByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        Set.CountAsync(c => c.UsuarioId == usuarioId, ct);
}
