using backend_claro.Application.DTOs;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdenTrabajoController : ControllerBase
{
    private readonly IOrdenTrabajoService _service;

    public OrdenTrabajoController(IOrdenTrabajoService service)
    {
        _service = service;
    }

    [HttpDelete("{id}/delete")]
    [Authorize] //cualquier usuario autenticado
    public async Task<IActionResult> EliminarOrden (int id)
    {

        try{
            // Opcional: Si necesitas extraer el Id del usuario autenticado desde el Token JWT
            // var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Llamamos al servicio que maneja la lógica
            await _service.EliminarOrdenAsync(id);

            // 204 No Content es el estándar profesional para un DELETE exitoso
            return NoContent();
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error al intentar eliminar la orden {Id}", id);
            return StatusCode(500, new { mensaje = "Error interno del servidor al eliminar la orden." });
        }
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

    // PATCH api/OrdenTrabajo/5/auditoria-pago   { estadoPago, motivoNoPago, observacionPago }
    [HttpPatch("{id}/auditoria-pago")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> AuditarPago(int id, [FromBody] AuditoriaPagoRequest request)
    {
        try { return Ok(await _service.AuditarPagoAsync(id, request, User.ObtenerUsuarioId())); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // PATCH api/OrdenTrabajo/5/edicion-rapida   -> edición de un campo desde la tabla de Órdenes
    [HttpPatch("{id}/edicion-rapida")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> EdicionRapida(int id, [FromBody] EdicionRapidaRequest request)
    {
        try { return Ok(await _service.EdicionRapidaAsync(id, request)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
