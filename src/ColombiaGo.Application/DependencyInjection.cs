using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ColombiaGo.Application;

/// <summary>Wires every Application-layer service and FluentValidation validator into the
/// container. Called once from the API's <c>Program.cs</c>.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddScoped<ILugarService, LugarService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IFavoritoService, FavoritoService>();
        services.AddScoped<IRutaService, RutaService>();
        services.AddScoped<IRutaIaService, RutaIaService>();
        services.AddScoped<IGuiaViajeIaService, GuiaViajeIaService>();
        services.AddScoped<ILugarLocalizador, LugarLocalizador>();
        services.AddScoped<ICalificacionService, CalificacionService>();
        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<IGuiaService, GuiaService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IConfiguracionService, ConfiguracionService>();
        services.AddScoped<IRecomendacionService, RecomendacionService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
