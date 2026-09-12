using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class GuiaMappings
{
    public static GuiaDto ToDto(this Guia guia) => new(
        Id: guia.Id,
        Name: guia.Nombre,
        Specialty: guia.Especialidad,
        Tag: guia.Etiqueta,
        TagTone: guia.TonoEtiqueta,
        Place: guia.Lugar,
        Rating: guia.CalificacionPromedio,
        Reviews: guia.TotalCalificaciones,
        Price: DisplayFormatting.ToHourlyPriceLabel(guia.TarifaHora),
        Languages: guia.Idiomas,
        Image: guia.FotoUrl,
        Featured: guia.Destacado,
        Badge: guia.Insignia);
}
