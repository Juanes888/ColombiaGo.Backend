using ColombiaGo.Application.DTOs.GuiaViaje;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

/// <summary>
/// Arma una guía de viaje completa día por día con IA a partir de ciudad, días, presupuesto e intereses,
/// usando únicamente lugares reales de la base de datos.
/// </summary>
public interface IGuiaViajeIaService
{
    Task<Result<GuiaViajeDto>> GenerarAsync(GenerarGuiaViajeRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<GuiaViajeResumenDto>> ListarAsync(CancellationToken ct = default);

    Task<Result<GuiaViajeDto>> ObtenerAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<CiudadDto>> CiudadesAsync(CancellationToken ct = default);
}
