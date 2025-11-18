using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("Persona")]
    public class Persona
    {
        [Key]
        [Column("PersonaId")]
        public Guid PersonaId { get; set; }
        
        [Required]
        [MaxLength(11)]
        [Column("Cuil")]
        public string Cuil { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [Column("Apellido")]
        public string Apellido { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [MaxLength(320)]
        [Column("Email")]
        public string? Email { get; set; }

        public Persona()
        {
            PersonaId = Guid.NewGuid();
        }

        [NotMapped]
        public string NombreCompleto => $"{Apellido}, {Nombre}";
    }
}