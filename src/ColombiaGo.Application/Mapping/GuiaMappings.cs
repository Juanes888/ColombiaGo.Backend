using System.Text.Json;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class GuiaMappings
{
    public static GuiaDto ToDto(this Guia guia, string idioma = Idiomas.Espanol) => new(
        Id: guia.Id,
        Name: guia.Nombre,
        Specialty: guia.Especialidad,
        Tag: guia.Etiqueta,
        TagTone: guia.TonoEtiqueta,
        Place: guia.Lugar,
        Rating: guia.CalificacionPromedio,
        Reviews: guia.TotalCalificaciones,
        Price: DisplayFormatting.ToHourlyPriceLabel(guia.TarifaHora, idioma),
        Languages: guia.Idiomas,
        Image: guia.FotoUrl,
        Featured: guia.Destacado,
        Badge: guia.Insignia,
        Verified: guia.Verificado,
        Bio: Bio(guia, idioma),
        ExperienceYears: guia.ExperienciaAnios,
        ToursCount: guia.ToursRealizados);

    private static string? Bio(Guia guia, string idioma)
    {
        if (idioma != Idiomas.Espanol && !string.IsNullOrWhiteSpace(guia.DescripcionI18n))
        {
            try
            {
                var traducciones = JsonSerializer.Deserialize<Dictionary<string, string>>(guia.DescripcionI18n);
                if (traducciones is not null && traducciones.TryGetValue(idioma, out var texto) && !string.IsNullOrWhiteSpace(texto))
                    return texto;
            }
            catch (JsonException)
            {
                // JSON mal formado en la base: se usa el texto en español.
            }
        }

        return guia.Descripcion;
    }
}
