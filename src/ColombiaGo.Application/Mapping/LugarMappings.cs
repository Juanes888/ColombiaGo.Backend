using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

/// <summary>
/// Explicit (no-reflection) mapping between <see cref="Lugar"/> and its DTOs. Kept as plain
/// extension methods rather than AutoMapper: every field is visible at the call site, there is
/// no runtime profile to misconfigure, and it's trivially unit-testable. When a cached
/// <see cref="LugarTraduccion"/> is supplied its text replaces the Spanish base text.
/// </summary>
public static class LugarMappings
{
    public static LugarResumenDto ToResumenDto(
        this Lugar lugar, string idioma = Idiomas.Espanol, LugarTraduccion? traduccion = null) => new(
        Id: lugar.Slug,
        Name: traduccion?.Nombre ?? lugar.Nombre,
        Region: lugar.Region,
        City: lugar.Ciudad,
        Rating: lugar.CalificacionPromedio,
        ReviewsCount: lugar.TotalCalificaciones,
        ReviewsLabel: DisplayFormatting.ToReviewsLabel(lugar.TotalCalificaciones, idioma),
        Category: lugar.Categoria.Slug,
        CategoryLabel: lugar.Categoria.Nombre,
        Image: lugar.ImagenPortadaUrl,
        Tagline: traduccion?.Eslogan ?? lugar.Eslogan,
        Latitude: lugar.Latitud,
        Longitude: lugar.Longitud);

    public static LugarDetalleDto ToDetalleDto(
        this Lugar lugar, bool isFavorite, bool isInRoute,
        string idioma = Idiomas.Espanol, LugarTraduccion? traduccion = null)
    {
        var atributos = lugar.Atributos
            .Select(a => new LugarAtributoDto(a.TipoAtributo.Nombre, a.TipoAtributo.Etiqueta, a.Valor))
            .ToList();

        var features = lugar.Caracteristicas
            .Select(lc => lc.Caracteristica.Nombre)
            .ToList();

        var gallery = lugar.Galeria
            .OrderBy(g => g.Orden)
            .Select(g => g.UrlImagen)
            .ToList();

        return new LugarDetalleDto(
            Id: lugar.Slug,
            Name: traduccion?.Nombre ?? lugar.Nombre,
            Region: lugar.Region,
            City: lugar.Ciudad,
            Rating: lugar.CalificacionPromedio,
            ReviewsCount: lugar.TotalCalificaciones,
            ReviewsLabel: DisplayFormatting.ToReviewsLabel(lugar.TotalCalificaciones, idioma),
            Category: lugar.Categoria.Slug,
            CategoryLabel: lugar.Categoria.Nombre,
            Image: lugar.ImagenPortadaUrl,
            Tagline: traduccion?.Eslogan ?? lugar.Eslogan,
            Description: traduccion?.Descripcion ?? lugar.Descripcion,
            Hours: traduccion?.HorarioAtencion ?? lugar.HorarioAtencion,
            Price: DisplayFormatting.ToPriceLabel(lugar.RangoPrecio, lugar.CostoEntrada, idioma),
            Transport: traduccion?.ComoLlegar ?? lugar.ComoLlegar,
            Address: lugar.Direccion,
            Latitude: lugar.Latitud,
            Longitude: lugar.Longitud,
            Gallery: gallery,
            Features: features,
            Attributes: atributos,
            IsFavorite: isFavorite,
            IsInRoute: isInRoute);
    }
}
