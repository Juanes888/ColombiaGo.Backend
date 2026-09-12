using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Rutas;
using ColombiaGo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/rutas")]
[Authorize]
public class RutasController : ControllerBase
{
    private readonly IRutaService _service;

    public RutasController(IRutaService service) => _service = service;

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
}
