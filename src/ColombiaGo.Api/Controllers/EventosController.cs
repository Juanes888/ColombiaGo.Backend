using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/eventos")]
public class EventosController : ControllerBase
{
    private readonly IEventoService _service;
    private readonly IValidator<CrearEventoRequest> _validator;

    public EventosController(IEventoService service, IValidator<CrearEventoRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EventoDto>>> Buscar(
        [FromQuery] string? ciudad, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await _service.BuscarAsync(ciudad, page, pageSize, ct));

    [HttpGet("{id:int}", Name = nameof(GetPorId))]
    public async Task<ActionResult<EventoDto>> GetPorId(int id, CancellationToken ct) =>
        this.ToActionResult(await _service.GetPorIdAsync(id, ct));

    /// <summary>Restricted to guides/admins — a tourist browsing events isn't the one publishing them.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireGuia")]
    public async Task<ActionResult<EventoDto>> Crear([FromBody] CrearEventoRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_validator, request, ct);
        if (validationError is not null) return validationError;

        var result = await _service.CrearAsync(request, ct);
        return this.ToCreatedResult(result, nameof(GetPorId), new { id = result.IsSuccess ? result.Value.Id : 0 });
    }
}
