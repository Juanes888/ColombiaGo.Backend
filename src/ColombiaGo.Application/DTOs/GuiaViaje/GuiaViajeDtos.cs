using ColombiaGo.Application.DTOs.Lugares;

namespace ColombiaGo.Application.DTOs.GuiaViaje;

/// <summary>Lo que la persona ingresa para que la IA le arme la guía completa.</summary>
public sealed record GenerarGuiaViajeRequest(
    string Ciudad,
    int Dias,
    decimal Presupuesto,
    IReadOnlyList<string>? Intereses);

public sealed record GuiaViajeParadaDto(
    LugarResumenDto Lugar,
    string Hora,
    int DuracionMin,
    decimal CostoAprox,
    string Consejo);

public sealed record GuiaViajeDiaDto(
    int Dia,
    string Tema,
    IReadOnlyList<GuiaViajeParadaDto> Paradas);

/// <summary>Guía completa lista para mostrar: resumen, plan día por día, costos y consejos.</summary>
public sealed record GuiaViajeDto(
    int Id,
    string Ciudad,
    int Dias,
    decimal Presupuesto,
    IReadOnlyList<string> Intereses,
    string Idioma,
    string Titulo,
    string Resumen,
    IReadOnlyList<GuiaViajeDiaDto> PlanDiario,
    decimal CostoTotalEstimado,
    decimal PresupuestoRestante,
    bool ExcedePresupuesto,
    IReadOnlyList<string> ConsejosGenerales,
    IReadOnlyList<string> QueLlevar,
    DateTimeOffset FechaCreacion);

/// <summary>Resumen para la lista "Mis guías".</summary>
public sealed record GuiaViajeResumenDto(
    int Id,
    string Titulo,
    string Ciudad,
    int Dias,
    decimal Presupuesto,
    DateTimeOffset FechaCreacion);

/// <summary>Ciudad con lugares disponibles — alimenta el selector de ciudad de la guía.</summary>
public sealed record CiudadDto(string Nombre, int TotalLugares);
