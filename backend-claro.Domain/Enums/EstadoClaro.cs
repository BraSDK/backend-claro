namespace backend_claro.Domain.Enums;

// Estado de la SOT en el sistema de Claro (TOA). Solo cuando está COMPLETADO se puede liquidar.
public enum EstadoClaro
{
    COMPLETADO,
    NO_COMPLETADO,
    EN_PROGRESO,
}