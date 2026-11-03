using backend_claro.Application.DTOs.PagosClaro;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_claro.Api.Extensions;

namespace backend_claro.API.Controllers;

// Pagos de Claro (lotes de SOT auditadas) y multas generales
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(Rol.ADMIN)},{nameof(Rol.ALMACEN)}")]
public class PagoClaroController : ControllerBase
{
    private readonly IPagoClaroService _service;

    public PagoClaroController(IPagoClaroService service)
    {
        _service = service;
    }

    // GET api/PagoClaro/sots-pendientes?desde=2026-11-01&hasta=2026-11-30   (por fecha de auditoría)
    [HttpGet("sots-pendientes")]
    public async Task<IActionResult> SotsPendientes([FromQuery] RangoFechasDto rango)
    {
        return Ok(await _service.SotsPendientesAsync(rango));
    }

    // GET api/PagoClaro?desde=&hasta=   (por fecha de pago)
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] RangoFechasDto rango)
    {
        return Ok(await _service.ListarPagosAsync(rango));
    }

    // GET api/PagoClaro/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        try { return Ok(await _service.ObtenerPagoAsync(id)); }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // POST api/PagoClaro   { fechaPago, comprobante, observacion, sots: [...], multaIds: [...] }
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPagoClaroRequest request)
    {
        try { return Ok(new { pagoClaroId = await _service.CrearPagoAsync(request, User.ObtenerUsuarioId()) }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // DELETE api/PagoClaro/5   (las SOT y multas vuelven a quedar pendientes)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Rol.ADMIN))]
    public async Task<IActionResult> Eliminar(int id)
    {
        try { await _service.EliminarPagoAsync(id); return NoContent(); }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // ===== Multas =====

    // GET api/PagoClaro/multas?desde=&hasta=&soloPendientes=true
    [HttpGet("multas")]
    public async Task<IActionResult> ListarMultas([FromQuery] ListarMultasRequest request)
    {
        return Ok(await _service.ListarMultasAsync(request));
    }

    [HttpPost("multas")]
    public async Task<IActionResult> CrearMulta([FromBody] GuardarMultaRequest request)
    {
        try { return Ok(new { multaClaroId = await _service.CrearMultaAsync(request) }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("multas/{id:int}")]
    public async Task<IActionResult> EditarMulta(int id, [FromBody] GuardarMultaRequest request)
    {
        try { await _service.EditarMultaAsync(id, request); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpDelete("multas/{id:int}")]
    public async Task<IActionResult> EliminarMulta(int id)
    {
        try { await _service.EliminarMultaAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (NotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
