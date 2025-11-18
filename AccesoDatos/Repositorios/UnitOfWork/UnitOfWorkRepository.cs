using AccesoDatos.Contratos;
using Microsoft.Data.SqlClient;



namespace AccesoDatos.Repositorios
{
    public class UnitOfWorkRepository : IUnitOfWorkRepository
    {
        
        public IPersonaRepositorio PersonaRepositorio { get; }
        public IPacienteRepositorio PacienteRepositorio { get; }
        public IDoctorRepositorio DoctorRepositorio { get; }
        public IEnfermeraRepositorio EnfermeraRepositorio { get; }
        public IUsuarioRepositorio UsuarioRepositorio { get; }
        public INivelRepositorio NivelRepositorio { get; }
        public INivelEmergenciaRepositorio NivelEmergenciaRepositorio { get; }
        public IObraSocialRepositorio ObraSocialRepositorio { get; }
        public IIngresoRepositorio IngresoRepositorio { get; }
        public IAtencionRepositorio AtencionRepositorio { get; }

        public UnitOfWorkRepository(SqlConnection context, SqlTransaction transaction)
        {            
            // Inicializar repositorios del sistema de urgencias
            PersonaRepositorio = new PersonaRepositorio(context, transaction);
            PacienteRepositorio = new PacienteRepositorio(context, transaction);
            DoctorRepositorio = new DoctorRepositorio(context, transaction);
            EnfermeraRepositorio = new EnfermeraRepositorio(context, transaction);
            UsuarioRepositorio = new UsuarioRepositorio(context, transaction);
            NivelRepositorio = new NivelRepositorio(context, transaction);
            NivelEmergenciaRepositorio = new NivelEmergenciaRepositorio(context, transaction);
            ObraSocialRepositorio = new ObraSocialRepositorio(context, transaction);
            IngresoRepositorio = new IngresoRepositorio(context, transaction);
            AtencionRepositorio = new AtencionRepositorio(context, transaction);
        }
    }
}

