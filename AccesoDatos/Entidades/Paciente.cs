using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("Paciente")]
    public class Paciente
    {
        [Key]
        [Column("PersonaId")]
        [ForeignKey("Persona")]
        public Guid PersonaId { get; set; }
        
        // Navegación a Persona (herencia)
        public virtual Persona Persona { get; set; } = null!;
        
        // Domicilio
        [Required]
        [MaxLength(120)]
        [Column("Calle")]
        public string Calle { get; set; } = string.Empty;
        
        [Required]
        [Column("Numero")]
        public int Numero { get; set; }
        
        [Required]
        [MaxLength(100)]
        [Column("Localidad")]
        public string Localidad { get; set; } = string.Empty;
        
        // Afiliación (opcional)
        [Column("ObraSocialId")]
        public Guid? ObraSocialId { get; set; }
        
        [ForeignKey("ObraSocialId")]
        public virtual ObraSocial? ObraSocial { get; set; }
        
        [MaxLength(50)]
        [Column("NumeroAfiliado")]
        public string? NumeroAfiliado { get; set; }
        
        // Relaciones
        public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();

        [NotMapped]
        public bool TieneObraSocial => ObraSocialId.HasValue;

        [NotMapped]
        public string NombreCompleto => Persona?.NombreCompleto ?? string.Empty;
    }
}