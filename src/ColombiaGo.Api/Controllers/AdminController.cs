using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Admin;
using ColombiaGo.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IReporteService _reportes;
    private readonly IValidator<ModerarLugarRequest> _moderarLugarValidator;
    private readonly IValidator<CrearReporteRequest> _crearReporteValidator;

    public AdminController(
        IAdminService admin,
        IReporteService reportes,
        IValidator<ModerarLugarRequest> moderarLugarValidator,
        IValidator<CrearReporteRequest> crearReporteValidator)
    {
        _admin = admin;
        _reportes = reportes;
        _moderarLugarValidator = moderarLugarValidator;
        _crearReporteValidator = crearReporteValidator;
    }

    [HttpGet("lugares/pendientes")]
    public async Task<ActionResult<PagedResult<LugarPendienteDto>>> GetLugaresPendientes(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await _admin.GetLugaresPendientesAsync(page, pageSize, ct));

    [HttpPatch("lugares/{id:int}/moderar")]
    public async Task<IActionResult> ModerarLugar(int id, [FromBody] ModerarLugarRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_moderarLugarValidator, request, ct);
        if (validationError is not null) return validationError;

        return this.ToActionResult(await _admin.ModerarLugarAsync(id, request, ct));
    }

    [HttpGet("calificaciones/pendientes")]
    public async Task<ActionResult<PagedResult<CalificacionPendienteDto>>> GetCalificacionesPendientes(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await _admin.GetCalificacionesPendientesAsync(page, pageSize, ct));

    [HttpPatch("calificaciones/{id:int}/moderar")]
    public async Task<IActionResult> ModerarCalificacion(
        int id, [FromBody] ModerarCalificacionRequest request, CancellationToken ct) =>
        this.ToActionResult(await _admin.ModerarCalificacionAsync(id, request, ct));

    [HttpPost("reportes")]
    public async Task<ActionResult<ReporteDto>> CrearReporte([FromBody] CrearReporteRequest request, CancellationToken ct)
    {
        var validationError = await this.ValidateAsync(_crearReporteValidator, request, ct);
        if (validationError is not null) return validationError;

        return Ok(await _reportes.CrearAsync(request, ct));
    }
}
