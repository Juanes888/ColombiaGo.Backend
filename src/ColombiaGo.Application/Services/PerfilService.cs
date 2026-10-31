using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public sealed class PerfilService : IPerfilService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public PerfilService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<PerfilDto>> GetPerfilAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var usuario = await _uow.Usuarios.GetByIdWithRolAsync(userId, ct);
        if (usuario is null)
            return Result<PerfilDto>.Failure(Error.NotFound("Usuario.NotFound", "Perfil no encontrado."));

        var places = await _uow.FavoritosLugares.CountByUsuarioAsync(userId, ct);
        var reviews = await _uow.Calificaciones.CountByUsuarioAsync(userId, ct);

        // Guides has no backing data yet — there is no guide-booking/usage table in the schema.
        // Documented in docs/api-contract.md; returns 0 until that feature exists.
        const int guides = 0;

        return Result<PerfilDto>.Success(usuario.ToPerfilDto(places, guides, reviews));
    }

    public async Task<PreferenciasDto> GetPreferenciasAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var ids = (await _uow.UsuarioPreferencias.GetCategoriaIdsByUsuarioAsync(userId, ct)).ToHashSet();
        var categorias = await _uow.Categorias.GetActivasAsync(ct);
        return new PreferenciasDto(categorias.Where(c => ids.Contains(c.Id)).Select(c => c.Slug).ToList());
    }

    public async Task<Result<PreferenciasDto>> GuardarPreferenciasAsync(
        GuardarPreferenciasRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var slugs = request.Categorias.Distinct().ToList();

        var categorias = await _uow.Categorias.GetActivasAsync(ct);
        var ids = categorias.Where(c => slugs.Contains(c.Slug)).Select(c => c.Id).ToList();
        if (ids.Count != slugs.Count)
            return Result<PreferenciasDto>.Failure(Error.Validation(
                "Perfil.CategoriaInvalida", "Alguna categoría no existe o no está activa."));

        await _uow.UsuarioPreferencias.ReemplazarAsync(userId, ids, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<PreferenciasDto>.Success(new PreferenciasDto(slugs));
    }

    public async Task<IReadOnlyList<LogroDto>> GetLogrosAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        // Antes de listar, se registran los logros que el usuario ya cumple (favoritos, guías, etc.).
        await _uow.UsuarioLogros.SincronizarAsync(userId, ct);

        var todos = await _uow.Logros.GetAllAsync(ct);
        var desbloqueados = await _uow.UsuarioLogros.GetByUsuarioAsync(userId, ct);
        var desbloqueadosPorId = desbloqueados.ToDictionary(ul => ul.LogroId, ul => ul.FechaObtenido);

        return todos
            .Select(logro =>
            {
                var unlocked = desbloqueadosPorId.TryGetValue(logro.Id, out var fechaObtenido);
                return logro.ToLogroDto(unlocked, unlocked ? fechaObtenido : null);
            })
            .ToList();
    }
}
