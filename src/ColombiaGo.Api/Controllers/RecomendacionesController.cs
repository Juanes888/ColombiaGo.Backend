using ColombiaGo.Application.DTOs.Recomendaciones;
using ColombiaGo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/recomendaciones")]
[Authorize]
public class RecomendacionesController : ControllerBase
{
    private readonly IRecomendacionService _service;

    public RecomendacionesController(IRecomendacionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecomendacionDto>>> Get(
        [FromQuery] int take = 10, CancellationToken ct = default) =>
        Ok(await _service.GetRecomendacionesAsync(take, ct));
}
