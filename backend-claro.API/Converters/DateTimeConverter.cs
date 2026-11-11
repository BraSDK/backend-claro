using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend_claro.API.Converters;

public class DateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] FormatosAceptados = new[]
    {
        "yyyy-MM-dd",                  // ISO estándar
        "yyyy-MM-ddTHH:mm:ss",         // ISO con hora
        "yyyy-MM-ddTHH:mm:ss.fffZ",    // ISO con milisegundos y Z
        "dd-MM-yyyy",                  // DD-MM-YYYY (tu caso)
        "dd/MM/yyyy",                  // DD/MM/YYYY
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var texto = reader.GetString();

        if (string.IsNullOrWhiteSpace(texto))
            return default;

        // Intento 1: formato ISO estándar (lo más rápido)
        if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fecha))
            return fecha;

        // Intento 2: formatos personalizados
        foreach (var formato in FormatosAceptados)
        {
            if (DateTime.TryParseExact(texto, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                return fecha;
        }

        throw new JsonException($"Formato de fecha no reconocido: '{texto}'");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
    }
}