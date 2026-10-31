namespace ColombiaGo.Application.Abstractions;

public interface IUsuarioPreferenciaRepository
{
    Task<IReadOnlyList<int>> GetCategoriaIdsByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Reemplaza por completo las categorías de interés de la persona (queda en el contexto;
    /// confirma con <c>IUnitOfWork.SaveChangesAsync</c>).</summary>
    Task ReemplazarAsync(Guid usuarioId, IReadOnlyCollection<int> categoriaIds, CancellationToken ct = default);
}
