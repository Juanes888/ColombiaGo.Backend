using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Guias;
using ColombiaGo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/guias")]
public class GuiasController : ControllerBase
{
    private readonly IGuiaService _service;

    public GuiasController(IGuiaService service) => _service = service;

    /// <summary>Backs <c>GuidesPage</c>.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GuiaDto>>> Buscar(
        [FromQuery] string? especialidad, [FromQuery] string? ciudad, CancellationToken ct) =>
        Ok(await _service.BuscarAsync(especialidad, ciudad, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuiaDto>> GetPorId(int id, CancellationToken ct) =>
        this.ToActionResult(await _service.GetPorIdAsync(id, ct));
}
