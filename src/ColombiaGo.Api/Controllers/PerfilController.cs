using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Application.Services;
using FluentValidation;
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
    private readonly IValidator<ActualizarConfiguracionRequest> _configuracionValidator;
    private readonly IValidator<GuardarPreferenciasRequest> _preferenciasValidator;

    public PerfilController(
        IPerfilService perfil,
        IConfiguracionService configuracion,
        IValidator<ActualizarConfiguracionRequest> configuracionValidator,
        IValidator<GuardarPreferenciasRequest> preferenciasValidator)
    {
        _perfil = perfil;
        _configuracion = configuracion;
        _configuracionValidator = configuracionValidator;
        _preferenciasValidator = preferenciasValidator;
    }

    /// <summary>Backs the profile screen's user block + stat card.</summary>
    [HttpGet]
    public async Task<ActionResult<PerfilDto>> Get(CancellationToken ct) =>
        this.ToActionResult(await _perfil.GetPerfilAsync(ct));

    /// <summary>Backs the profile screen's "Mis logros" section.</summary>
    [HttpGet("logros")]
    public async Task<ActionResult<IReadOnlyList<LogroDto>>> GetLogros(CancellationToken ct) =>
        Ok(await _perfil.GetLogrosAsync(ct));

    /// <summary>Notifications, location, reduced motion, app language and onboarding state.</summary>
    [HttpGet("configuracion")]
    public async Task<ActionResult<ConfiguracionDto>> GetConfiguracion(CancellationToken ct) =>
        Ok(await _configuracion.GetAsync(ct));

    [HttpPut("configuracion")]
    public async Task<ActionResult<ConfiguracionDto>> ActualizarConfiguracion(
        [FromBody] ActualizarConfiguracionRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_configuracionValidator, request, ct);
        if (validationError is not null) return validationError;

        return Ok(await _configuracion.ActualizarAsync(request, ct));
    }

    /// <summary>Marks the welcome tour as finished (or skipped) so it is not shown again.</summary>
    [HttpPost("onboarding/completar")]
    public async Task<ActionResult<ConfiguracionDto>> CompletarOnboarding(CancellationToken ct) =>
        Ok(await _configuracion.CompletarOnboardingAsync(ct));

    /// <summary>Interest categories picked during onboarding (feed recommendations and the AI guide).</summary>
    [HttpGet("preferencias")]
    public async Task<ActionResult<PreferenciasDto>> GetPreferencias(CancellationToken ct) =>
        Ok(await _perfil.GetPreferenciasAsync(ct));

    [HttpPut("preferencias")]
    public async Task<ActionResult<PreferenciasDto>> GuardarPreferencias(
        [FromBody] GuardarPreferenciasRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_preferenciasValidator, request, ct);
        if (validationError is not null) return validationError;

        return this.ToActionResult(await _perfil.GuardarPreferenciasAsync(request, ct));
    }
}
