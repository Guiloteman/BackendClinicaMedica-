namespace Dominio.DTOs
{
    /// <summary>
    /// DTO que representa un paciente en la cola de espera
    /// </summary>
    public class ColaEsperaDto
    {
        public Guid IngresoId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int PosicionEnCola { get; set; }
        
        // Paciente
        public Guid PacienteId { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string ApellidoPaciente { get; set; } = string.Empty;
        public string CuilPaciente { get; set; } = string.Empty;
        
        // Nivel de emergencia
        public int NivelEmergenciaId { get; set; }
        public string NombreNivelEmergencia { get; set; } = string.Empty;
        public string ColorNivel { get; set; } = string.Empty;
        public int PrioridadNivel { get; set; }
        public int TiempoEsperaMaximoMinutos { get; set; }
        
        // Signos vitales
        public decimal? TemperaturaC { get; set; }
        public decimal FrecuenciaCardiacaLpm { get; set; }
        public decimal FrecuenciaRespiratoriaRpm { get; set; }
        public string TensionArterial { get; set; } = string.Empty;
        
        // Tiempo de espera actual
        public TimeSpan TiempoEsperaActual { get; set; }
        public bool SuperaTiempoMaximo { get; set; }
    }
}