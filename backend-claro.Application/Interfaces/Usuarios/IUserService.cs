using backend_claro.Application.DTOs.User;

namespace backend_claro.Application.Interfaces;

public interface IUserService
{
    Task<object> ListAsync(UserListResponseDto request);
    Task<string> UpdateAsync(EditRequestDto request, string rolLogueado);
    Task<string> DeleteAsync(int id);
}