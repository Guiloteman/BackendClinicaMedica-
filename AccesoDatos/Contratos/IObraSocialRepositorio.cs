using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IObraSocialRepositorio
    {
        Task<ObraSocial?> ObtenerPorIdAsync(Guid id);
        Task<ObraSocial?> ObtenerPorNombreAsync(string nombre);
        Task<List<ObraSocial>> ObtenerTodasAsync();
        Task<Guid> CrearAsync(ObraSocial obraSocial);
        Task<bool> ExisteAsync(Guid id);
    }
}