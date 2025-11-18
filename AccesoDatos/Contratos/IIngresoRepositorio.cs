using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IIngresoRepositorio
    {
        Task<Ingreso?> ObtenerPorIdAsync(Guid id);
        Task<Ingreso?> ObtenerConRelacionesAsync(Guid id);
        Task<List<Ingreso>> ObtenerTodosAsync();
        Task<Guid> CrearAsync(Ingreso ingreso);
        Task<int> ActualizarAsync(Ingreso ingreso);
        Task<int> CambiarEstadoAsync(Guid ingresoId, string nuevoEstado);
        
        // Cola de espera (IS2025-001, IS2025-003)
        Task<List<Ingreso>> ObtenerColaEsperaAsync();
        Task<Ingreso?> ObtenerProximoPacienteAsync();
        Task<List<Ingreso>> ObtenerPorPacienteAsync(Guid pacienteId);
        Task<List<Ingreso>> ObtenerPorEnfermeraAsync(Guid enfermeraId);
        Task<List<Ingreso>> ObtenerPorEstadoAsync(string estado);
        Task<int> ContarPendientesAsync();
    }
}