using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ColombiaGo.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the whole schema. Table/column naming is snake_case, applied globally by
/// <c>EFCore.NamingConventions</c> (wired in <c>DependencyInjection.cs</c>) rather than repeated
/// per property in every <see cref="Configurations"/> class — relationships, indexes, enum
/// conversions and precision are what actually vary per entity, so that's what those classes
/// focus on.
/// </summary>
public class ColombiaGoDbContext : DbContext
{
    public ColombiaGoDbContext(DbContextOptions<ColombiaGoDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<UsuarioPreferencia> UsuarioPreferencias => Set<UsuarioPreferencia>();
    public DbSet<Lugar> Lugares => Set<Lugar>();
    public DbSet<TipoAtributo> TiposAtributo => Set<TipoAtributo>();
    public DbSet<LugarAtributo> LugarAtributos => Set<LugarAtributo>();
    public DbSet<Caracteristica> Caracteristicas => Set<Caracteristica>();
    public DbSet<LugarCaracteristica> LugarCaracteristicas => Set<LugarCaracteristica>();
    public DbSet<GaleriaFoto> GaleriaFotos => Set<GaleriaFoto>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();
    public DbSet<FavoritoLugar> FavoritosLugares => Set<FavoritoLugar>();
    public DbSet<FavoritoEvento> FavoritosEventos => Set<FavoritoEvento>();
    public DbSet<HistorialInteraccion> HistorialInteracciones => Set<HistorialInteraccion>();
    public DbSet<Recomendacion> Recomendaciones => Set<Recomendacion>();
    public DbSet<Ruta> Rutas => Set<Ruta>();
    public DbSet<RutaLugar> RutaLugares => Set<RutaLugar>();
    public DbSet<ReporteGenerado> ReportesGenerados => Set<ReporteGenerado>();
    public DbSet<Guia> Guias => Set<Guia>();
    public DbSet<Logro> Logros => Set<Logro>();
    public DbSet<UsuarioLogro> UsuarioLogros => Set<UsuarioLogro>();
    public DbSet<LugarTraduccion> LugarTraducciones => Set<LugarTraduccion>();
    public DbSet<GuiaViaje> GuiasViaje => Set<GuiaViaje>();
    public DbSet<UsuarioConfiguracion> UsuarioConfiguraciones => Set<UsuarioConfiguracion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Native PostgreSQL enum types (see db/001_schema.sql for the matching CREATE TYPE
        // statements). Npgsql translates each C# member name to snake_case for the on-the-wire
        // label (e.g. RangoPrecio.Gratis -> 'gratis'), which the SQL script's labels must match.
        modelBuilder.HasPostgresEnum<RangoPrecio>();
        modelBuilder.HasPostgresEnum<EstadoPublicacion>();
        modelBuilder.HasPostgresEnum<EstadoModeracion>();
        modelBuilder.HasPostgresEnum<EstadoEvento>();
        modelBuilder.HasPostgresEnum<TipoInteraccion>();
        modelBuilder.HasPostgresEnum<TipoDato>();
        modelBuilder.HasPostgresEnum<TipoReporte>();
        modelBuilder.HasPostgresEnum<FormatoSalida>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ColombiaGoDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
