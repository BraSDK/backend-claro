using backend_claro.Application.DTOs.Service;

namespace backend_claro.Application.Interfaces;

public interface IServiceService
{
    Task<ServiceResponseDto>GetByIdAsync(int codigo);
    Task<object>ListAsync(ListRequestDto request);
    Task<string>CreateAsync(CreateRequestDto request);
    Task<string>UpdateAsync(UpdateRequestDto request);
    Task<string>DeleteAsync(int codigo);
}