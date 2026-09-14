using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Dashboard;

namespace backend_claro.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public DashboardService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task <DashboardResumenDto> ObtenerResumenAsync()
    {
        // Conteo de todos los datos de cada tabla
        // Espera a que la base de datos responda, abre la caja y saca el número real (int) - await
        var totalServicios = await _context.Servicios.CountAsync(); 
        var totalUsuarios = await _context.Usuarios.CountAsync();

        var ordenesPendientes = 0;
        
        return new DashboardResumenDto
        {
            TotalServicios = totalServicios,
            OrdenesPendientes = ordenesPendientes,
            TotalUsuarios =  totalUsuarios
        };
    }
}