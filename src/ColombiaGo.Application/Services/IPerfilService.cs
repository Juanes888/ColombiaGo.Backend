using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface IPerfilService
{
    Task<Result<PerfilDto>> GetPerfilAsync(CancellationToken ct = default);

    Task<IReadOnlyList<LogroDto>> GetLogrosAsync(CancellationToken ct = default);
}
