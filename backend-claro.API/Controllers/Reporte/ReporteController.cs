using backend_claro.Application.DTOs.Reportes;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_claro.API.Controllers;

// Reportes descargables en Excel (mismos roles que el permiso verReportes del front)
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
public class ReporteController : ControllerBase
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IReporteService _service;

    public ReporteController(IReporteService service)
    {
        _service = service;
    }

    // GET api/Reporte/pagos-validados?anio=2026&mes=11
    [HttpGet("pagos-validados")]
    public async Task<IActionResult> PagosValidados([FromQuery] ReporteFiltroDto filtro)
    {
        if (filtro.Mes is < 1 or > 12 || filtro.Anio is < 2000 or > 2100)
            return BadRequest(new { error = "Año o mes no válido" });

        var bytes = await _service.PagosValidadosExcelAsync(filtro);
        return File(bytes, TipoExcel, $"pagos-validados-{filtro.Anio}-{filtro.Mes:00}.xlsx");
    }
}
