using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.GuiaViaje;
using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

/// <summary>Guía de viaje día por día generada con IA (ciudad, días, presupuesto e intereses).</summary>
[ApiController]
[Route("api/v1/ia")]
[Authorize]
public class IaController : ControllerBase
{
    private readonly IGuiaViajeIaService _service;
    private readonly IValidator<GenerarGuiaViajeRequest> _validator;

    public IaController(IGuiaViajeIaService service, IValidator<GenerarGuiaViajeRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Ciudades con lugares disponibles, para el selector de la pantalla de guía.</summary>
    [HttpGet("ciudades")]
    public async Task<ActionResult<IReadOnlyList<CiudadDto>>> Ciudades(CancellationToken ct) =>
        Ok(await _service.CiudadesAsync(ct));

    /// <summary>Genera y guarda una guía. Responde en el idioma de la cabecera Accept-Language.</summary>
    [HttpPost("guia-viaje")]
    public async Task<ActionResult<GuiaViajeDto>> Generar([FromBody] GenerarGuiaViajeRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_validator, request, ct);
        if (validationError is not null) return validationError;

        var result = await _service.GenerarAsync(request, ct);
        return this.ToCreatedResult(result, nameof(Obtener), new { id = result.IsSuccess ? result.Value.Id : 0 });
    }

    [HttpGet("guias-viaje")]
    public async Task<ActionResult<IReadOnlyList<GuiaViajeResumenDto>>> Listar(CancellationToken ct) =>
        Ok(await _service.ListarAsync(ct));

    [HttpGet("guias-viaje/{id:int}", Name = nameof(Obtener))]
    public async Task<ActionResult<GuiaViajeDto>> Obtener(int id, CancellationToken ct) =>
        this.ToActionResult(await _service.ObtenerAsync(id, ct));
}
