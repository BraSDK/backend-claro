using backend_claro.Application.DTOs.User;

namespace backend_claro.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> ListAsync(int id);
    Task<string> UpdateAsync(EditRequestDto request, string rolLogueado);
    Task<string> DeleteAsync(int id);
}