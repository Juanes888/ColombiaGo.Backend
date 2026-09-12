namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Aggregates every repository behind a single scoped instance and commits all pending changes
/// in one transaction via <see cref="SaveChangesAsync"/> — services compose several repositories
/// per use case without each one committing independently.
/// </summary>
public interface IUnitOfWork
{
    ILugarRepository Lugares { get; }
    ICategoriaRepository Categorias { get; }
    IGuiaRepository Guias { get; }
    IFavoritoLugarRepository FavoritosLugares { get; }
    IFavoritoEventoRepository FavoritosEventos { get; }
    IRutaRepository Rutas { get; }
    ICalificacionRepository Calificaciones { get; }
    IEventoRepository Eventos { get; }
    IUsuarioRepository Usuarios { get; }
    IUsuarioConfiguracionRepository Configuraciones { get; }
    ILogroRepository Logros { get; }
    IUsuarioLogroRepository UsuarioLogros { get; }
    IRecomendacionRepository Recomendaciones { get; }
    IReporteGeneradoRepository Reportes { get; }
    IHistorialInteraccionRepository HistorialInteracciones { get; }
    IUsuarioPreferenciaRepository UsuarioPreferencias { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
