using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Categorias;
using ColombiaGo.Application.Mapping;

namespace ColombiaGo.Application.Services;

public sealed class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _uow;

    public CategoriaService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<CategoriaDto>> GetActivasAsync(CancellationToken ct = default)
    {
        var categorias = await _uow.Categorias.GetActivasAsync(ct);
        return categorias.Select(c => c.ToDto()).ToList();
    }
}
