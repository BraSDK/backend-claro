using backend_claro.Application.DTOs.Hojas;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace backend_claro.Infrastructure.Services;

public class HojaService : IHojaService
{
    private readonly IApplicationDbContext _context;

    public HojaService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HojaDto>> ListarAsync()
    {
        // la hoja del mes actual siempre existe: es la "hoja limpia" donde se empieza a trabajar
        await HojaDelMesAsync(DateTime.UtcNow);
        await _context.SaveChangesAsync();

        return await _context.Hojas
            .AsNoTracking()
            .OrderByDescending(h => h.Anio).ThenByDescending(h => h.Mes)
            .Select(h => new HojaDto
            {
                HojaId = h.HojaId,
                Nombre = h.Nombre,
                Anio = h.Anio,
                Mes = h.Mes,
                CantidadLiquidaciones = h.Liquidaciones.Count,
            })
            .ToListAsync();
    }

    public async Task<HojaDto> RenombrarAsync(int hojaId, string nombre)
    {
        var hoja = await _context.Hojas.FirstOrDefaultAsync(h => h.HojaId == hojaId)
            ?? throw new NotFoundException("La hoja no existe");

        hoja.Renombrar(nombre);   // valida vacío y largo
        await _context.SaveChangesAsync();

        return new HojaDto
        {
            HojaId = hoja.HojaId,
            Nombre = hoja.Nombre,
            Anio = hoja.Anio,
            Mes = hoja.Mes,
            CantidadLiquidaciones = await _context.Liquidaciones.CountAsync(l => l.HojaId == hoja.HojaId),
        };
    }

    public async Task<Hoja> HojaDelMesAsync(DateTime fechaUtc)
    {
        var (anio, mes) = Hoja.PeriodoDe(fechaUtc);

        // primero en memoria: al importar un Excel se crean varias liquidaciones antes de guardar,
        // y todas deben usar la MISMA hoja nueva (si no, se intentaría crear dos veces y chocaría el índice único)
        var hoja = _context.Hojas.Local.FirstOrDefault(h => h.Anio == anio && h.Mes == mes)
                   ?? await _context.Hojas.FirstOrDefaultAsync(h => h.Anio == anio && h.Mes == mes);

        if (hoja is null)
        {
            hoja = Hoja.NuevaParaPeriodo(anio, mes);
            _context.Hojas.Add(hoja);
        }

        return hoja;
    }
}
