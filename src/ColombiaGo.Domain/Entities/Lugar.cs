using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// A place (destination) that can be explored, rated, favorited and routed to. <see cref="Slug"/>,
/// <see cref="Ciudad"/>, <see cref="Region"/>, <see cref="Eslogan"/> and <see cref="ComoLlegar"/>
/// are additions beyond the ER: the frontend filters by city, sorts by region, links by slug
/// (<c>/destino/comuna-13</c>) and shows a tagline + transport hint on every card. Putting these
/// in the EAV <see cref="LugarAtributo"/> table would force an EAV join on every paginated
/// listing query — they are real, always-present columns instead. Category-specific, optional
/// attributes still go through <see cref="LugarAtributo"/>.
/// </summary>
public class Lugar : Entity<int>
{
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    /// <summary>Null for system/seed-provided places (there is no human registrant to point
    /// to); set for community-submitted places.</summary>
    public Guid? RegistradoPor { get; set; }
    public Usuario? UsuarioRegistrador { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Eslogan { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string? HorarioAtencion { get; set; }
    public string? ComoLlegar { get; set; }
    public string? ImagenPortadaUrl { get; set; }

    public decimal CalificacionPromedio { get; set; }
    public int TotalCalificaciones { get; set; }

    /// <summary>Curated flag for the home screen's featured carousel. Addition beyond the ER —
    /// the frontend's hero carousel needs a small, editorially-picked set of places.</summary>
    public bool Destacado { get; set; }

    public RangoPrecio RangoPrecio { get; set; }
    public decimal? CostoEntrada { get; set; }

    public EstadoPublicacion EstadoPublicacion { get; set; } = EstadoPublicacion.Pendiente;
    public string? MotivoRechazo { get; set; }

    public DateTimeOffset FechaRegistro { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }

    public ICollection<LugarAtributo> Atributos { get; set; } = new List<LugarAtributo>();
    public ICollection<LugarCaracteristica> Caracteristicas { get; set; } = new List<LugarCaracteristica>();
    public ICollection<GaleriaFoto> Galeria { get; set; } = new List<GaleriaFoto>();
    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
    public ICollection<FavoritoLugar> FavoritosLugares { get; set; } = new List<FavoritoLugar>();
    public ICollection<HistorialInteraccion> HistorialInteracciones { get; set; } = new List<HistorialInteraccion>();
    public ICollection<Recomendacion> Recomendaciones { get; set; } = new List<Recomendacion>();
    public ICollection<RutaLugar> RutaLugares { get; set; } = new List<RutaLugar>();
    public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
