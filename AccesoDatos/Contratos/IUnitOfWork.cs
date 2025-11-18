namespace AccesoDatos.Contratos
{
    public interface IUnitOfWork
    {
        IUnitOfWorkAdapter Create(bool test);
    }
}
