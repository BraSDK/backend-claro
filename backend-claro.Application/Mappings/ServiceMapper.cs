using backend_claro.Application.DTOs.Service;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings;

public static class ServiceMapper
{
    // 1. PARA LISTAR: Transforma consultas enteras de BD a DTOs de salida (Altamente optimizado)
    public static IQueryable<ServiceResponseDto> ToResponseDto(this IQueryable<Servicio> query)
    {
        return query.Select(s => new ServiceResponseDto
        {
            Codigo = s.Codigo,
            Nombre = s.Nombre,
            Precio = s.Precio,
            Categoria = s.Categoria.ToString() // Convierte el Enum a texto
        });
    }

    // 2. PARA CREAR: Transforma un DTO de entrada en una Entidad nueva
    public static Servicio ToEntity(this CreateRequestDto request)
    {
        return new Servicio
        {
            Nombre = request.Nombre,
            Precio = request.Precio,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow
        };
    }

    // 4. PARA EDITAR
    public static void UpdateEntity(this UpdateRequestDto request, Servicio servicioExistente)
    {
        {
            servicioExistente.Nombre = request.Nombre;
            servicioExistente.Precio = request.Precio;
            servicioExistente.Categoria = request.Categoria;
        };
    }

    // 3. PARA DETALLE: Transforma una sola Entidad en un DTO de salida
    public static ServiceResponseDto ToResponseDto(this Servicio entidad)
    {
        return new ServiceResponseDto
        {
            Codigo = entidad.Codigo,
            Nombre = entidad.Nombre,
            Precio = entidad.Precio,
            Categoria = entidad.Categoria.ToString()
        };
    }
}
