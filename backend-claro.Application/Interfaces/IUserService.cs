using backend_claro.Application.DTOs.User;

namespace backend_claro.Application.Interfaces;

public interface IUserService
{
    Task<string> UpdateAsync(EditRequestDto request);
}