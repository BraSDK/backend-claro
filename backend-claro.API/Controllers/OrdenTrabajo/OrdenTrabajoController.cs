using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    // ============ LECTURA ============

    // GET Api/OrdenTrabajo
    [HttpGet]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

    // GET Api/OrdenTrabajo/5
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _service.ObtenerPorIdAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // GET Api/OrdenTrabajo/sot/1234
    [HttpGet("sot/{sot}")]
    public async Task<IActionResult> ObtenerPorSot(int sot)
    {
        try { return Ok(await _service.ObtenerPorSotAsync(sot)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // ============ ORDEN (cabecera) ============

    // POST Api/OrdenTrabajo   (multipart/form-data por las imagenes)
    [HttpPost]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    public async Task<IActionResult> Crear([FromForm] CrearOrdenRequest request)
    {
        try { return Ok(await _service.CrearAsync(request)); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT Api/OrdenTrabajo/5   
    [HttpPut("{id}")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarOrdenRequest request)
    {
        try
        {
            return Ok(await _service.EditarAsync(id, request));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ============ DETALLES ============

    // PUT Api/OrdenTrabajo/1/detalles/
    [HttpPut("{id}/detalles-edit")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> EditarDetalles(int id, [FromBody] List<DetalleEditar> detalles)
    {
        try
        {
            await _service.EditarDetallesAsunc(id, detalles);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }


    // POST Api/OrdenTrabajo/5/detalles
    [HttpPost("{id}/detalles")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> AgregarDetalle(int id, [FromBody] CrearDetalleRequest request)
    {
        try { return Ok(await _service.AgregarDetalleAsync(id, request)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE Api/OrdenTrabajo/5/detalles/9
    [HttpDelete("{id}/detalles/{detalleId}")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> EliminarDetalle(int id, int detalleId)
    {
        try 
        { 
            await _service.EliminarDetalleAsync(id, detalleId); return NoContent(); 
        }
        catch (KeyNotFoundException ex) 
        { 
            return NotFound(new { error = ex.Message }); 
        }
    }


    // POST Api/OrdenTrabajo/5/archivos   
    [HttpPost("{id}/archivos")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    public async Task<IActionResult> AgregarArchivos(int id, [FromForm] List<IFormFile> archivos)
    {
        try { return Ok(await _service.AgregarArchivosAsync(id, archivos)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE Api/OrdenTrabajo/5/archivos/12
    [HttpDelete("{id}/archivos/{archivoId}")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    public async Task<IActionResult> EliminarArchivo(int id, int archivoId)
    {
        try { await _service.EliminarArchivoAsync(id, archivoId); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
