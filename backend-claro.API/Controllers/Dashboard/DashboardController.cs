using backend_claro.Api.Extensions;
using backend_claro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    // GET api/Dashboard/resumen?anio=2026&mes=7   (sin parámetros = mes actual)
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen([FromQuery] int? anio, [FromQuery] int? mes)
    {
        var hoy = DateTime.UtcNow;
        var a = anio ?? hoy.Year;
        var m = mes ?? hoy.Month;
        if (m < 1 || m > 12) return BadRequest(new { error = "El mes debe estar entre 1 y 12" });

        return Ok(await _service.ResumenMensualAsync(a, m, User.ObtenerRol(), User.ObtenerUsuarioId()));
    }

    // GET api/Dashboard/tecnicos   (para los filtros de las vistas)
    [HttpGet("tecnicos")]
    public async Task<IActionResult> Tecnicos()
    {
        return Ok(await _service.ListarTecnicosAsync());
    }
}
