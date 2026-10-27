using backend_claro.API.Converters;
using System.Text.Json.Serialization;

namespace backend_claro.API.Extensions;

public static class JsonOptionsExtensions
{
    public static IMvcBuilder AddCustomJsonOptions(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return builder;
    }
}