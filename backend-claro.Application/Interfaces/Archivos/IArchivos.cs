using Microsoft.AspNetCore.Http;

namespace backend_claro.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> GestionarArchivo(IFormFile archivo, string src ); 

    Task<string> EliminarArchivo(string src);
}