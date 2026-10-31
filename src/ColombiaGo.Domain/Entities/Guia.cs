using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Tour-guide directory profile. Not present in the ER — added because the frontend's "Guías"
/// screen already lists guides with specialty, rate, languages and rating. <see cref="UsuarioId"/>
/// is optional rather than a required 1:1 link to <see cref="Usuario"/>: a guide starts as a
/// directory entry (which is how the seeded demo guides — Mateo, Sofía, Andrés, Lucía — exist at
/// all, since they have no real Supabase-authenticated account) and can later be linked to a real
/// user account so that person can self-manage their profile. <see cref="Nombre"/>/<see cref="FotoUrl"/>
/// are therefore this entity's own display fields, not read through <see cref="Usuario"/>.
/// </summary>
public class Guia : Entity<int>
{
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }

    public string Especialidad { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string TonoEtiqueta { get; set; } = string.Empty;
    public string? Insignia { get; set; }

    /// <summary>Free-text place label as shown on the guide card (e.g. "Salento, Quindío").</summary>
    public string Lugar { get; set; } = string.Empty;

    public decimal TarifaHora { get; set; }
    public List<string> Idiomas { get; set; } = new();
    public bool Destacado { get; set; }
    public bool Verificado { get; set; }

    /// <summary>Presentación del guía en español (idioma base).</summary>
    public string? Descripcion { get; set; }

    /// <summary>Presentación en los demás idiomas, como JSON <c>{"en": "...", "pt": "...", "fr": "...", "zh": "..."}</c>.</summary>
    public string DescripcionI18n { get; set; } = "{}";

    public int ExperienciaAnios { get; set; }
    public int ToursRealizados { get; set; }

    public decimal CalificacionPromedio { get; set; }
    public int TotalCalificaciones { get; set; }

    public DateTimeOffset FechaRegistro { get; set; }
}
