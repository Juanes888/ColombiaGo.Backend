using ColombiaGo.Application.DTOs.Lugares;

namespace ColombiaGo.Application.DTOs.Recomendaciones;

public sealed record RecomendacionDto(LugarResumenDto Lugar, decimal Score, string? Motivo);
