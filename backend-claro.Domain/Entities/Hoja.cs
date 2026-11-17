using backend_claro.Domain.Interfaces;

namespace backend_claro.Domain.Entities;

// Hoja de trabajo de liquidaciones: una por mes, como las pestañas de un Excel.
// Cada liquidación pertenece a la hoja del mes en que se creó (hora de Perú).
public class Hoja : IAuditable
{
    public int HojaId { get; set; }
    public string Nombre { get; set; } = string.Empty;   // editable; por defecto "Noviembre 2026"
    public int Anio { get; set; }
    public int Mes { get; set; }                          // 1..12

    public ICollection<Liquidacion> Liquidaciones { get; set; } = new List<Liquidacion>();

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public const int LargoMaximoNombre = 100;

    // Perú no tiene horario de verano: siempre UTC-5.
    // Sin esto, una SOT creada el 30/11 a las 8 pm (1/12 en UTC) caía en la hoja de diciembre.
    private static readonly TimeSpan HoraPeru = TimeSpan.FromHours(-5);

    private static readonly string[] NombresMes =
    {
        "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    };

    // Año y mes (en hora de Perú) de una fecha guardada en UTC
    public static (int Anio, int Mes) PeriodoDe(DateTime fechaUtc)
    {
        var fechaPeru = fechaUtc.Add(HoraPeru);
        return (fechaPeru.Year, fechaPeru.Month);
    }

    public static Hoja NuevaParaPeriodo(int anio, int mes) => new()
    {
        Anio = anio,
        Mes = mes,
        Nombre = $"{NombresMes[mes - 1]} {anio}",
    };

    public void Renombrar(string nombre)
    {
        var limpio = nombre.Trim();
        if (limpio.Length == 0) throw new ArgumentException("El nombre de la hoja no puede estar vacío");
        if (limpio.Length > LargoMaximoNombre) throw new ArgumentException($"El nombre admite hasta {LargoMaximoNombre} caracteres");
        Nombre = limpio;
    }
}
