using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>Reference table of user roles (e.g. turista, guia, admin).</summary>
public class Rol : Entity<int>
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
