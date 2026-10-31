using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface IPerfilService
{
    Task<Result<PerfilDto>> GetPerfilAsync(CancellationToken ct = default);

    Task<IReadOnlyList<LogroDto>> GetLogrosAsync(CancellationToken ct = default);

    Task<PreferenciasDto> GetPreferenciasAsync(CancellationToken ct = default);

    /// <summary>Reemplaza los intereses de la persona (alimentan las recomendaciones y la guía con IA).</summary>
    Task<Result<PreferenciasDto>> GuardarPreferenciasAsync(GuardarPreferenciasRequest request, CancellationToken ct = default);
}
