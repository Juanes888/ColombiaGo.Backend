using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/guias")]
public class GuiasController : ControllerBase
{
    private readonly IGuiaService _service;
    private readonly IValidator<GuardarGuiaRequest> _validator;

    public GuiasController(IGuiaService service, IValidator<GuardarGuiaRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Backs <c>GuidesPage</c>.</summary>
    [HttpGet]
    [OutputCache(PolicyName = CachingExtensions.CatalogPolicy)]
    public async Task<ActionResult<IReadOnlyList<GuiaDto>>> Buscar(
        [FromQuery] string? especialidad, [FromQuery] string? ciudad, CancellationToken ct) =>
        Ok(await _service.BuscarAsync(especialidad, ciudad, ct));

    [HttpGet("{id:int}")]
    [OutputCache(PolicyName = CachingExtensions.CatalogPolicy)]
    public async Task<ActionResult<GuiaDto>> GetPorId(int id, CancellationToken ct) =>
        this.ToActionResult(await _service.GetPorIdAsync(id, ct));

    /// <summary>Panel del guía: su propio perfil (404 si aún no lo ha creado).</summary>
    [HttpGet("mi-perfil")]
    [Authorize(Policy = "RequireGuia")]
    public async Task<ActionResult<GuiaDto>> MiPerfil(CancellationToken ct) =>
        this.ToActionResult(await _service.GetMiPerfilAsync(ct));

    /// <summary>Crea o actualiza el perfil del guía. Sale en la lista pública cuando un admin lo verifica.</summary>
    [HttpPut("mi-perfil")]
    [Authorize(Policy = "RequireGuia")]
    public async Task<ActionResult<GuiaDto>> GuardarMiPerfil([FromBody] GuardarGuiaRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_validator, request, ct);
        if (validationError is not null) return validationError;

        return this.ToActionResult(await _service.GuardarMiPerfilAsync(request, ct));
    }
}
