using ColombiaGo.Application.DTOs.Categorias;

namespace ColombiaGo.Application.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaDto>> GetActivasAsync(CancellationToken ct = default);
}
