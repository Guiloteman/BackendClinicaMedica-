using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class DoctorRepositorio : IDoctorRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public DoctorRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Doctor?> ObtenerPorIdAsync(Guid personaId)
        {
            var sql = @"
                SELECT PersonaId, Matricula
                FROM dbo.Doctor
                WHERE PersonaId = @PersonaId";

            return await _connection.QueryFirstOrDefaultAsync<Doctor>(
                sql,
                new { PersonaId = personaId },
                _transaction
            );
        }

        public async Task<Doctor?> ObtenerPorMatriculaAsync(string matricula)
        {
            var sql = @"
                SELECT d.PersonaId, d.Matricula,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Doctor d
                INNER JOIN dbo.Persona per ON d.PersonaId = per.PersonaId
                WHERE d.Matricula = @Matricula";

            var doctores = await _connection.QueryAsync<Doctor, Persona, Doctor>(
                sql,
                (doctor, persona) =>
                {
                    doctor.Persona = persona;
                    return doctor;
                },
                new { Matricula = matricula },
                _transaction,
                splitOn: "PersonaId"
            );

            return doctores.FirstOrDefault();
        }

        public async Task<Doctor?> ObtenerConPersonaAsync(Guid personaId)
        {
            var sql = @"
                SELECT d.PersonaId, d.Matricula,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Doctor d
                INNER JOIN dbo.Persona per ON d.PersonaId = per.PersonaId
                WHERE d.PersonaId = @PersonaId";

            var doctores = await _connection.QueryAsync<Doctor, Persona, Doctor>(
                sql,
                (doctor, persona) =>
                {
                    doctor.Persona = persona;
                    return doctor;
                },
                new { PersonaId = personaId },
                _transaction,
                splitOn: "PersonaId"
            );

            return doctores.FirstOrDefault();
        }

        public async Task<List<Doctor>> ObtenerTodosAsync()
        {
            var sql = @"
                SELECT d.PersonaId, d.Matricula,
                       per.PersonaId, per.Cuil, per.Apellido, per.Nombre, per.Email
                FROM dbo.Doctor d
                INNER JOIN dbo.Persona per ON d.PersonaId = per.PersonaId
                ORDER BY per.Apellido, per.Nombre";

            var doctores = await _connection.QueryAsync<Doctor, Persona, Doctor>(
                sql,
                (doctor, persona) =>
                {
                    doctor.Persona = persona;
                    return doctor;
                },
                transaction: _transaction,
                splitOn: "PersonaId"
            );

            return doctores.ToList();
        }

        public async Task<Guid> CrearAsync(Doctor doctor)
        {
            var sql = @"
                INSERT INTO dbo.Doctor (PersonaId, Matricula)
                VALUES (@PersonaId, @Matricula)";

            await _connection.ExecuteAsync(sql, doctor, _transaction);
            return doctor.PersonaId;
        }

        public async Task<int> ActualizarAsync(Doctor doctor)
        {
            var sql = @"
                UPDATE dbo.Doctor
                SET Matricula = @Matricula
                WHERE PersonaId = @PersonaId";

            return await _connection.ExecuteAsync(sql, doctor, _transaction);
        }

        public async Task<bool> ExisteAsync(Guid personaId)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Doctor WHERE PersonaId = @PersonaId";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { PersonaId = personaId },
                _transaction
            );
            return count > 0;
        }

        public async Task<bool> ExisteMatriculaAsync(string matricula)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Doctor WHERE Matricula = @Matricula";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { Matricula = matricula },
                _transaction
            );
            return count > 0;
        }
    }
}