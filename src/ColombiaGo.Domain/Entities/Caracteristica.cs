using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>A checkbox-style feature a place can offer within its category (e.g. "Wifi gratis",
/// "Apto para niños"), listed via <see cref="LugarCaracteristica"/>.</summary>
public class Caracteristica : Entity<int>
{
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    public ICollection<LugarCaracteristica> LugarCaracteristicas { get; set; } = new List<LugarCaracteristica>();
}
