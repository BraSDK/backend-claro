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
            policy.AllowAnyOrigin() // El puerto exacto de tu React
              .AllowAnyHeader()
              .AllowAnyMethod();
        }
        else
        {
            // En el servidor real
            policy.WithOrigins("https://www.tudominio-claro.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplicar la política de CORS
app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();