namespace Dominio.DTOs
{
    /// <summary>
    /// DTO para registrar un nuevo ingreso a urgencias (IS2025-001)
    /// </summary>
    public class RegistrarIngresoDto
    {
        // Identificación del paciente
        public string CuilPaciente { get; set; } = string.Empty;
        
        // Identificación de la enfermera que registra
        public Guid EnfermeraId { get; set; }
        
        // Datos del ingreso
        public string Informe { get; set; } = string.Empty;
        public int NivelEmergenciaId { get; set; }
        
        // Signos vitales
        public decimal? TemperaturaC { get; set; }
        public decimal FrecuenciaCardiacaLpm { get; set; }
        public decimal FrecuenciaRespiratoriaRpm { get; set; }
        public decimal TensionSistolicaMmHg { get; set; }
        public decimal TensionDiastolicaMmHg { get; set; }
    }
}