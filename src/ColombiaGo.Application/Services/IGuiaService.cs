using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface IGuiaService
{
    Task<IReadOnlyList<GuiaDto>> BuscarAsync(string? especialidad, string? ciudad, CancellationToken ct = default);

    Task<Result<GuiaDto>> GetPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Perfil de guía de la persona autenticada; NotFound si todavía no lo ha creado.</summary>
    Task<Result<GuiaDto>> GetMiPerfilAsync(CancellationToken ct = default);

    /// <summary>Crea o actualiza el perfil de guía de la persona autenticada.</summary>
    Task<Result<GuiaDto>> GuardarMiPerfilAsync(GuardarGuiaRequest request, CancellationToken ct = default);
}
