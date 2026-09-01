using Microsoft.AspNetCore.Mvc;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Domain.Enums;

namespace backend_claro.API.Controllers;



[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [Authorize(Roles = nameof(Rol.ADMIN))]
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            // Pasamos los datos al servicio
            var resultado = await _authService.RegisterAsync(request);
            
            // Si todo sale bien, devolvemos un 200 OK
            return Ok(new { message = resultado });
        }
        catch (Exception ex)
        {
            var errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { error = errorReal });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            // En este punto, devuelve nuestro string simulado (pronto será el JWT real)
            var token = await _authService.LoginAsync(request);
            
            return Ok(new { token = token });
        }
        catch (Exception ex)
        {
            // Si la contraseña falla, devolvemos un 401 No Autorizado
            return Unauthorized(new { error = ex.Message });
        }
    }
}