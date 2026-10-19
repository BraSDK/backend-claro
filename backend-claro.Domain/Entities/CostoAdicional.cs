using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using backend_claro.Domain.Interfaces;
namespace backend_claro.Domain.Entities;


public class CostoAdicional :IAuditable
{
    /*
        decos
        repetidor
        220 mts
    */
    public enum TipoAdicional { Adicional,DROP150,DROP200,DROP300} //TIPO QUE VA A SEPARAR
    [Key]
    public int AdicionalId {get; set;}  

    public string Nombre {get; set;} = String.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; } 

    public TipoAdicional Tipo { get; set; }

    public string? ColumnaExcel { get; set; } 
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set;} =DateTime.UtcNow;



}