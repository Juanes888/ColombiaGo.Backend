using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Calificaciones;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

public sealed class CalificacionService : ICalificacionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IModeracionSettings _moderacion;

    public CalificacionService(IUnitOfWork uow, ICurrentUser currentUser, IModeracionSettings moderacion)
    {
        _uow = uow;
        _currentUser = currentUser;
        _moderacion = moderacion;
    }

    public async Task<Result<PagedResult<CalificacionDto>>> GetByLugarAsync(
        string lugarSlug, int page, int pageSize, CancellationToken ct = default)
    {
        var lugar = await _uow.Lugares.GetBySlugAsync(lugarSlug, ct);
        if (lugar is null)
            return Result<PagedResult<CalificacionDto>>.Failure(
                Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarSlug}'."));

        var pageResult = await _uow.Calificaciones.GetByLugarAsync(lugar.Id, onlyAprobadas: true, page, pageSize, ct);
        var items = pageResult.Items.Select(c => c.ToDto()).ToList();
        return Result<PagedResult<CalificacionDto>>.Success(
            new PagedResult<CalificacionDto>(items, pageResult.Page, pageResult.PageSize, pageResult.TotalCount));
    }

    public async Task<Result<CalificacionDto>> CrearAsync(
        string lugarSlug, CrearCalificacionRequest request, CancellationToken ct = default)
    {
        if (request.Puntuacion is < 1 or > 5)
            return Result<CalificacionDto>.Failure(
                Error.Validation("Calificacion.PuntuacionInvalida", "La puntuación debe estar entre 1 y 5."));

        var userId = _currentUser.RequireUserId();

        var lugar = await _uow.Lugares.GetBySlugAsync(lugarSlug, ct);
        if (lugar is null)
            return Result<CalificacionDto>.Failure(
                Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarSlug}'."));

        if (await _uow.Calificaciones.ExistsForUsuarioLugarAsync(userId, lugar.Id, ct))
            return Result<CalificacionDto>.Failure(
                Error.Conflict("Calificacion.YaExiste", "Ya calificaste este lugar."));

        var calificacion = new Calificacion
        {
            UsuarioId = userId,
            LugarId = lugar.Id,
            Puntuacion = request.Puntuacion,
            Comentario = request.Comentario,
            EstadoModeracion = _moderacion.AutoAprobarCalificaciones
                ? EstadoModeracion.Aprobada
                : EstadoModeracion.Pendiente,
            Fecha = DateTimeOffset.UtcNow,
        };

        await _uow.Calificaciones.AddAsync(calificacion, ct);
        await _uow.SaveChangesAsync(ct);

        // Reload with Usuario included so the response carries the reviewer's name/photo.
        var usuario = await _uow.Usuarios.GetByIdAsync(userId, ct);
        calificacion.Usuario = usuario!;

        return Result<CalificacionDto>.Success(calificacion.ToDto());
    }
}
