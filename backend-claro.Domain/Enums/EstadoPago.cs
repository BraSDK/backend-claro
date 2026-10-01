namespace backend_claro.Domain.Enums;

// Resultado de la auditoría de almacén: si Claro paga o no la SOT
public enum EstadoPago
{
    Pendiente = 0,   // todavía no auditada
    SiPago = 1,      // se paga (con el descuento que tenga, si hay)
    NoPago = 2,      // no se paga nada (multa): MotivoNoPago obligatorio
}
