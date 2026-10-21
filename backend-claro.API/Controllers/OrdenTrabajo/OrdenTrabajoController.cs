using backend_claro.Application.DTOs;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

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

    // GET api/OrdenTrabajo/List?pagina=1&canPagina=15&buscar=887&tecnicoId=4&desde=2026-10-01&hasta=2026-10-31
    // Un técnico solo recibe sus propias órdenes (se decide en el servicio con el rol del token).
    [HttpGet("List")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> Listar([FromQuery] ListRequestOrdenesDto request)
    {
        return Ok(await _service.ListarAsync(request, User.ObtenerRol(), User.ObtenerUsuarioId()));
    }

    // POST api/OrdenTrabajo   (multipart/form-data)
    // Campos de la orden + "imagenes" (opcional, varias). Crea la orden y luego sube sus imágenes.
    [HttpPost]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Crear([FromForm] CrearOrdenTrabajoRequest request, [FromForm] List<IFormFile>? imagenes)
    {
        try
        {
            var orden = await _service.CrearOrdenTrabajoAsync(request, User.ObtenerRol(), User.ObtenerUsuarioId());

            if (imagenes is { Count: > 0 })
                orden.Archivos = await _service.AgregarArchivosAsync(orden.OrdenTrabajoId, imagenes);

            return CreatedAtAction(nameof(Listar), new { buscar = orden.Sot }, orden);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
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
        catch (Exception)
        {
            //_logger.LogError(ex, "Error al intentar eliminar la orden {Id}", id);
            return StatusCode(500, new { mensaje = "Error interno del servidor al eliminar la orden." });
        }
    }

    // POST Api/OrdenTrabajo/5/archivos   
    [HttpPost("{id}/archivos")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    public async Task<IActionResult> AgregarArchivos(int id, [FromForm] List<IFormFile> archivos)
    {
        try { return Ok(await _service.AgregarArchivosAsync(id, archivos)); }
        catch (Exception ex) when (ex is NotFoundException or KeyNotFoundException) { return NotFound(new { error = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE Api/OrdenTrabajo/5/archivos/12
    [HttpDelete("{id}/archivos/{archivoId}")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.TECNICO)}")]
    public async Task<IActionResult> EliminarArchivo(int id, int archivoId)
    {
        try { await _service.EliminarArchivoAsync(id, archivoId); return NoContent(); }
        catch (Exception ex) when (ex is NotFoundException or KeyNotFoundException) { return NotFound(new { error = ex.Message }); }
    }

    // PATCH api/OrdenTrabajo/5/edicion-rapida   -> editar la orden (solo los campos que llegan)
    [HttpPatch("{id}/edicion-rapida")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> EdicionRapida(int id, [FromBody] EdicionRapidaRequest request)
    {
        try { return Ok(await _service.EdicionRapidaAsync(id, request)); }
        catch (Exception ex) when (ex is NotFoundException or KeyNotFoundException) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
