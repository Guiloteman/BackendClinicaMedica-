using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;



namespace AccesoDatos.Repositorios
{
    public class PersonaRepositorio : IPersonaRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public PersonaRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Persona?> ObtenerPorIdAsync(Guid id)
        {
            var sql = @"
                SELECT PersonaId, Cuil, Apellido, Nombre, Email
                FROM dbo.Persona
                WHERE PersonaId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<Persona>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<Persona?> ObtenerPorCuilAsync(string cuil)
        {
            var sql = @"
                SELECT PersonaId, Cuil, Apellido, Nombre, Email
                FROM dbo.Persona
                WHERE Cuil = @Cuil";

            return await _connection.QueryFirstOrDefaultAsync<Persona>(
                sql,
                new { Cuil = cuil },
                _transaction
            );
        }

        public async Task<List<Persona>> ObtenerTodasAsync()
        {
            var sql = @"
                SELECT PersonaId, Cuil, Apellido, Nombre, Email
                FROM dbo.Persona
                ORDER BY Apellido, Nombre";

            var result = await _connection.QueryAsync<Persona>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<Guid> CrearAsync(Persona persona)
        {
            var sql = @"
                INSERT INTO dbo.Persona (PersonaId, Cuil, Apellido, Nombre, Email)
                VALUES (@PersonaId, @Cuil, @Apellido, @Nombre, @Email)";

            await _connection.ExecuteAsync(sql, persona, _transaction);
            return persona.PersonaId;
        }

        public async Task<int> ActualizarAsync(Persona persona)
        {
            var sql = @"
                UPDATE dbo.Persona
                SET Apellido = @Apellido,
                    Nombre = @Nombre,
                    Email = @Email
                WHERE PersonaId = @PersonaId";

            return await _connection.ExecuteAsync(sql, persona, _transaction);
        }

        public async Task<bool> ExisteAsync(Guid id)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Persona WHERE PersonaId = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { Id = id },
                _transaction
            );
            return count > 0;
        }

        public async Task<bool> ExisteCuilAsync(string cuil)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Persona WHERE Cuil = @Cuil";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { Cuil = cuil },
                _transaction
            );
            return count > 0;
        }
    }
}