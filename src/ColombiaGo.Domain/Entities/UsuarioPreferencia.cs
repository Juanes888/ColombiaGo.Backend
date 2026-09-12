using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>A category a user marked as a travel interest, used to personalize recommendations.</summary>
public class UsuarioPreferencia : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
}
