using backend_claro.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;


namespace backend_claro.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _config;  
    public FileStorageService(IConfiguration config)
    {
        _config = config;
    }


public async Task<string> EliminarArchivo(string src)
{
    string? rutaBase = _config["Storage:RutaBase"];

    if (string.IsNullOrEmpty(rutaBase))
    {
        throw new InvalidOperationException("La ruta base no está configurada.");
    }

    string rutaFisica = Path.GetFullPath(Path.Combine(rutaBase, src));

    if (!rutaFisica.StartsWith(Path.GetFullPath(rutaBase), StringComparison.OrdinalIgnoreCase))
    {
        throw new UnauthorizedAccessException("Acceso denegado a la ruta especificada.");
    }

    if (File.Exists(rutaFisica))
    {
        File.Delete(rutaFisica);
    }

    // 4. Corrección de Task/async: Retornar el resultado directamente sin 'async'
    return rutaFisica;
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