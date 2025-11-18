using AccesoDatos.Contratos;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class UnitOfWorkAdapter : IUnitOfWorkAdapter
    {
        private SqlConnection _context { get; set; }
        private SqlTransaction _transaction { get; set; }
        public IUnitOfWorkRepository Repositorios { get; set; }

        public UnitOfWorkAdapter(string connectionString)
        {
            _context = new SqlConnection(connectionString);
            _context.Open();
            _transaction = _context.BeginTransaction();
            Repositorios = new UnitOfWorkRepository(_context, _transaction);
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }
            if (_context != null)
            {
                _context.Close();
                _context.Dispose();
            }
            Repositorios = null;
        }

        public async Task GuardarCambios()
        {
            await _transaction.CommitAsync();
        }

        public async Task CancelarCambios()
        {
            await _transaction.RollbackAsync();
        }
    }
}

