using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("ObraSocial")]
    public class ObraSocial
    {
        [Key]
        [Column("ObraSocialId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ObraSocialId { get; set; }
        
        [Required]
        [MaxLength(120)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();

        public ObraSocial()
        {
            ObraSocialId = Guid.NewGuid();
        }
    }
}