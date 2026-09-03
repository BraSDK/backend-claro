using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.User;
using backend_claro.Domain.Enums;

// Libreria de de Claims - Token
using System.Security.Claims;

namespace backend_claro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] EditRequestDto request)
    {
        // Extraemos el ID del usuario que envia el token
        var logueadoIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var rolLogueado = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // Para comparar el ID
        int logueadoId = int.Parse(logueadoIdStr ?? "0");

        if ( logueadoId != request.Id && rolLogueado != nameof(Rol.ADMIN))
        {
            return Forbid();
        }
        try
        {
            var resultado = await _userService.UpdateAsync(request, rolLogueado);

            return Ok( new  { message =  resultado});
        }
        catch (Exception ex)
        {
            // Atrapamos el error y le enviamos un Bad Request (400) al frontend
            return BadRequest(new { error = ex.Message });
        }
    }
}