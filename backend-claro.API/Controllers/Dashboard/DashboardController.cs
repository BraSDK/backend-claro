using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_claro.Api.Extensions;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // GET api/Dashboard/resumen?anio=2026&mes=11   (sin parámetros = mes actual)
    // Un técnico solo recibe el resumen de sus propias órdenes.
    [HttpGet("resumen")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> Resumen([FromQuery] int? anio, [FromQuery] int? mes)
    {
        var hoy = DateTime.UtcNow;
        var anioConsulta = anio ?? hoy.Year;
        var mesConsulta = mes ?? hoy.Month;
        if (mesConsulta is < 1 or > 12 || anioConsulta is < 2000 or > 2100)
            return BadRequest(new { error = "Año o mes no válido" });

        return Ok(await _service.ResumenAsync(anioConsulta, mesConsulta, User.ObtenerRol(), User.ObtenerUsuarioId()));
    }

    // GET api/Dashboard/tecnicos   lista para los filtros (ej. Conciliación)
    [HttpGet("tecnicos")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> Tecnicos()
    {
        return Ok(await _service.TecnicosAsync());
    }
}
