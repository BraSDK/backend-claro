using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Service;
using backend_claro.Domain.Enums;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetById(int codigo)
    {
        try
        {
            var resultado = await _serviceService.GetByIdAsync(codigo);
            return Ok ( new { message = resultado});
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] ListRequestDto request)
    {
        var resultado = await _serviceService.ListAsync(request);
        return Ok(resultado);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto request)
    {
        try
        {
            var resultado = await _serviceService.CreateAsync(request);

            return Ok(new { message = resultado });
        }
        catch (Exception ex)
        {
            var errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { error = errorReal });
        }
    }

    [Authorize(Roles = nameof(Rol.ADMIN))]
    [HttpPut ("update")]
    public async Task <IActionResult> Update([FromBody] UpdateRequestDto request)
    {
        var resultado = await _serviceService.UpdateAsync(request);
        return Ok(new { message = resultado }) ;
    }

    [Authorize(Roles = nameof(Rol.ADMIN))]
    [HttpDelete ("{codigo}")]
    public async Task<IActionResult> Delete(int codigo)
    {
        try
        {
            var resultado = await _serviceService.DeleteAsync(codigo);
            return Ok(new { message = resultado });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
