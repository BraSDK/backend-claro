
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
    private readonly ILogger<LiquidacionController> _logger;

    public LiquidacionController(ILiquidacion service, ILogger<LiquidacionController> logger)
    {
        _service = service;
        _logger = logger;
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

    [HttpDelete("{sot}/delete")]
    [Authorize(Roles = $"{nameof(Rol.ADMIN)}, {nameof(Rol.ALMACEN)}")] //cualquier usuario autenticado
    public async Task<IActionResult> EliminarLiquidacion(long sot)
    {

        try{
            // Llamamos al servicio que maneja la lógica
            await _service.EliminarLiquidacionAsync(sot);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno del servidor al eliminar la liquidación.", error = ex.Message });
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
            // 👇 Loguea la excepción completa (con inner exceptions) para verla en la terminal
            _logger.LogError(ex, "Error al editar la liquidación SOT {Sot}", request.Sot);

            // 👇 En desarrollo, devuelve la cadena completa de inner exceptions al front
            var detalle = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new
            {
                mensaje = "Error interno del servidor al editar la orden.",
                error = detalle
            });
        }

    }

}