using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("Nivel")]
    public class Nivel
    {
        [Key]
        [Column("NivelId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NivelId { get; set; }
        
        [Required]
        [MaxLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        [Column("Color")]
        public string Color { get; set; } = string.Empty;
        
        [Required]
        [Column("TiempoEsperaMinutos")]
        public int TiempoEsperaMinutos { get; set; }
        
        [Required]
        [Column("Prioridad")]
        public int Prioridad { get; set; }

        public virtual ICollection<NivelEmergencia> NivelesEmergencia { get; set; } = new List<NivelEmergencia>();
    }
}