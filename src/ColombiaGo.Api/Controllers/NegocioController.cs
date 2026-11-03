using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Negocio;
using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

/// <summary>Panel de las cuentas de negocio: sus lugares publicados y su estado de moderación.</summary>
[ApiController]
[Route("api/v1/negocio")]
[Authorize(Policy = "RequireNegocio")]
public class NegocioController : ControllerBase
{
    private readonly INegocioService _service;
    private readonly IValidator<GuardarLugarRequest> _validator;

    public NegocioController(INegocioService service, IValidator<GuardarLugarRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet("lugares")]
    public async Task<ActionResult<IReadOnlyList<MiLugarDto>>> MisLugares(CancellationToken ct) =>
        Ok(await _service.GetMisLugaresAsync(ct));

    /// <summary>Publica un lugar; queda "pendiente" hasta que un admin lo aprueba.</summary>
    [HttpPost("lugares")]
    public async Task<ActionResult<MiLugarDto>> Crear([FromBody] GuardarLugarRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_validator, request, ct);
        if (validationError is not null) return validationError;

        return this.ToActionResult(await _service.CrearAsync(request, ct));
    }

    /// <summary>Edita un lugar propio; vuelve a "pendiente" para que un admin revise los cambios.</summary>
    [HttpPut("lugares/{id:int}")]
    public async Task<ActionResult<MiLugarDto>> Actualizar(int id, [FromBody] GuardarLugarRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_validator, request, ct);
        if (validationError is not null) return validationError;

        return this.ToActionResult(await _service.ActualizarAsync(id, request, ct));
    }
}
