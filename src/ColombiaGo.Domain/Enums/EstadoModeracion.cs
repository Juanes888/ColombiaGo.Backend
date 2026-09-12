namespace ColombiaGo.Domain.Enums;

/// <summary>Moderation state of a <c>calificacion</c> (review) before it counts toward the
/// place's average rating.</summary>
public enum EstadoModeracion
{
    Pendiente = 0,
    Aprobada = 1,
    Rechazada = 2,
}
