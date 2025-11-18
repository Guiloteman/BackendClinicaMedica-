using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IDoctorRepositorio
    {
        Task<Doctor?> ObtenerPorIdAsync(Guid personaId);
        Task<Doctor?> ObtenerPorMatriculaAsync(string matricula);
        Task<Doctor?> ObtenerConPersonaAsync(Guid personaId);
        Task<List<Doctor>> ObtenerTodosAsync();
        Task<Guid> CrearAsync(Doctor doctor);
        Task<int> ActualizarAsync(Doctor doctor);
        Task<bool> ExisteAsync(Guid personaId);
        Task<bool> ExisteMatriculaAsync(string matricula);
    }
}