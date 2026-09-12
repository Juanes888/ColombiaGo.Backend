using ColombiaGo.Application.DTOs.Recomendaciones;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class RecomendacionMappings
{
    public static RecomendacionDto ToDto(this Recomendacion recomendacion) =>
        new(recomendacion.Lugar.ToResumenDto(), recomendacion.Score, recomendacion.Motivo);
}
