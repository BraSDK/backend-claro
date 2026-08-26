using Microsoft.EntityFrameworkCore;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Auth;
using backend_claro.Domain.Entities;
using backend_claro.Application.Mappings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace backend_claro.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    // Inyectamos tu contexto de base de datos
    public AuthService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Verificar si el correo ya está registrado
        var existeUsuario = await _context.CuentaUsuarios.AnyAsync(c => c.Email == request.Email);
        if (existeUsuario)
        {
            throw new Exception("El correo electrónico ya está registrado.");
        }

        // Llamamos al método Mapeado
        var nuevaCuenta = request.ToEntity();

        nuevaCuenta.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. Guardar en la base de datos de PostgreSQL
        _context.CuentaUsuarios.Add(nuevaCuenta);
        await _context.SaveChangesAsync();

        return "Usuario registrado correctamente.";
    }

    public async Task<string> LoginAsync(LoginRequestDto request)
    {
        // 1. Buscar la cuenta por email
        var cuenta = await _context.CuentaUsuarios.FirstOrDefaultAsync(c => c.Email == request.Email);
        
        // Si no existe, devolvemos un error genérico por seguridad (no dar pistas a atacantes)
        if (cuenta == null)
        {
            throw new Exception("Credenciales incorrectas.");
        }

        // 2. Verificar que la contraseña coincida con el Hash guardado
        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, cuenta.PasswordHash);

        if (!passwordValida)
        {
            throw new Exception("Credenciales incorrectas.");
        }

        // 3. Por ahora devolvemos un texto simulado. En el próximo paso generaremos el Token Real.
        return GenerarTokenJwt(cuenta);
    }

    // Metodo provado para fabricar el Token
    private string GenerarTokenJwt(CuentaUsuario cuenta)
    {
        // Leer las variables es appsettings.json
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["secretKey"];
        var key = Encoding.ASCII.GetBytes(secretKey!);

        // Crear los Claims (la imformacion que viajara dentro del token)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, cuenta.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, cuenta.Email),
            new Claim("rol", cuenta.Rol) //Guardamos el rol para futuras validaciones
        };

        // Configurar los detalles del token (firma, expiracion, etc.)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"]!)),
            SigningCredentials =  new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        // Crear y devolver el token final
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}