using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// A trip itinerary composed of an ordered set of stops (<see cref="RutaLugar"/>). A user
/// accumulates stops into a single open "draft" route (<see cref="FechaGeneracion"/> is
/// <c>null</c>) by bookmarking places — mirroring the frontend's <c>SavedRoutesContext</c>, which
/// today just tracks a flat set of ids. Calling "generar" computes an optimized visiting order
/// and stamps <see cref="FechaGeneracion"/>, closing that route; a new draft starts on the next
/// bookmark. <see cref="FechaGeneracion"/> being nullable is a small, deliberate refinement of
/// the ER (which shows it as always-set) needed to represent the draft state at all.
/// </summary>
public class Ruta : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int? TiempoEstimadoMin { get; set; }
    public DateTimeOffset? FechaGeneracion { get; set; }

    public ICollection<RutaLugar> Paradas { get; set; } = new List<RutaLugar>();
}
