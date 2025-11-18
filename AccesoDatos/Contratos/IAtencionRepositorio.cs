using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IAtencionRepositorio
    {
        Task<Atencion?> ObtenerPorIdAsync(Guid id);
        Task<Atencion?> ObtenerConRelacionesAsync(Guid id);
        Task<Atencion?> ObtenerPorIngresoAsync(Guid ingresoId);
        Task<List<Atencion>> ObtenerTodasAsync();
        Task<Guid> CrearAsync(Atencion atencion);
        Task<List<Atencion>> ObtenerPorMedicoAsync(Guid medicoId);
        Task<List<Atencion>> ObtenerPorPacienteAsync(Guid pacienteId);
        Task<bool> ExisteAtencionParaIngresoAsync(Guid ingresoId);
    }
}