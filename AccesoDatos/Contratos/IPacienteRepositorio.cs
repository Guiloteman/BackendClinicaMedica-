using Entidades;

namespace AccesoDatos.Contratos
{
    public interface IPacienteRepositorio
    {
        Task<Paciente?> ObtenerPorIdAsync(Guid personaId);
        Task<Paciente?> ObtenerPorCuilAsync(string cuil);
        Task<Paciente?> ObtenerConPersonaAsync(Guid personaId);
        Task<List<Paciente>> ObtenerTodosAsync();
        Task<Guid> CrearAsync(Paciente paciente);
        Task<int> ActualizarAsync(Paciente paciente);
        Task<bool> ExisteAsync(Guid personaId);
        Task<List<Paciente>> BuscarPorNombreOApellidoAsync(string termino);
    }
}