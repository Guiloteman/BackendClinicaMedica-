namespace AccesoDatos.Contratos
{
    public interface IUnitOfWorkRepository
    {
        // Repositorios existentes
       
        // Repositorios del sistema de urgencias
        IPersonaRepositorio PersonaRepositorio { get; }
        IPacienteRepositorio PacienteRepositorio { get; }
        IDoctorRepositorio DoctorRepositorio { get; }
        IEnfermeraRepositorio EnfermeraRepositorio { get; }
        IUsuarioRepositorio UsuarioRepositorio { get; }
        INivelRepositorio NivelRepositorio { get; }
        INivelEmergenciaRepositorio NivelEmergenciaRepositorio { get; }
        IObraSocialRepositorio ObraSocialRepositorio { get; }
        IIngresoRepositorio IngresoRepositorio { get; }
        IAtencionRepositorio AtencionRepositorio { get; }
    }
}
