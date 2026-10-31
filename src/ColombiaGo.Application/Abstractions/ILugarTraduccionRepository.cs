using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface ILugarTraduccionRepository : IRepository<LugarTraduccion, int>
{
    Task<IReadOnlyList<LugarTraduccion>> GetAsync(
        IReadOnlyCollection<int> lugarIds, string idioma, CancellationToken ct = default);

    /// <summary>Guarda traducciones recién generadas. Tolera que otra petición haya guardado la misma
    /// (lugar, idioma) un instante antes: en ese caso simplemente descarta las duplicadas.</summary>
    Task GuardarAsync(IReadOnlyCollection<LugarTraduccion> traducciones, CancellationToken ct = default);
}
