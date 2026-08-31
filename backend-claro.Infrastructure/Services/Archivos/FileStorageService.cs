using backend_claro.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;


namespace backend_claro.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _config;  
    public FileStorageService(IConfiguration config)
    {
        _config = config;
    }
    async Task<string> IFileStorageService.GestionarArchivo(IFormFile archivo, string src)
    {

        if(archivo is null || archivo.Length < 0)
        {
            throw new ArgumentException("El archivo está vacío.", nameof(archivo));
        }

        
        string? rutaBase = _config["Storage:RutaBase"];

       

        if (string.IsNullOrEmpty(rutaBase))
        {
            throw new InvalidOperationException("Falta la configuración 'Storage:RutaBase'.");
        }

        string rutaFinal = Path.Combine(rutaBase,src);

        Directory.CreateDirectory(rutaFinal);

        string extension = Path.GetExtension(archivo.FileName);

        string nuevoNombre = $"{Guid.NewGuid():N}{extension}";
        
        //Datos 
        string rutaCompleta = Path.Combine(rutaFinal,nuevoNombre);
        await using var stream = new FileStream(rutaCompleta, FileMode.Create);
        await archivo.CopyToAsync(stream);

        return Path.Combine(src, rutaCompleta).Replace("\\","/");

    }
}