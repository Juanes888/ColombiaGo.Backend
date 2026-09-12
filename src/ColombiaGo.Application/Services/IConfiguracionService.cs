using ColombiaGo.Application.DTOs.Perfil;

namespace ColombiaGo.Application.Services;

public interface IConfiguracionService
{
    Task<ConfiguracionDto> GetAsync(CancellationToken ct = default);

    Task<ConfiguracionDto> ActualizarAsync(ActualizarConfiguracionRequest request, CancellationToken ct = default);
}
