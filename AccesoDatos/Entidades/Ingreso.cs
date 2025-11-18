using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entidades.Enums;

namespace Entidades
{
    [Table("Ingreso")]
    public class Ingreso
    {
        [Key]
        [Column("IngresoId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid IngresoId { get; set; }
        
        [Required]
        [Column("PacienteId")]
        public Guid PacienteId { get; set; }
        
        [ForeignKey("PacienteId")]
        public virtual Paciente Paciente { get; set; } = null!;
        
        [Required]
        [Column("EnfermeraId")]
        public Guid EnfermeraId { get; set; }
        
        [ForeignKey("EnfermeraId")]
        public virtual Enfermera Enfermera { get; set; } = null!;
        
        [Required]
        [Column("FechaIngreso")]
        public DateTime FechaIngreso { get; set; }
        
        [Required]
        [MaxLength(1000)]
        [Column("Informe")]
        public string Informe { get; set; } = string.Empty;
        
        [Required]
        [Column("NivelEmergenciaId")]
        public int NivelEmergenciaId { get; set; }
        
        [ForeignKey("NivelEmergenciaId")]
        public virtual NivelEmergencia NivelEmergencia { get; set; } = null!;
        
        [Required]
        [MaxLength(20)]
        [Column("Estado")]
        public string Estado { get; set; } = "PENDIENTE";
        
        // Signos vitales
        [Column("TemperaturaC", TypeName = "decimal(4,1)")]
        public decimal? TemperaturaC { get; set; }
        
        [Required]
        [Column("FrecuenciaCardiacaLpm", TypeName = "decimal(6,2)")]
        public decimal FrecuenciaCardiacaLpm { get; set; }
        
        [Required]
        [Column("FrecuenciaRespRpm", TypeName = "decimal(6,2)")]
        public decimal FrecuenciaRespRpm { get; set; }
        
        [Required]
        [Column("TensionSistolicaMmHg", TypeName = "decimal(6,2)")]
        public decimal TensionSistolicaMmHg { get; set; }
        
        [Required]
        [Column("TensionDiastolicaMmHg", TypeName = "decimal(6,2)")]
        public decimal TensionDiastolicaMmHg { get; set; }
        
        public virtual ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();

        public Ingreso()
        {
            IngresoId = Guid.NewGuid();
            FechaIngreso = DateTime.UtcNow;
            Estado = "PENDIENTE";
        }

        [NotMapped]
        public EstadoIngreso EstadoEnum
        {
            get => Enum.Parse<EstadoIngreso>(Estado);
            set => Estado = value.ToString();
        }

        [NotMapped]
        public string TensionArterialFormato => $"{TensionSistolicaMmHg}/{TensionDiastolicaMmHg}";

        [NotMapped]
        public bool EstaPendiente => Estado == "PENDIENTE";

        [NotMapped]
        public bool EstaEnProceso => Estado == "EN_PROCESO";

        [NotMapped]
        public bool EstaFinalizado => Estado == "FINALIZADO";
    }
}