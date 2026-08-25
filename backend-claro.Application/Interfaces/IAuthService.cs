using backend_claro.Application.DTOs.Auth;

namespace backend_claro.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequestDto request);
    Task<string> LoginAsync(LoginRequestDto resquest);
}