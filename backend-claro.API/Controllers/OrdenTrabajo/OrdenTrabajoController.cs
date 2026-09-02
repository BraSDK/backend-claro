using backend_claro.Application.Interfaces;
using backend_claro.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_claro.Domain.Enums;
using backend_claro.Application.DTOs.OrdenTrabajo;
namespace backend_claro.API.Controllers;

[ApiController]
[Route("Api/[controller]")]
[Authorize]
public class OrdenTrabajoController : ControllerBase
{   
    private readonly IOrdenTrabajoService _service;

    public OrdenTrabajoController(IOrdenTrabajoService service)
    {
        _service = service;
        
    }

    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] OrdenTrabajoDto request)
    {
        try
        {
            // Pasamos los datos al servicio
            var resultado = await _service.AsyncRegister(request);

            return Ok(new 
            {
                Sot = resultado.Sot,
                Descripcion = resultado.Descripcion,
                Estado = resultado.Estado,
                Imagenes = resultado.Archivos.Select(a => new { a.NombreArchivo, a.Src })
                
            });
        
        }
        catch (Exception ex)
        {
    
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)},{nameof(Rol.ALMACEN)}")]
    [HttpGet]
    public async Task<IActionResult> ListarOrdenes()
    {
             
        return Ok( await _service.AsyncListar());
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> AsyncObtenerPorId(int id)
    {
        try
        {
            return Ok(await _service.AsyncObtenerPorId(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });   
        }
    }
   [HttpGet("sot/{sot}")]
    public async Task<IActionResult> AsyncObtenerPorSot(int sot)
    {
        try
        {
            return Ok(await _service.AsyncObtenerPorSot(sot));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });   
        }
    }

        
    
}
