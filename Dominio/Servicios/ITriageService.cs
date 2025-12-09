using Dominio.DTOs;
using Entidades;

namespace Dominio.Servicios
{
    /// <summary>
    /// Servicio de dominio para gestionar el triage y cola de espera de urgencias (IS2025-001)
    /// </summary>
    public interface ITriageService
    {
        /// <summary>
        /// Registra un nuevo ingreso a urgencias (IS2025-001)
        /// </summary>
        Task<ResultadoOperacion<Ingreso>> RegistrarIngresoAsync(RegistrarIngresoDto dto);
        
        /// <summary>
        /// Obtiene la cola de espera ordenada por prioridad (IS2025-001)
        /// </summary>
        Task<ResultadoOperacion<List<ColaEsperaDto>>> ObtenerColaEsperaAsync();
        
        /// <summary>
        /// Reclama el próximo paciente a atender (IS2025-003)
        /// </summary>
        Task<ResultadoOperacion<Ingreso>> ReclamarProximoPacienteAsync(Guid medicoId);
        
        /// <summary>
        /// Calcula la posición de un ingreso en la cola
        /// </summary>
        Task<ResultadoOperacion<int>> CalcularPosicionEnColaAsync(Guid ingresoId);
        
        /// <summary>
        /// Valida si un ingreso supera el tiempo máximo de espera
        /// </summary>
        Task<ResultadoOperacion<List<Ingreso>>> ObtenerIngresosConTiempoExcedidoAsync();
    }
}