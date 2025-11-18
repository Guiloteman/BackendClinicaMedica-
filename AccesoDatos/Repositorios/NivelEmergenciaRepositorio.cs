using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class NivelEmergenciaRepositorio : INivelEmergenciaRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public NivelEmergenciaRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<NivelEmergencia?> ObtenerPorIdAsync(int id)
        {
            var sql = @"
                SELECT NivelEmergenciaId, Nombre, NivelId
                FROM dbo.NivelEmergencia
                WHERE NivelEmergenciaId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<NivelEmergencia>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<NivelEmergencia?> ObtenerPorNombreAsync(string nombre)
        {
            var sql = @"
                SELECT NivelEmergenciaId, Nombre, NivelId
                FROM dbo.NivelEmergencia
                WHERE Nombre = @Nombre";

            return await _connection.QueryFirstOrDefaultAsync<NivelEmergencia>(
                sql,
                new { Nombre = nombre },
                _transaction
            );
        }

        public async Task<NivelEmergencia?> ObtenerConNivelAsync(int id)
        {
            var sql = @"
                SELECT ne.NivelEmergenciaId, ne.Nombre, ne.NivelId,
                       n.NivelId, n.Nombre, n.Color, n.TiempoEsperaMinutos, n.Prioridad
                FROM dbo.NivelEmergencia ne
                INNER JOIN dbo.Nivel n ON ne.NivelId = n.NivelId
                WHERE ne.NivelEmergenciaId = @Id";

            var nivelesEmergencia = await _connection.QueryAsync<NivelEmergencia, Nivel, NivelEmergencia>(
                sql,
                (nivelEmergencia, nivel) =>
                {
                    nivelEmergencia.Nivel = nivel;
                    return nivelEmergencia;
                },
                new { Id = id },
                _transaction,
                splitOn: "NivelId"
            );

            return nivelesEmergencia.FirstOrDefault();
        }

        public async Task<List<NivelEmergencia>> ObtenerTodosAsync()
        {
            var sql = @"
                SELECT NivelEmergenciaId, Nombre, NivelId
                FROM dbo.NivelEmergencia
                ORDER BY Nombre";

            var result = await _connection.QueryAsync<NivelEmergencia>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<List<NivelEmergencia>> ObtenerTodosConNivelAsync()
        {
            var sql = @"
                SELECT ne.NivelEmergenciaId, ne.Nombre, ne.NivelId,
                       n.NivelId, n.Nombre, n.Color, n.TiempoEsperaMinutos, n.Prioridad
                FROM dbo.NivelEmergencia ne
                INNER JOIN dbo.Nivel n ON ne.NivelId = n.NivelId
                ORDER BY n.Prioridad ASC";

            var nivelesEmergencia = await _connection.QueryAsync<NivelEmergencia, Nivel, NivelEmergencia>(
                sql,
                (nivelEmergencia, nivel) =>
                {
                    nivelEmergencia.Nivel = nivel;
                    return nivelEmergencia;
                },
                transaction: _transaction,
                splitOn: "NivelId"
            );

            return nivelesEmergencia.ToList();
        }
    }
}