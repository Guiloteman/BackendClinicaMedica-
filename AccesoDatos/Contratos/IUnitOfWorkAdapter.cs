namespace AccesoDatos.Contratos
{
    public interface IUnitOfWorkAdapter:IDisposable
    {      
     IUnitOfWorkRepository Repositorios { get; }
     Task GuardarCambios();
     Task CancelarCambios();
    }
}
