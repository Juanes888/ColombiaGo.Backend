using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/rutas")]
[Authorize]
public class RutasController : ControllerBase
{
    private readonly IRutaService _service;
    private readonly IRutaIaService _rutaIaService;
    private readonly IValidator<GenerarRutaIaRequest> _rutaIaValidator;

    public RutasController(IRutaService service, IRutaIaService rutaIaService, IValidator<GenerarRutaIaRequest> rutaIaValidator)
    {
        _service = service;
        _rutaIaService = rutaIaService;
        _rutaIaValidator = rutaIaValidator;
    }

    /// <summary>Backs <c>SavedRoutesContext</c> / <c>SavedRoutesPage</c>.</summary>
    [HttpGet("mis-paradas")]
    public async Task<ActionResult<RutaDto>> GetMisParadas(CancellationToken ct) =>
        Ok(await _service.GetMisParadasAsync(ct));

    [HttpPost("mis-paradas/{slug}")]
    public async Task<ActionResult<RutaDto>> AgregarParada(string slug, CancellationToken ct) =>
        this.ToActionResult(await _service.AgregarParadaAsync(slug, ct));

    [HttpDelete("mis-paradas/{slug}")]
    public async Task<ActionResult<RutaDto>> QuitarParada(string slug, CancellationToken ct) =>
        this.ToActionResult(await _service.QuitarParadaAsync(slug, ct));

    /// <summary>Closes the current draft with an optimized stop order.</summary>
    [HttpPost("generar")]
    public async Task<ActionResult<RutaDto>> Generar(CancellationToken ct) =>
        this.ToActionResult(await _service.GenerarAsync(ct));

    /// <summary>Suggests a route (subset + order) for candidate places within a budget, via
    /// Gemini. Purely a suggestion — nothing is persisted; the caller adds the stops it likes
    /// via the existing mis-paradas endpoints.</summary>
    [HttpPost("generar-ia")]
    public async Task<ActionResult<RutaIaDto>> GenerarConIa([FromBody] GenerarRutaIaRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_rutaIaValidator, request, ct);
        if (validationError is not null) return validationError;

        return this.ToActionResult(await _rutaIaService.GenerarConIaAsync(request, ct));
    }
}
