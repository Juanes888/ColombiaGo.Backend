namespace ColombiaGo.Application.DTOs.Admin;

/// <summary>Summary shown in the moderation queue — deliberately not the public
/// <c>LugarDetalleDto</c>, which has no room for who submitted it or when.</summary>
public sealed record LugarPendienteDto(
    int Id,
    string Nombre,
    string Slug,
    string Categoria,
    string Ciudad,
    // Null for system/seed-provided places — see Lugar.RegistradoPor.
    string? RegistradoPorNombre,
    string? RegistradoPorEmail,
    DateTimeOffset FechaRegistro,
    string Descripcion,
    string Direccion,
    string? ImagenUrl,
    decimal Latitud,
    decimal Longitud);
