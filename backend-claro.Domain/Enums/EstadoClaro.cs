namespace backend_claro.Domain.Enums;

// Estado de la SOT en el sistema de Claro (TOA). Solo cuando está COMPLETADO se puede liquidar.
public enum EstadoClaro
{
    COMPLETADO,
    NO_COMPLETADO,
    EN_PROGRESO,
}


public enum ImagenValidacionClaro
{
    SIN_VALIDAR,
    CORRECTOs,
    OBSERVADO
}



public enum EstadoAlmacen
{
    SIN_VALIDAR,
    CORRECTO,
    OBSERVADO
}