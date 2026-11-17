using backend_claro.Application.DTOs.Hojas;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Interfaces;

public interface IHojaService
{
    // Todas las hojas, la más reciente primero. Siempre incluye la del mes actual (aunque esté vacía).
    Task<List<HojaDto>> ListarAsync();

    Task<HojaDto> RenombrarAsync(int hojaId, string nombre);

    // Hoja del mes de esa fecha (UTC); si no existe la agrega al contexto (se guarda con el SaveChanges de quien la pide)
    Task<Hoja> HojaDelMesAsync(DateTime fechaUtc);
}
