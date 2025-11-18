using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class PacienteRepositorio : IPacienteRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public PacienteRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Paciente?> ObtenerPorIdAsync(Guid personaId)
        {
            var sql = @"
                SELECT pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad,
                       pac.ObraSocialId, pac.NumeroAfiliado
                FROM dbo.Paciente pac
                WHERE pac.PersonaId = @PersonaId";

            return await _connection.QueryFirstOrDefaultAsync<Paciente>(
                sql,
                new { PersonaId = personaId },
                _transaction
            );
        }

        public async Task<Paciente?> ObtenerPorCuilAsync(string cuil)
        {
            var sql = @"
                SELECT pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad,
                       pac.ObraSocialId, pac.NumeroAfiliado,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Paciente pac
                INNER JOIN dbo.Persona per ON pac.PersonaId = per.PersonaId
                WHERE per.Cuil = @Cuil";

            var pacientes = await _connection.QueryAsync<Paciente, Persona, Paciente>(
                sql,
                (paciente, persona) =>
                {
                    paciente.Persona = persona;
                    return paciente;
                },
                new { Cuil = cuil },
                _transaction,
                splitOn: "PersonaId"
            );

            return pacientes.FirstOrDefault();
        }

        public async Task<Paciente?> ObtenerConPersonaAsync(Guid personaId)
        {
            var sql = @"
                SELECT pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad,
                       pac.ObraSocialId, pac.NumeroAfiliado,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email,
                       os.ObraSocialId, os.Nombre
                FROM dbo.Paciente pac
                INNER JOIN dbo.Persona per ON pac.PersonaId = per.PersonaId
                LEFT JOIN dbo.ObraSocial os ON pac.ObraSocialId = os.ObraSocialId
                WHERE pac.PersonaId = @PersonaId";

            var pacientes = await _connection.QueryAsync<Paciente, Persona, ObraSocial, Paciente>(
                sql,
                (paciente, persona, obraSocial) =>
                {
                    paciente.Persona = persona;
                    paciente.ObraSocial = obraSocial;
                    return paciente;
                },
                new { PersonaId = personaId },
                _transaction,
                splitOn: "PersonaId,ObraSocialId"
            );

            return pacientes.FirstOrDefault();
        }

        public async Task<List<Paciente>> ObtenerTodosAsync()
        {
            var sql = @"
                SELECT pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad,
                       pac.ObraSocialId, pac.NumeroAfiliado,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Paciente pac
                INNER JOIN dbo.Persona per ON pac.PersonaId = per.PersonaId
                ORDER BY per.Apellido, per.Nombre";

            var pacientes = await _connection.QueryAsync<Paciente, Persona, Paciente>(
                sql,
                (paciente, persona) =>
                {
                    paciente.Persona = persona;
                    return paciente;
                },
                transaction: _transaction,
                splitOn: "PersonaId"
            );

            return pacientes.ToList();
        }

        public async Task<Guid> CrearAsync(Paciente paciente)
        {
            var sql = @"
                INSERT INTO dbo.Paciente 
                    (PersonaId, Calle, Numero, Localidad, ObraSocialId, NumeroAfiliado)
                VALUES 
                    (@PersonaId, @Calle, @Numero, @Localidad, @ObraSocialId, @NumeroAfiliado)";

            await _connection.ExecuteAsync(sql, paciente, _transaction);
            return paciente.PersonaId;
        }

        public async Task<int> ActualizarAsync(Paciente paciente)
        {
            var sql = @"
                UPDATE dbo.Paciente
                SET Calle = @Calle,
                    Numero = @Numero,
                    Localidad = @Localidad,
                    ObraSocialId = @ObraSocialId,
                    NumeroAfiliado = @NumeroAfiliado
                WHERE PersonaId = @PersonaId";

            return await _connection.ExecuteAsync(sql, paciente, _transaction);
        }

        public async Task<bool> ExisteAsync(Guid personaId)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Paciente WHERE PersonaId = @PersonaId";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { PersonaId = personaId },
                _transaction
            );
            return count > 0;
        }

        public async Task<List<Paciente>> BuscarPorNombreOApellidoAsync(string termino)
        {
            var sql = @"
                SELECT pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad,
                       pac.ObraSocialId, pac.NumeroAfiliado,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Paciente pac
                INNER JOIN dbo.Persona per ON pac.PersonaId = per.PersonaId
                WHERE per.Nombre LIKE @Termino OR per.Apellido LIKE @Termino
                ORDER BY per.Apellido, per.Nombre";

            var pacientes = await _connection.QueryAsync<Paciente, Persona, Paciente>(
                sql,
                (paciente, persona) =>
                {
                    paciente.Persona = persona;
                    return paciente;
                },
                new { Termino = $"%{termino}%" },
                _transaction,
                splitOn: "PersonaId"
            );

            return pacientes.ToList();
        }
    }
}