using Entidades;

namespace AccesoDatos.Contratos
{
    public interface INivelRepositorio
    {
        Task<Nivel?> ObtenerPorIdAsync(int id);
        Task<Nivel?> ObtenerPorNombreAsync(string nombre);
        Task<List<Nivel>> ObtenerTodosAsync();
        Task<List<Nivel>> ObtenerOrdenadosPorPrioridadAsync();
    }
}