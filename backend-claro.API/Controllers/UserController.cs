using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.User;
using backend_claro.Domain.Enums;

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

    [Authorize(Roles = nameof(Rol.ADMIN))]
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] EditRequestDto request)
    {
        try
        {
            var resultado = await _userService.UpdateAsync(request);

            return Ok( new  { message =  resultado});
        }
        catch (Exception ex)
        {
            // Atrapamos el error y le enviamos un Bad Request (400) al frontend
            return BadRequest(new { error = ex.Message });
        }
    }
}