using ColombiaGo.Application.Abstractions;
using ColombiaGo.Infrastructure.Persistence;

namespace ColombiaGo.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ColombiaGoDbContext _context;

    public UnitOfWork(ColombiaGoDbContext context)
    {
        _context = context;

        Lugares = new LugarRepository(context);
        Categorias = new CategoriaRepository(context);
        Guias = new GuiaRepository(context);
        FavoritosLugares = new FavoritoLugarRepository(context);
        FavoritosEventos = new FavoritoEventoRepository(context);
        Rutas = new RutaRepository(context);
        Calificaciones = new CalificacionRepository(context);
        Eventos = new EventoRepository(context);
        Usuarios = new UsuarioRepository(context);
        Configuraciones = new UsuarioConfiguracionRepository(context);
        Logros = new LogroRepository(context);
        UsuarioLogros = new UsuarioLogroRepository(context);
        Recomendaciones = new RecomendacionRepository(context);
        Reportes = new ReporteGeneradoRepository(context);
        HistorialInteracciones = new HistorialInteraccionRepository(context);
        UsuarioPreferencias = new UsuarioPreferenciaRepository(context);
        LugarTraducciones = new LugarTraduccionRepository(context);
        GuiasViaje = new GuiaViajeRepository(context);
    }

    public ILugarRepository Lugares { get; }
    public ICategoriaRepository Categorias { get; }
    public IGuiaRepository Guias { get; }
    public IFavoritoLugarRepository FavoritosLugares { get; }
    public IFavoritoEventoRepository FavoritosEventos { get; }
    public IRutaRepository Rutas { get; }
    public ICalificacionRepository Calificaciones { get; }
    public IEventoRepository Eventos { get; }
    public IUsuarioRepository Usuarios { get; }
    public IUsuarioConfiguracionRepository Configuraciones { get; }
    public ILogroRepository Logros { get; }
    public IUsuarioLogroRepository UsuarioLogros { get; }
    public IRecomendacionRepository Recomendaciones { get; }
    public IReporteGeneradoRepository Reportes { get; }
    public IHistorialInteraccionRepository HistorialInteracciones { get; }
    public IUsuarioPreferenciaRepository UsuarioPreferencias { get; }
    public ILugarTraduccionRepository LugarTraducciones { get; }
    public IGuiaViajeRepository GuiasViaje { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
