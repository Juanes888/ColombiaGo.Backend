using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Per-user app preferences (notifications, location, reduced motion). Not present in the ER —
/// the frontend's Settings screen already persists these, today only in localStorage, which is
/// lost across devices; this gives them a durable, server-side home. Shares its primary key with
/// <see cref="Usuario"/> (one settings row per user, created on first access) — hence the
/// explicit constructor: unlike the auto-increment entities, this Id is never assigned by the
/// database, so it must be set at creation time.
/// </summary>
public class UsuarioConfiguracion : Entity<Guid>
{
    public Usuario Usuario { get; set; } = null!;

    public bool Notificaciones { get; set; } = true;
    public bool Ubicacion { get; set; } = true;
    public bool ReduceMotion { get; set; }

    /// <summary>Idioma de la app elegido por la persona: es, en, pt, zh o fr.</summary>
    public string Idioma { get; set; } = "es";

    /// <summary>False hasta que termina (o salta) el recorrido de bienvenida.</summary>
    public bool OnboardingCompletado { get; set; }

    public DateTimeOffset FechaActualizacion { get; set; }

    /// <summary>For EF Core materialization only.</summary>
    private UsuarioConfiguracion()
    {
    }

    public UsuarioConfiguracion(Guid usuarioId) : base(usuarioId)
    {
    }
}
