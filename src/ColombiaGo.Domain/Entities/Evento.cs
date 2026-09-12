using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Domain.Entities;

/// <summary>An event administered by a user (e.g. an admin or business owner). May reference a
/// <see cref="Lugar"/> as its venue, or stand on its own with a free-text address.</summary>
public class Evento : Entity<int>
{
    public Guid AdminId { get; set; }
    public Usuario Admin { get; set; } = null!;

    public int? LugarId { get; set; }
    public Lugar? Lugar { get; set; }

    public string NombreEvento { get; set; } = string.Empty;
    public DateOnly FechaEvento { get; set; }
    public TimeOnly HoraEvento { get; set; }
    public string DireccionEvento { get; set; } = string.Empty;
    public decimal? Costo { get; set; }
    public string? Descripcion { get; set; }
    public EstadoEvento Estado { get; set; } = EstadoEvento.Programado;

    public DateTimeOffset FechaCreacion { get; set; }

    public ICollection<FavoritoEvento> FavoritosEventos { get; set; } = new List<FavoritoEvento>();
}
