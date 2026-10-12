namespace backend_claro.Domain.Enums;

// Resultado de la auditoría de almacén: si Claro paga o no la SOT
public enum EstadoPago
{
    Pendiente = 0,   // todavía no auditada, corregir liquidacion (para recibir el proxima boleta)
    SiPago = 1,      // se paga (con el descuento que tenga, si hay)
    NoPagara = 2,      // no se paga nada (multa): MotivoNoPago obligatorio (penalizacion completa)
}
