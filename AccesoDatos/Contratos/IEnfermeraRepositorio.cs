using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IEnfermeraRepositorio
    {
        Task<Enfermera?> ObtenerPorIdAsync(Guid personaId);
        Task<Enfermera?> ObtenerPorMatriculaAsync(string matricula);
        Task<Enfermera?> ObtenerConPersonaAsync(Guid personaId);
        Task<List<Enfermera>> ObtenerTodasAsync();
        Task<Guid> CrearAsync(Enfermera enfermera);
        Task<int> ActualizarAsync(Enfermera enfermera);
        Task<bool> ExisteAsync(Guid personaId);
        Task<bool> ExisteMatriculaAsync(string matricula);
        Task ObtenerTodosAsync();
    }
}