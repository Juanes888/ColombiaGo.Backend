namespace ColombiaGo.Application.DTOs.Rutas;

/// <summary>Candidate places the user is considering plus their budget, for the AI-assisted
/// route planner (<c>POST /api/v1/rutas/generar-ia</c>).</summary>
public sealed record GenerarRutaIaRequest(
    IReadOnlyList<string> LugarSlugs,
    decimal Presupuesto);
