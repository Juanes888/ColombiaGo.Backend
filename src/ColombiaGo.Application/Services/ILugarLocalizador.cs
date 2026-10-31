using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

/// <summary>
/// Convierte lugares a DTOs en el idioma de la persona. Para es devuelve el contenido base; para en, pt,
/// zh o fr usa la traducción en caché y, si falta, la genera con IA, la guarda y la devuelve. Si la IA
/// no está disponible o falla, cae al español: la app nunca se rompe por una traducción.
/// </summary>
public interface ILugarLocalizador
{
    string Idioma { get; }

    Task<IReadOnlyList<LugarResumenDto>> ResumenAsync(IReadOnlyCollection<Lugar> lugares, CancellationToken ct = default);

    Task<LugarDetalleDto> DetalleAsync(Lugar lugar, bool isFavorite, bool isInRoute, CancellationToken ct = default);

    /// <summary>Traduce y cachea todos los lugares aprobados a <paramref name="idioma"/>. Devuelve cuántos tradujo.</summary>
    Task<int> PrecalentarAsync(string idioma, CancellationToken ct = default);
}
