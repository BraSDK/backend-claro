using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Dashboard;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]//Agarra la palabra que no sea controlador
[Authorize] // Microsoft.AspNetCore.Authorization;
public class DashboardController : ControllerBase // Lo vuelve en un controlador API - Microsoft.AspNetCore.Mvc;
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;   
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen()//Resultado de la peticion HTTP (200, 400, 404, 500) - IActionResult
    {
        var resultado = await _dashboardService.ObtenerResumenAsync();
        return Ok(resultado);
    }
}