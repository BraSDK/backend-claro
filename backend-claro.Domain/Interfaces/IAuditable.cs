namespace backend_claro.Domain.Interfaces;

public interface IAuditable
{
    DateTime FechaCreacion {get; set;}
    DateTime FechaActualizacion {get; set;}
}