using Microsoft.AspNetCore.Mvc;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Domain.Enums;

namespace backend_claro.API.Controllers;



[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [Authorize(Roles = nameof(Rol.ADMIN))]
    [HttpPost("register")]
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
            // Si el correo ya existe, devolvemos un 400 Bad Request
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
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