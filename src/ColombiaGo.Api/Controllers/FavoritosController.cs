using ColombiaGo.Api.Extensions;
using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Application.DTOs.Favoritos;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColombiaGo.Api.Controllers;

[ApiController]
[Route("api/v1/favoritos")]
[Authorize]
public class FavoritosController : ControllerBase
{
    private readonly IFavoritoService _service;

    public FavoritosController(IFavoritoService service) => _service = service;

    /// <summary>Backs <c>FavoritesContext</c> / <c>FavoritesPage</c>.</summary>
    [HttpGet("lugares")]
    public async Task<ActionResult<IReadOnlyList<LugarResumenDto>>> GetLugares(CancellationToken ct) =>
        Ok(await _service.GetLugaresFavoritosAsync(ct));

    [HttpPost("lugares/{slug}")]
    public async Task<ActionResult<FavoritoEstadoDto>> AgregarLugar(string slug, CancellationToken ct) =>
        this.ToActionResult(await _service.AgregarLugarAsync(slug, ct));

    [HttpDelete("lugares/{slug}")]
    public async Task<ActionResult<FavoritoEstadoDto>> QuitarLugar(string slug, CancellationToken ct) =>
        this.ToActionResult(await _service.QuitarLugarAsync(slug, ct));

    [HttpGet("eventos")]
    public async Task<ActionResult<IReadOnlyList<EventoDto>>> GetEventos(CancellationToken ct) =>
        Ok(await _service.GetEventosFavoritosAsync(ct));

    [HttpPost("eventos/{eventoId:int}")]
    public async Task<ActionResult<FavoritoEstadoDto>> AgregarEvento(int eventoId, CancellationToken ct) =>
        this.ToActionResult(await _service.AgregarEventoAsync(eventoId, ct));

    [HttpDelete("eventos/{eventoId:int}")]
    public async Task<ActionResult<FavoritoEstadoDto>> QuitarEvento(int eventoId, CancellationToken ct) =>
        this.ToActionResult(await _service.QuitarEventoAsync(eventoId, ct));
}
