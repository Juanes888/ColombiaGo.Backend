using ColombiaGo.Application.DTOs.Admin;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class ReporteMappings
{
    public static ReporteDto ToDto(this ReporteGenerado reporte) => new(
        reporte.Id, reporte.TipoReporte, reporte.FechaInicio, reporte.FechaFin,
        reporte.FormatoSalida, reporte.ArchivoUrl, reporte.FechaGeneracion);
}
