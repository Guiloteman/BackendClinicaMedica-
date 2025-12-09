using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class EnfermeraRepositorio : IEnfermeraRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public EnfermeraRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Enfermera?> ObtenerPorIdAsync(Guid personaId)
        {
            var sql = @"
                SELECT PersonaId, Matricula
                FROM dbo.Enfermera
                WHERE PersonaId = @PersonaId";

            return await _connection.QueryFirstOrDefaultAsync<Enfermera>(
                sql,
                new { PersonaId = personaId },
                _transaction
            );
        }

        public async Task<Enfermera?> ObtenerPorMatriculaAsync(string matricula)
        {
            var sql = @"
                SELECT e.PersonaId, e.Matricula,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Enfermera e
                INNER JOIN dbo.Persona per ON e.PersonaId = per.PersonaId
                WHERE e.Matricula = @Matricula";

            var enfermeras = await _connection.QueryAsync<Enfermera, Persona, Enfermera>(
                sql,
                (enfermera, persona) =>
                {
                    enfermera.Persona = persona;
                    return enfermera;
                },
                new { Matricula = matricula },
                _transaction,
                splitOn: "PersonaId"
            );

            return enfermeras.FirstOrDefault();
        }

        public async Task<Enfermera?> ObtenerConPersonaAsync(Guid personaId)
        {
            var sql = @"
                SELECT e.PersonaId, e.Matricula,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Enfermera e
                INNER JOIN dbo.Persona per ON e.PersonaId = per.PersonaId
                WHERE e.PersonaId = @PersonaId";

            var enfermeras = await _connection.QueryAsync<Enfermera, Persona, Enfermera>(
                sql,
                (enfermera, persona) =>
                {
                    enfermera.Persona = persona;
                    return enfermera;
                },
                new { PersonaId = personaId },
                _transaction,
                splitOn: "PersonaId"
            );

            return enfermeras.FirstOrDefault();
        }

        public async Task<List<Enfermera>> ObtenerTodasAsync()
        {
            var sql = @"
                SELECT e.PersonaId, e.Matricula,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Enfermera e
                INNER JOIN dbo.Persona per ON e.PersonaId = per.PersonaId
                ORDER BY per.Apellido, per.Nombre";

            var enfermeras = await _connection.QueryAsync<Enfermera, Persona, Enfermera>(
                sql,
                (enfermera, persona) =>
                {
                    enfermera.Persona = persona;
                    return enfermera;
                },
                transaction: _transaction,
                splitOn: "PersonaId"
            );

            return enfermeras.ToList();
        }

        public async Task<Guid> CrearAsync(Enfermera enfermera)
        {
            var sql = @"
                INSERT INTO dbo.Enfermera (PersonaId, Matricula)
                VALUES (@PersonaId, @Matricula)";

            await _connection.ExecuteAsync(sql, enfermera, _transaction);
            return enfermera.PersonaId;
        }

        public async Task<int> ActualizarAsync(Enfermera enfermera)
        {
            var sql = @"
                UPDATE dbo.Enfermera
                SET Matricula = @Matricula
                WHERE PersonaId = @PersonaId";

            return await _connection.ExecuteAsync(sql, enfermera, _transaction);
        }

        public async Task<bool> ExisteAsync(Guid personaId)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Enfermera WHERE PersonaId = @PersonaId";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { PersonaId = personaId },
                _transaction
            );
            return count > 0;
        }

        public async Task<bool> ExisteMatriculaAsync(string matricula)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Enfermera WHERE Matricula = @Matricula";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { Matricula = matricula },
                _transaction
            );
            return count > 0;
        }

        public Task ObtenerTodosAsync()
        {
            throw new NotImplementedException();
        }
    }
}