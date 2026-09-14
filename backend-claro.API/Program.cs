using backend_claro.Infrastructure;
using backend_claro.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin() //=> permite calquier origen
              .AllowAnyHeader()
              .AllowAnyMethod();
        }
        else
        {
            
            policy.WithOrigins("https://www.tudominio-claro.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddInfrastructureServices(builder.Configuration);
/*
builder.Services.AddCors(
    cors =>
    {
        cors.AddPolicy("Politica_privada",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
    }
);
*/
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseCors("Politica_privada");
app.UseHttpsRedirection();

// Aplicar la política de CORS
app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("=== VALORES DE CONFIGURACIÓN CARGADOS ===");

foreach (var kvp in app.Configuration.AsEnumerable())
{
    if (!string.IsNullOrEmpty(kvp.Value)) 
    {
        Console.WriteLine($"[Clave]: {kvp.Key}  -->  [Valor]: {kvp.Value}");
    }
}

Console.WriteLine("=========================================");

app.Run();