using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Application profile that extends Supabase's <c>auth.users</c> (same Id / primary key — see
/// the ER's "extiende_perfil" relationship). Supabase owns credentials and the row in
/// <c>auth.users</c>; this entity owns everything the app needs about the person.
/// </summary>
public class Usuario : Entity<Guid>
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string? FotoPerfilUrl { get; set; }
    public bool AceptoTerminos { get; set; }
    public DateTimeOffset? FechaAceptacionTerminos { get; set; }
    public bool Activo { get; set; } = true;

    /// <summary>Gamification label shown on the profile screen (e.g. "Viajero Nivel 5").
    /// Addition beyond the ER — the frontend already renders a level/points block.</summary>
    public string Nivel { get; set; } = "Viajero Nivel 1";
    public int Puntos { get; set; }

    public DateTimeOffset FechaRegistro { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }

    public ICollection<UsuarioPreferencia> Preferencias { get; set; } = new List<UsuarioPreferencia>();
    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
    public ICollection<FavoritoLugar> FavoritosLugares { get; set; } = new List<FavoritoLugar>();
    public ICollection<FavoritoEvento> FavoritosEventos { get; set; } = new List<FavoritoEvento>();
    public ICollection<HistorialInteraccion> HistorialInteracciones { get; set; } = new List<HistorialInteraccion>();
    public ICollection<Recomendacion> Recomendaciones { get; set; } = new List<Recomendacion>();
    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
    public ICollection<Evento> EventosAdministrados { get; set; } = new List<Evento>();
    public ICollection<Lugar> LugaresRegistrados { get; set; } = new List<Lugar>();
    public ICollection<ReporteGenerado> ReportesGenerados { get; set; } = new List<ReporteGenerado>();
    public ICollection<UsuarioLogro> Logros { get; set; } = new List<UsuarioLogro>();

    public Guia? Guia { get; set; }
    public UsuarioConfiguracion? Configuracion { get; set; }

    /// <summary>For EF Core materialization only.</summary>
    private Usuario()
    {
    }

    /// <summary>
    /// <paramref name="id"/> must be the matching <c>auth.users.id</c> — this entity's PK is not
    /// database-generated, it is provisioned to mirror Supabase's own user id.
    /// </summary>
    public Usuario(Guid id) : base(id)
    {
    }
}
