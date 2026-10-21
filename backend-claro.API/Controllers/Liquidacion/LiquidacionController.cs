
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using backend_claro.Application.DTOs.Liquidaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_claro.Api.Extensions;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiquidacionController : ControllerBase
{
    private readonly ILiquidacion _service;

    public LiquidacionController(ILiquidacion service)
    {
        _service = service;
    }

    [HttpGet("Listar")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> ListarLiquidaciones([FromQuery] ListParamsLiquidacionesDto request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var usuarioRol = User.ObtenerRol();
        var listaLiquidaciones = await _service.ListarAsync(request,usuarioRol,usuarioId);
        return Ok(listaLiquidaciones);
    }

    [HttpPost("crearLiquidacion")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
    public async Task<IActionResult> CreateLiquidacion([FromBody] CrearLiquidacionRequest request)
    {
        try
        {
           var usuarioId = User.ObtenerUsuarioId();
           await _service.CrearLiquidacionAsync(request, usuarioId);

            return Ok();
        }
        catch (Exception ex)
        {
            var errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { error = errorReal });
        }
    }

    [HttpDelete("{id}/delete")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)}, {nameof(Rol.ALMACEN)}")] //cualquier usuario autenticado
    public async Task<IActionResult> EliminarLiquidacion(int id)
    {

        try{
            // Llamamos al servicio que maneja la lógica
            await _service.EliminarLiquidacionAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error al intentar eliminar la orden {Id}", id);
            return StatusCode(500, new { mensaje = "Error interno del servidor al eliminar la orden.", ex });
        }
    }

    [HttpPut("{id}/EditarLiquidacion")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)}, {nameof(Rol.ALMACEN)}")] //cualquier usuario autenticado
    public async Task<IActionResult> EditarLiquidacion( [FromBody] EditarLiquidacionRequestDto request )
    {
        try{
            var usuarioId = User.ObtenerUsuarioId();
            // Llamamos al servicio que maneja la lógica
            await _service.EditarLiquidacionAsync(request , usuarioId);
            // 204 No Content es el estándar profesional para un DELETE exitoso
            return Ok();
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error al intentar eliminar la orden {Id}", id);
            return StatusCode(500, new { mensaje = "Error interno del servidor al editar la orden.", ex });
        }
    }

}