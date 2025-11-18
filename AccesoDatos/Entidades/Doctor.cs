using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("Doctor")]
    public class Doctor
    {
        [Key]
        [Column("PersonaId")]
        [ForeignKey("Persona")]
        public Guid PersonaId { get; set; }
        
        // Navegación a Persona (herencia)
        public virtual Persona Persona { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        [Column("Matricula")]
        public string Matricula { get; set; } = string.Empty;
        
        // Relaciones
        public virtual ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        [NotMapped]
        public string NombreCompleto => Persona?.NombreCompleto ?? string.Empty;
    }
}