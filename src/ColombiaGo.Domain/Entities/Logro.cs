using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Achievement definition (badge) a user can unlock — powers the "Mis logros" section of the
/// profile screen. Not present in the original ER diagram; added because the frontend already
/// renders a hardcoded achievements list that needs a real, per-user source of truth.
/// </summary>
public class Logro : Entity<int>
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string Tono { get; set; } = string.Empty;
    public string? Criterio { get; set; }

    public ICollection<UsuarioLogro> UsuarioLogros { get; set; } = new List<UsuarioLogro>();
}
