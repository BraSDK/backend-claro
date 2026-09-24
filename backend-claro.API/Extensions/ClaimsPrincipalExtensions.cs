using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend_claro.Domain.Enums;

namespace backend_claro.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Rol ObtenerRol(this ClaimsPrincipal user)
    {
        var rolClaim = user.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedAccessException("No se pudo determinar el rol del usuario autenticado");

        if (!Enum.TryParse<Rol>(rolClaim, ignoreCase: true, out var rol))
            throw new UnauthorizedAccessException($"Rol '{rolClaim}' no es válido");

        return rol;
    }

    public static int ObtenerUsuarioId(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("No se pudo determinar el usuario autenticado");

        return int.Parse(idClaim);
    }
}