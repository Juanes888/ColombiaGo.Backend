using System.Globalization;
using System.Text;
using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Negocio;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

/// <summary>
/// Panel del negocio: publica y edita sus propios lugares. Todo lo que se crea o se edita vuelve a quedar
/// <see cref="EstadoPublicacion.Pendiente"/> hasta que un admin lo aprueba, así nada sale al público sin revisión.
/// </summary>
public sealed class NegocioService : INegocioService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IGeocoder _geocoder;

    public NegocioService(IUnitOfWork uow, ICurrentUser currentUser, IGeocoder geocoder)
    {
        _uow = uow;
        _currentUser = currentUser;
        _geocoder = geocoder;
    }

    public async Task<IReadOnlyList<MiLugarDto>> GetMisLugaresAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var lugares = await _uow.Lugares.GetByRegistradorAsync(userId, ct);
        return lugares.Select(ToDto).ToList();
    }

    public async Task<Result<MiLugarDto>> CrearAsync(GuardarLugarRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var categoria = await _uow.Categorias.GetBySlugAsync(request.Category, ct);
        if (categoria is null)
            return Result<MiLugarDto>.Failure(Error.Validation("Lugar.CategoriaInvalida", "La categoría no existe."));

        var coordenadas = await ResolverCoordenadasAsync(request, ct);
        if (coordenadas is null)
            return Result<MiLugarDto>.Failure(DireccionNoEncontrada());

        var ahora = DateTimeOffset.UtcNow;
        var lugar = new Lugar
        {
            CategoriaId = categoria.Id,
            Categoria = categoria,
            RegistradoPor = userId,
            Slug = await GenerarSlugUnicoAsync(request.Name, request.City, ct),
            EstadoPublicacion = EstadoPublicacion.Pendiente,
            FechaRegistro = ahora,
        };
        Aplicar(lugar, request, coordenadas.Value, ahora);

        await _uow.Lugares.AddAsync(lugar, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<MiLugarDto>.Success(ToDto(lugar));
    }

    public async Task<Result<MiLugarDto>> ActualizarAsync(int id, GuardarLugarRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var lugar = await _uow.Lugares.GetByIdAsync(id, ct);
        if (lugar is null)
            return Result<MiLugarDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{id}'."));
        if (lugar.RegistradoPor != userId && !_currentUser.IsInRole("admin"))
            return Result<MiLugarDto>.Failure(Error.Forbidden("Lugar.NoEsTuyo", "Solo puedes editar los lugares que publicaste."));

        var categoria = await _uow.Categorias.GetBySlugAsync(request.Category, ct);
        if (categoria is null)
            return Result<MiLugarDto>.Failure(Error.Validation("Lugar.CategoriaInvalida", "La categoría no existe."));

        // Solo se vuelve a geolocalizar si cambió la dirección (o si el negocio manda coordenadas nuevas).
        var mismaDireccion = string.Equals(lugar.Direccion, request.Address.Trim(), StringComparison.OrdinalIgnoreCase)
                             && string.Equals(lugar.Ciudad, request.City.Trim(), StringComparison.OrdinalIgnoreCase);
        (decimal, decimal)? coordenadas = request.Latitude is not null && request.Longitude is not null
            ? (request.Latitude.Value, request.Longitude.Value)
            : mismaDireccion ? (lugar.Latitud, lugar.Longitud) : await ResolverCoordenadasAsync(request, ct);
        if (coordenadas is null)
            return Result<MiLugarDto>.Failure(DireccionNoEncontrada());

        lugar.CategoriaId = categoria.Id;
        lugar.Categoria = categoria;
        Aplicar(lugar, request, coordenadas.Value, DateTimeOffset.UtcNow);
        lugar.EstadoPublicacion = EstadoPublicacion.Pendiente; // cualquier cambio vuelve a revisión
        lugar.MotivoRechazo = null;

        _uow.Lugares.Update(lugar);
        await _uow.SaveChangesAsync(ct);

        return Result<MiLugarDto>.Success(ToDto(lugar));
    }

    private async Task<(decimal, decimal)?> ResolverCoordenadasAsync(GuardarLugarRequest request, CancellationToken ct)
    {
        if (request.Latitude is not null && request.Longitude is not null)
            return (request.Latitude.Value, request.Longitude.Value);

        return await _geocoder.BuscarAsync(request.Address.Trim(), request.City.Trim(), request.Region.Trim(), ct);
    }

    private static Error DireccionNoEncontrada() => Error.Validation(
        "Lugar.DireccionNoEncontrada",
        "No encontramos esa dirección en el mapa. Revisa la dirección y la ciudad.");

    private static void Aplicar(Lugar lugar, GuardarLugarRequest r, (decimal Lat, decimal Lng) coords, DateTimeOffset ahora)
    {
        lugar.Nombre = r.Name.Trim();
        lugar.Ciudad = r.City.Trim();
        lugar.Region = r.Region.Trim();
        lugar.Direccion = r.Address.Trim();
        lugar.Descripcion = r.Description.Trim();
        lugar.Eslogan = Limpio(r.Tagline);
        lugar.HorarioAtencion = Limpio(r.Hours);
        lugar.ComoLlegar = Limpio(r.HowToGet);
        lugar.RangoPrecio = r.PriceRange;
        lugar.CostoEntrada = r.EntryCost;
        lugar.ImagenPortadaUrl = Limpio(r.ImageUrl);
        lugar.Latitud = Math.Round(coords.Lat, 6);
        lugar.Longitud = Math.Round(coords.Lng, 6);
        lugar.FechaActualizacion = ahora;
    }

    private static string? Limpio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private async Task<string> GenerarSlugUnicoAsync(string nombre, string ciudad, CancellationToken ct)
    {
        var baseSlug = Slugify($"{nombre} {ciudad}");
        if (baseSlug.Length == 0) baseSlug = "lugar";
        if (baseSlug.Length > 200) baseSlug = baseSlug[..200].TrimEnd('-');

        var slug = baseSlug;
        for (var n = 2; await _uow.Lugares.ExistsAsync(l => l.Slug == slug, ct); n++)
            slug = $"{baseSlug}-{n}";
        return slug;
    }

    /// <summary>"Café Pergamino, Medellín" → "cafe-pergamino-medellin".</summary>
    public static string Slugify(string texto)
    {
        var sinTildes = new StringBuilder();
        foreach (var c in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sinTildes.Append(c);
        }

        var resultado = new StringBuilder();
        var guion = false;
        foreach (var c in sinTildes.ToString().ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                resultado.Append(c);
                guion = false;
            }
            else if (!guion && resultado.Length > 0)
            {
                resultado.Append('-');
                guion = true;
            }
        }

        return resultado.ToString().Trim('-');
    }

    private static MiLugarDto ToDto(Lugar l) => new(
        Id: l.Id,
        Slug: l.Slug,
        Name: l.Nombre,
        Category: l.Categoria?.Slug ?? string.Empty,
        City: l.Ciudad,
        Region: l.Region,
        Address: l.Direccion,
        Description: l.Descripcion,
        Tagline: l.Eslogan,
        Hours: l.HorarioAtencion,
        HowToGet: l.ComoLlegar,
        PriceRange: l.RangoPrecio.ToString().ToLowerInvariant(),
        EntryCost: l.CostoEntrada,
        Image: l.ImagenPortadaUrl,
        Latitude: l.Latitud,
        Longitude: l.Longitud,
        Status: l.EstadoPublicacion.ToString().ToLowerInvariant(),
        RejectReason: l.MotivoRechazo,
        Rating: l.CalificacionPromedio,
        ReviewsCount: l.TotalCalificaciones,
        UpdatedAt: l.FechaActualizacion);
}
