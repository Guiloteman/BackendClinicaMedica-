namespace AccesoDatos.Contratos
{
    public interface IRepositorioGenerico<Entity> where Entity : class
    {
        Task<Entity> TraerIdAsync(int id);
        Task<int> AgregarAsync(Entity entity);
        Task<int> EditarAsync(Entity entity);
        Task<int> EliminarAsync(int id);
        Task<List<Entity>> TraerTodosAsync();

    }
}
