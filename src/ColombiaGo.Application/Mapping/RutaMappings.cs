using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class RutaMappings
{
    public static RutaDto ToDto(this Ruta ruta)
    {
        var paradas = ruta.Paradas
            .OrderBy(p => p.OrdenOptimo)
            .Select(p => p.Lugar.ToResumenDto())
            .ToList();

        return new RutaDto(
            Id: ruta.Id,
            Generated: ruta.FechaGeneracion is not null,
            FechaGeneracion: ruta.FechaGeneracion,
            TiempoEstimadoMin: ruta.TiempoEstimadoMin,
            Paradas: paradas);
    }
}
