using backend_claro.Application.DTOs.Hojas;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_claro.API.Controllers;

// Hojas (pestañas por mes) de la grilla de liquidaciones. Mismos roles que la grilla.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
public class HojaController : ControllerBase
{
    private readonly IHojaService _service;

    public HojaController(IHojaService service)
    {
        _service = service;
    }

    // GET api/Hoja
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        return Ok(await _service.ListarAsync());
    }

    // PUT api/Hoja/5/nombre   { "nombre": "Noviembre – cierre" }
    [HttpPut("{id}/nombre")]
    public async Task<IActionResult> Renombrar(int id, [FromBody] RenombrarHojaDto request)
    {
        try
        {
            return Ok(await _service.RenombrarAsync(id, request.Nombre));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
