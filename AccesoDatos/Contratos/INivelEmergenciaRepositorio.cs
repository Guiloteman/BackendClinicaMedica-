using Entidades;

namespace AccesoDatos.Contratos
{
    public interface INivelEmergenciaRepositorio
    {
        Task<NivelEmergencia?> ObtenerPorIdAsync(int id);
        Task<NivelEmergencia?> ObtenerPorNombreAsync(string nombre);
        Task<NivelEmergencia?> ObtenerConNivelAsync(int id);
        Task<List<NivelEmergencia>> ObtenerTodosAsync();
        Task<List<NivelEmergencia>> ObtenerTodosConNivelAsync();
    }
}