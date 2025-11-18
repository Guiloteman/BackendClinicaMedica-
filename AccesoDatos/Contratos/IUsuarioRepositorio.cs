using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorIdAsync(Guid id);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<Usuario?> ObtenerConRelacionesAsync(Guid id);
        Task<List<Usuario>> ObtenerTodosAsync();
        Task<Guid> CrearAsync(Usuario usuario);
        Task<int> ActualizarAsync(Usuario usuario);
        Task<bool> ExisteEmailAsync(string email);
    }
}