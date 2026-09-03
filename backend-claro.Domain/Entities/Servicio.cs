using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_claro.Domain.Entities;

public class Servicio
{

        public enum CategoriaServicio
    {
        HFC = 0,
        FTH = 1,
        MANTTO = 2,
        
    }


    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] //No es autoincrementable
    public int Codigo { get; set;} 
    public string Nombre {get; set;} = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio {get; set;} = 0;

    public CategoriaServicio Categoria {get; set;} = CategoriaServicio.HFC;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    // public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

}