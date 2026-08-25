using Microsoft.EntityFrameworkCore;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Auth;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;

    // Inyectamos tu contexto de base de datos
    public AuthService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Verificar si el correo ya está registrado
        var existeUsuario = await _context.CuentaUsuarios.AnyAsync(c => c.Email == request.Email);
        if (existeUsuario)
        {
            throw new Exception("El correo electrónico ya está registrado.");
        }

        // 2. Encriptar (Hashear) la contraseña con BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Construir las entidades unidas (Entity Framework maneja la relación 1 a 1 automáticamente)
        var nuevaCuenta = new CuentaUsuario
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Rol = request.Rol,
            FechaRegistro = DateTime.UtcNow,
            // Construimos el perfil anidado directamente
            Perfil = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                DocumentoIdentidad = request.DocumentoIdentidad,
                FechaActualizacion = DateTime.UtcNow
            }
        };

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
        return "Simulacion_De_Token_JWT_Valido_123456";
    }
}