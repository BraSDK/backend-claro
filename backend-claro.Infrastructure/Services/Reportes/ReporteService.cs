using backend_claro.Application.DTOs.Reportes;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace backend_claro.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private const string FormatoMonto = "#,##0.00";

    private readonly IApplicationDbContext _context;

    public ReporteService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> PagosValidadosExcelAsync(ReporteFiltroDto filtro)
    {
        // el mes completo: desde el día 1 hasta antes del día 1 del mes siguiente
        var desde = new DateTime(filtro.Anio, filtro.Mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var hasta = desde.AddMonths(1);

        var listaReporte = await _context.Liquidaciones.AsNoTracking()
                                                       .Where(l => l.EstadoPago == EstadoPago.SiPago
                                                                && l.FechaValidacion >= desde
                                                                && l.FechaValidacion < hasta)
                                                       .OrderBy(l => l.FechaValidacion)
                                                       .ThenBy(l => l.Sot)
                                                       .Select(l => new
                                                       {
                                                           l.Sot,
                                                           l.NombreTecnico,
                                                           l.FechaProgramacion,
                                                           l.FechaValidacion,
                                                           ACobrar = l.PrecioTotalEditable ?? l.PrecioTotalReal ?? 0,
                                                           l.MontoPagado,
                                                           Comprobante = l.PagoClaro != null ? l.PagoClaro.Comprobante : null,
                                                       })
                                                       .ToListAsync();

        using var libro = new XLWorkbook();

        // el nombre de una hoja no admite / \ : ? * [ ] (ClosedXML lanza error)
        var hoja = libro.Worksheets.Add($"Pagos {filtro.Mes:00}-{filtro.Anio}");

        string[] titulos = { "SOT", "Técnico", "F. programación", "F. auditoría", "A cobrar", "Pagado", "Diferencia", "Comprobante" };
        for (int c = 0; c < titulos.Length; c++)
            hoja.Cell(1, c + 1).Value = titulos[c];

        int fila = 2;
        foreach (var item in listaReporte)
        {
            // si en la auditoría marcaron "Pagó" sin escribir el monto, se asume que pagó completo
            var pagado = item.MontoPagado ?? item.ACobrar;

            hoja.Cell(fila, 1).Value = item.Sot;
            hoja.Cell(fila, 2).Value = item.NombreTecnico;
            hoja.Cell(fila, 3).Value = item.FechaProgramacion;
            hoja.Cell(fila, 4).Value = item.FechaValidacion;
            hoja.Cell(fila, 5).Value = item.ACobrar;
            hoja.Cell(fila, 6).Value = pagado;
            hoja.Cell(fila, 7).Value = pagado - item.ACobrar;
            hoja.Cell(fila, 8).Value = item.Comprobante ?? "Sin pago registrado";
            fila++;
        }

        // fila de totales (fórmulas: si alguien corrige un monto en el Excel, el total se actualiza)
        if (listaReporte.Count > 0)
        {
            hoja.Cell(fila, 4).Value = "Total";
            for (int c = 5; c <= 7; c++)
            {
                var letra = hoja.Column(c).ColumnLetter();
                hoja.Cell(fila, c).FormulaA1 = $"SUM({letra}2:{letra}{fila - 1})";
            }
            hoja.Row(fila).Style.Font.Bold = true;
        }

        // formato: encabezado en negrita con fondo, fechas y montos legibles, filtros y anchos automáticos
        var encabezado = hoja.Range(1, 1, 1, titulos.Length);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F3F3");
        hoja.Columns(3, 4).Style.DateFormat.Format = "dd/MM/yyyy";
        hoja.Columns(5, 7).Style.NumberFormat.Format = FormatoMonto;
        if (listaReporte.Count > 0) hoja.Range(1, 1, fila - 1, titulos.Length).SetAutoFilter();
        hoja.SheetView.FreezeRows(1);
        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return stream.ToArray();
    }
}
