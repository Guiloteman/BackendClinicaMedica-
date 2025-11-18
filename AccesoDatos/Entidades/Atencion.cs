using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("Atencion")]
    public class Atencion
    {
        [Key]
        [Column("AtencionId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AtencionId { get; set; }
        
        [Required]
        [Column("IngresoId")]
        public Guid IngresoId { get; set; }
        
        [ForeignKey("IngresoId")]
        public virtual Ingreso Ingreso { get; set; } = null!;
        
        [Required]
        [Column("MedicoId")]
        public Guid MedicoId { get; set; }
        
        [ForeignKey("MedicoId")]
        public virtual Doctor Medico { get; set; } = null!;
        
        [Required]
        [Column("Informe")]
        public string Informe { get; set; } = string.Empty;
        
        [Required]
        [Column("FechaAtencion")]
        public DateTime FechaAtencion { get; set; }

        public Atencion()
        {
            AtencionId = Guid.NewGuid();
            FechaAtencion = DateTime.UtcNow;
        }
    }
}