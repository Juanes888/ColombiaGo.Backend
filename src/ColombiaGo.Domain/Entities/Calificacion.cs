using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Domain.Entities;

/// <summary>A user's review of a place. Only <see cref="EstadoModeracion.Aprobada"/> reviews
/// count toward <see cref="Lugar.CalificacionPromedio"/> (enforced by a DB trigger, see
/// db/001_schema.sql, so the average never drifts out of sync with moderation decisions).</summary>
public class Calificacion : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public EstadoModeracion EstadoModeracion { get; set; } = EstadoModeracion.Pendiente;

    public DateTimeOffset Fecha { get; set; }
}
