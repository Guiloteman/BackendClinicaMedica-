using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("NivelEmergencia")]
    public class NivelEmergencia
    {
        [Key]
        [Column("NivelEmergenciaId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NivelEmergenciaId { get; set; }
        
        [Required]
        [MaxLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [Column("NivelId")]
        public int NivelId { get; set; }
        
        [ForeignKey("NivelId")]
        public virtual Nivel Nivel { get; set; } = null!;
        
        public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}