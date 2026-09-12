using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _perfil;
    private readonly IConfiguracionService _configuracion;

    public PerfilController(IPerfilService perfil, IConfiguracionService configuracion)
    {
        _perfil = perfil;
        _configuracion = configuracion;
    }

    /// <summary>Backs <c>ProfilePage</c>'s <c>USER</c> block + stat card.</summary>
    [HttpGet]
    public async Task<ActionResult<PerfilDto>> Get(CancellationToken ct) =>
        this.ToActionResult(await _perfil.GetPerfilAsync(ct));

    /// <summary>Backs <c>ProfilePage</c>'s "Mis logros" section.</summary>
    [HttpGet("logros")]
    public async Task<ActionResult<IReadOnlyList<LogroDto>>> GetLogros(CancellationToken ct) =>
        Ok(await _perfil.GetLogrosAsync(ct));

    /// <summary>Backs <c>SettingsPage</c> (today persisted only to localStorage).</summary>
    [HttpGet("configuracion")]
    public async Task<ActionResult<ConfiguracionDto>> GetConfiguracion(CancellationToken ct) =>
        Ok(await _configuracion.GetAsync(ct));

    [HttpPut("configuracion")]
    public async Task<ActionResult<ConfiguracionDto>> ActualizarConfiguracion(
        [FromBody] ActualizarConfiguracionRequest request, CancellationToken ct) =>
        Ok(await _configuracion.ActualizarAsync(request, ct));
}
