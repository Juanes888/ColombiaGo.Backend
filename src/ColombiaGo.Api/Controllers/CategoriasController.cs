using ColombiaGo.Application.DTOs.Categorias;
using ColombiaGo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _service;

    public CategoriasController(ICategoriaService service) => _service = service;

    /// <summary>Backs the Home screen's category chips and the Explore screen's filter row.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Get(CancellationToken ct) =>
        Ok(await _service.GetActivasAsync(ct));
}
