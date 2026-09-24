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
    Console.WriteLine("========================\n"+ rutaBase + "\n==========================");

    if (string.IsNullOrEmpty(rutaBase))  
    {
        throw new InvalidOperationException("La ruta base no está configurada.");
    }
    //el combine evita la ruta raiz al estar con un slash antes asi que volaréel slash
    if (src.StartsWith("/"))
    {
        src = src.Substring(1);
    } 
    
    string rutaFisica = Path.GetFullPath(Path.Combine(rutaBase, src));
    Console.WriteLine("========================\n"+ rutaFisica + "\n==========================");
 
    if (!rutaFisica.StartsWith(Path.GetFullPath(rutaBase), StringComparison.OrdinalIgnoreCase))
    {
        throw new UnauthorizedAccessException("Acceso denegado a la ruta especificada.");
    }

    if (File.Exists(rutaFisica))
    {
        Console.WriteLine(rutaFisica);
        File.Delete(rutaFisica);
    }

    // 4. Corrección de Task/async: Retornar el resultado directamente sin 'async'
    return rutaFisica;
}

    async Task<string> IFileStorageService.GestionarArchivo(IFormFile archivo, string src)
    {

        if(archivo is null || archivo.Length < 1)
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

       return $"/{src}/{nuevoNombre}".Replace("\\", "/");

    }
}