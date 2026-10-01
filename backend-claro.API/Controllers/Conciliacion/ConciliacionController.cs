using backend_claro.Application.DTOs.Conciliacion;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
public class ConciliacionController : ControllerBase
{
    private readonly IConciliacionService _service;

    public ConciliacionController(IConciliacionService service)
    {
        _service = service;
    }

    // GET api/Conciliacion?anio=2026&mes=7&categoria=1&buscar=887&pagina=1
    [HttpGet]
    public async Task<IActionResult> Conciliar([FromQuery] ConciliacionRequest request)
    {
        return Ok(await _service.ConciliarAsync(request));
    }

    // PUT api/Conciliacion/apelacion   { ordenIds: [1,2], marcar: true }
    [HttpPut("apelacion")]
    public async Task<IActionResult> MarcarApelacion([FromBody] MarcarApelacionRequest request)
    {
        var actualizadas = await _service.MarcarApelacionAsync(request);
        return Ok(new { actualizadas });
    }

    // GET api/Conciliacion/exportar?anio=2026&mes=7   -> Excel con las SOT que no cuadran
    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar([FromQuery] int? anio, [FromQuery] int? mes)
    {
        var hoy = DateTime.UtcNow;
        var a = anio ?? hoy.Year;
        var m = mes is >= 1 and <= 12 ? mes.Value : hoy.Month;
        var archivo = await _service.ExportarDiferenciasAsync(a, m);
        return File(archivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"diferencias-conciliacion-{a}-{m:00}.xlsx");
    }
}
