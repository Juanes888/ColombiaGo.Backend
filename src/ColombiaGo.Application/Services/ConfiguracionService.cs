using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

public sealed class ConfiguracionService : IConfiguracionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public ConfiguracionService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ConfiguracionDto> GetAsync(CancellationToken ct = default)
    {
        var config = await GetOrCreateAsync(ct);
        return config.ToDto();
    }

    public async Task<ConfiguracionDto> ActualizarAsync(ActualizarConfiguracionRequest request, CancellationToken ct = default)
    {
        var config = await GetOrCreateAsync(ct);

        config.Notificaciones = request.Notifications;
        config.Ubicacion = request.Location;
        config.ReduceMotion = request.ReduceMotion;
        config.FechaActualizacion = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(ct);
        return config.ToDto();
    }

    private async Task<UsuarioConfiguracion> GetOrCreateAsync(CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var config = await _uow.Configuraciones.GetByIdAsync(userId, ct);
        if (config is not null)
            return config;

        config = new UsuarioConfiguracion(userId)
        {
            Notificaciones = true,
            Ubicacion = true,
            ReduceMotion = false,
            FechaActualizacion = DateTimeOffset.UtcNow,
        };
        await _uow.Configuraciones.AddAsync(config, ct);
        await _uow.SaveChangesAsync(ct);
        return config;
    }
}
