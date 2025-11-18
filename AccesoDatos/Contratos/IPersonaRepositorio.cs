using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IPersonaRepositorio
    {
        Task<Persona?> ObtenerPorIdAsync(Guid id);
        Task<Persona?> ObtenerPorCuilAsync(string cuil);
        Task<List<Persona>> ObtenerTodasAsync();
        Task<Guid> CrearAsync(Persona persona);
        Task<int> ActualizarAsync(Persona persona);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteCuilAsync(string cuil);
    }
}   