using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class NivelRepositorio : INivelRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public NivelRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Nivel?> ObtenerPorIdAsync(int id)
        {
            var sql = @"
                SELECT NivelId, Nombre, Color, TiempoEsperaMinutos, Prioridad
                FROM dbo.Nivel
                WHERE NivelId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<Nivel>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<Nivel?> ObtenerPorNombreAsync(string nombre)
        {
            var sql = @"
                SELECT NivelId, Nombre, Color, TiempoEsperaMinutos, Prioridad
                FROM dbo.Nivel
                WHERE Nombre = @Nombre";

            return await _connection.QueryFirstOrDefaultAsync<Nivel>(
                sql,
                new { Nombre = nombre },
                _transaction
            );
        }

        public async Task<List<Nivel>> ObtenerTodosAsync()
        {
            var sql = @"
                SELECT NivelId, Nombre, Color, TiempoEsperaMinutos, Prioridad
                FROM dbo.Nivel
                ORDER BY Nombre";

            var result = await _connection.QueryAsync<Nivel>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<List<Nivel>> ObtenerOrdenadosPorPrioridadAsync()
        {
            var sql = @"
                SELECT NivelId, Nombre, Color, TiempoEsperaMinutos, Prioridad
                FROM dbo.Nivel
                ORDER BY Prioridad ASC";

            var result = await _connection.QueryAsync<Nivel>(sql, transaction: _transaction);
            return result.ToList();
        }
    }
}