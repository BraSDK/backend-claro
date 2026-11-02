using backend_claro.Application.DTOs.Adicional;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_claro.Api.Extensions;

namespace backend_claro.API.Controllers;

// Catálogo de precios de los adicionales de la liquidación (DECO, REPETIDOR, DROP 150/220/300).
// El Nombre debe coincidir con el concepto que manda la grilla de liquidaciones.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdicionalController : ControllerBase
{
    private readonly IAdicionales _service;

    public AdicionalController(IAdicionales service)
    {
        _service = service;
    }

    // GET api/Adicional
    [HttpGet]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> Listar()
    {
        return Ok(await _service.ListarAdicional(User.ObtenerRol()));
    }

    // POST api/Adicional
    [HttpPost]
    [Authorize(Roles = nameof(Rol.ADMIN))]
    public async Task<IActionResult> Crear([FromBody] CrearAdicionalDto request)
    {
        try
        {
            await _service.CrearAdicional(request);
            return Ok();
        }
        catch (AlreadyExistException ex) { return Conflict(new { error = ex.Message }); }
    }

    // PUT api/Adicional/3
    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Rol.ADMIN))]
    public async Task<IActionResult> Editar(int id, [FromBody] UpdateAdicionalDto request)
    {
        try
        {
            await _service.EditarAdicional(id, request);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // DELETE api/Adicional/3
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Rol.ADMIN))]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _service.EliminarAdicional(id);
            return NoContent();
        }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
