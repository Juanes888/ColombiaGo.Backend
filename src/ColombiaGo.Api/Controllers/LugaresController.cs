using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Calificaciones;
using ColombiaGo.Application.Services;
using ColombiaGo.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/lugares")]
public class LugaresController : ControllerBase
{
    private readonly ILugarService _lugares;
    private readonly ICalificacionService _calificaciones;
    private readonly IValidator<CrearCalificacionRequest> _calificacionValidator;

    public LugaresController(
        ILugarService lugares, ICalificacionService calificaciones, IValidator<CrearCalificacionRequest> calificacionValidator)
    {
        _lugares = lugares;
        _calificaciones = calificaciones;
        _calificacionValidator = calificacionValidator;
    }

    /// <summary>Backs the Explore screen's search + category chips + city deep link.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Application.DTOs.Lugares.LugarResumenDto>>> Buscar(
        [FromQuery] string? q,
        [FromQuery] string? categoria,
        [FromQuery] string? ciudad,
        [FromQuery] string? region,
        [FromQuery] RangoPrecio? rangoPrecio,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new LugarQueryParams
        {
            Q = q,
            CategoriaSlug = categoria,
            Ciudad = ciudad,
            Region = region,
            RangoPrecio = rangoPrecio,
            Page = page,
            PageSize = pageSize,
        };

        return Ok(await _lugares.BuscarAsync(query, ct));
    }

    /// <summary>Backs the Home screen's featured carousel.</summary>
    [HttpGet("destacados")]
    public async Task<ActionResult<IReadOnlyList<Application.DTOs.Lugares.LugarResumenDto>>> Destacados(
        [FromQuery] int take = 5, CancellationToken ct = default) =>
        Ok(await _lugares.GetDestacadosAsync(take, ct));

    /// <summary>Backs the destination detail screen (<c>/destino/:id</c>, id = slug).</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<Application.DTOs.Lugares.LugarDetalleDto>> GetPorSlug(
        string slug, CancellationToken ct)
    {
        var result = await _lugares.GetPorSlugAsync(slug, ct);
        return this.ToActionResult(result);
    }

    [HttpGet("{slug}/calificaciones")]
    public async Task<ActionResult<PagedResult<CalificacionDto>>> GetCalificaciones(
        string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _calificaciones.GetByLugarAsync(slug, page, pageSize, ct);
        return this.ToActionResult(result);
    }

    [HttpPost("{slug}/calificaciones")]
    [Authorize]
    public async Task<ActionResult<CalificacionDto>> CrearCalificacion(
        string slug, [FromBody] CrearCalificacionRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_calificacionValidator, request, ct);
        if (validationError is not null) return validationError;

        var result = await _calificaciones.CrearAsync(slug, request, ct);
        return this.ToActionResult(result);
    }
}
