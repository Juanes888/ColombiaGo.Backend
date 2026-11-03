namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Convierte una dirección escrita en coordenadas. La usan los negocios al publicar un lugar: escriben la dirección
/// y no tienen que saber latitud y longitud. Implementado en Infrastructure (OpenStreetMap Nominatim, gratis).
/// </summary>
public interface IGeocoder
{
    /// <summary>Coordenadas de la dirección, o null si no se encontró.</summary>
    Task<(decimal Latitud, decimal Longitud)?> BuscarAsync(
        string direccion, string ciudad, string region, CancellationToken ct = default);
}
