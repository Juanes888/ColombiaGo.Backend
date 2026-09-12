using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface IGuiaService
{
    Task<IReadOnlyList<GuiaDto>> BuscarAsync(string? especialidad, string? ciudad, CancellationToken ct = default);

    Task<Result<GuiaDto>> GetPorIdAsync(int id, CancellationToken ct = default);
}
