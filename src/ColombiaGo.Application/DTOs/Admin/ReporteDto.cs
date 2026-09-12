using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.DTOs.Admin;

public sealed record ReporteDto(
    int Id,
    TipoReporte Tipo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    FormatoSalida Formato,
    string? ArchivoUrl,
    DateTimeOffset FechaGeneracion);
