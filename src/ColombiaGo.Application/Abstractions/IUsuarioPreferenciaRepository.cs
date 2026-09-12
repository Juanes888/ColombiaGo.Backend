namespace ColombiaGo.Application.Abstractions;

public interface IUsuarioPreferenciaRepository
{
    Task<IReadOnlyList<int>> GetCategoriaIdsByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}
