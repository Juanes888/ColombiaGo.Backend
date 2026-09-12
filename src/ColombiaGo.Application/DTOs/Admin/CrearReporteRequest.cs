using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.DTOs.Admin;

public sealed record CrearReporteRequest(
    TipoReporte Tipo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    FormatoSalida Formato);
