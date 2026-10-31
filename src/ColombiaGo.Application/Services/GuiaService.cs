using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public sealed class GuiaService : IGuiaService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdiomaActual _idioma;

    public GuiaService(IUnitOfWork uow, IIdiomaActual idioma)
    {
        _uow = uow;
        _idioma = idioma;
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
}
