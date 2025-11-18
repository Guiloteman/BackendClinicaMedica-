using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class ObraSocialRepositorio : IObraSocialRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public ObraSocialRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<ObraSocial?> ObtenerPorIdAsync(Guid id)
        {
            var sql = @"
                SELECT ObraSocialId, Nombre
                FROM dbo.ObraSocial
                WHERE ObraSocialId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<ObraSocial>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<ObraSocial?> ObtenerPorNombreAsync(string nombre)
        {
            var sql = @"
                SELECT ObraSocialId, Nombre
                FROM dbo.ObraSocial
                WHERE Nombre = @Nombre";

            return await _connection.QueryFirstOrDefaultAsync<ObraSocial>(
                sql,
                new { Nombre = nombre },
                _transaction
            );
        }

        public async Task<List<ObraSocial>> ObtenerTodasAsync()
        {
            var sql = @"
                SELECT ObraSocialId, Nombre
                FROM dbo.ObraSocial
                ORDER BY Nombre";

            var result = await _connection.QueryAsync<ObraSocial>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<Guid> CrearAsync(ObraSocial obraSocial)
        {
            var sql = @"
                INSERT INTO dbo.ObraSocial (ObraSocialId, Nombre)
                VALUES (@ObraSocialId, @Nombre)";

            await _connection.ExecuteAsync(sql, obraSocial, _transaction);
            return obraSocial.ObraSocialId;
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            var sql = "SELECT COUNT(1) FROM dbo.ObraSocial WHERE ObraSocialId = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { Id = id },
                _transaction
            );
            return count > 0;
        }
    }
}