using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

public sealed class GuiaService : IGuiaService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdiomaActual _idioma;
    private readonly ICurrentUser _currentUser;

    public GuiaService(IUnitOfWork uow, IIdiomaActual idioma, ICurrentUser currentUser)
    {
        _uow = uow;
        _idioma = idioma;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GuiaDto>> BuscarAsync(
        string? especialidad, string? ciudad, CancellationToken ct = default)
    {
        var guias = await _uow.Guias.SearchAsync(especialidad, ciudad, ct);
        return guias.Select(g => g.ToDto(_idioma.Codigo)).ToList();
    }

    public async Task<Result<GuiaDto>> GetPorIdAsync(int id, CancellationToken ct = default)
    {
        var guia = await _uow.Guias.GetByIdAsync(id, ct);
        return guia is null
            ? Result<GuiaDto>.Failure(Error.NotFound("Guia.NotFound", $"No existe un guía con id '{id}'."))
            : Result<GuiaDto>.Success(guia.ToDto(_idioma.Codigo));
    }

    public async Task<Result<GuiaDto>> GetMiPerfilAsync(CancellationToken ct = default)
    {
        var guia = await _uow.Guias.GetByUsuarioIdAsync(_currentUser.RequireUserId(), ct);
        return guia is null
            ? Result<GuiaDto>.Failure(Error.NotFound("Guia.SinPerfil", "Todavía no has creado tu perfil de guía."))
            : Result<GuiaDto>.Success(guia.ToDto(Idiomas.Espanol));
    }

    public async Task<Result<GuiaDto>> GuardarMiPerfilAsync(GuardarGuiaRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var ahora = DateTimeOffset.UtcNow;

        var guia = await _uow.Guias.GetByUsuarioIdAsync(userId, ct);
        var nuevo = guia is null;
        guia ??= new Guia
        {
            UsuarioId = userId,
            TonoEtiqueta = "blue",
            Verificado = false, // lo verifica un admin; mientras tanto no aparece en la lista pública
            Destacado = false,
            FechaRegistro = ahora,
        };

        var especialidad = request.Specialty.Trim();
        guia.Nombre = request.Name.Trim();
        guia.Especialidad = especialidad;
        guia.Etiqueta = especialidad.Length > 60 ? especialidad[..60] : especialidad;
        guia.Lugar = request.Place.Trim();
        guia.TarifaHora = request.HourlyRate;
        guia.Idiomas = request.Languages
            .Select(l => l.Trim().ToUpperInvariant())
            .Where(l => l.Length > 0)
            .Distinct()
            .ToList();
        guia.ExperienciaAnios = request.ExperienceYears;
        if (!string.IsNullOrWhiteSpace(request.PhotoUrl)) guia.FotoUrl = request.PhotoUrl.Trim();

        var bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        if (bio != guia.Descripcion)
        {
            guia.Descripcion = bio;
            guia.DescripcionI18n = "{}"; // las traducciones viejas ya no corresponden al texto nuevo
        }

        if (nuevo) await _uow.Guias.AddAsync(guia, ct);
        else _uow.Guias.Update(guia);
        await _uow.SaveChangesAsync(ct);

        return Result<GuiaDto>.Success(guia.ToDto(Idiomas.Espanol));
    }
}
