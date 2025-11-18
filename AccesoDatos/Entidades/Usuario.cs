using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entidades.Enums;

namespace Entidades
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("UsuarioId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UsuarioId { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(320)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        [Column("Rol")]
        public string Rol { get; set; } = string.Empty;
        
        [Column("MedicoId")]
        public Guid? MedicoId { get; set; }
        
        [ForeignKey("MedicoId")]
        public virtual Doctor? Medico { get; set; }
        
        [Column("EnfermeraId")]
        public Guid? EnfermeraId { get; set; }
        
        [ForeignKey("EnfermeraId")]
        public virtual Enfermera? Enfermera { get; set; }
        
        [Required]
        [Column("CreadoEn")]
        public DateTime CreadoEn { get; set; }

        public Usuario()
        {
            UsuarioId = Guid.NewGuid();
            CreadoEn = DateTime.UtcNow;
        }

        [NotMapped]
        public RolUsuario RolEnum
        {
            get => Enum.Parse<RolUsuario>(Rol);
            set => Rol = value.ToString();
        }

        [NotMapped]
        public bool EsMedico => Rol == "MEDICO";

        [NotMapped]
        public bool EsEnfermera => Rol == "ENFERMERA";
    }
}