using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class AtencionRepositorio : IAtencionRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public AtencionRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Atencion?> ObtenerPorIdAsync(Guid id)
        {
            var sql = @"
                SELECT AtencionId, IngresoId, MedicoId, Informe, FechaAtencion
                FROM dbo.Atencion
                WHERE AtencionId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<Atencion>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<Atencion?> ObtenerConRelacionesAsync(Guid id)
        {
            var sql = @"
                SELECT a.AtencionId, a.IngresoId, a.MedicoId, a.Informe, a.FechaAtencion,
                       d.PersonaId, d.Matricula,
                       pd.PersonaId, pd.Cuil, pd.Apellido, pd.Nombre, pd.Email,
                       i.IngresoId, i.PacienteId, i.EnfermeraId, i.FechaIngreso, 
                       i.NivelEmergenciaId, i.Estado,
                       pac.PersonaId,
                       ppac.PersonaId, ppac.Cuil, ppac.Apellido, ppac.Nombre
                FROM dbo.Atencion a
                INNER JOIN dbo.Doctor d ON a.MedicoId = d.PersonaId
                INNER JOIN dbo.Persona pd ON d.PersonaId = pd.PersonaId
                INNER JOIN dbo.Ingreso i ON a.IngresoId = i.IngresoId
                INNER JOIN dbo.Paciente pac ON i.PacienteId = pac.PersonaId
                INNER JOIN dbo.Persona ppac ON pac.PersonaId = ppac.PersonaId
                WHERE a.AtencionId = @Id";

            var atenciones = await _connection.QueryAsync<Atencion, Doctor, Persona, Ingreso, Paciente, Persona, Atencion>(
                sql,
                (atencion, doctor, personaDoctor, ingreso, paciente, personaPaciente) =>
                {
                    doctor.Persona = personaDoctor;
                    atencion.Medico = doctor;
                    
                    paciente.Persona = personaPaciente;
                    ingreso.Paciente = paciente;
                    atencion.Ingreso = ingreso;
                    
                    return atencion;
                },
                new { Id = id },
                _transaction,
                splitOn: "PersonaId,PersonaId,IngresoId,PersonaId,PersonaId"
            );

            return atenciones.FirstOrDefault();
        }

        public async Task<Atencion?> ObtenerPorIngresoAsync(Guid ingresoId)
        {
            var sql = @"
                SELECT AtencionId, IngresoId, MedicoId, Informe, FechaAtencion
                FROM dbo.Atencion
                WHERE IngresoId = @IngresoId";

            return await _connection.QueryFirstOrDefaultAsync<Atencion>(
                sql,
                new { IngresoId = ingresoId },
                _transaction
            );
        }

        public async Task<List<Atencion>> ObtenerTodasAsync()
        {
            var sql = @"
                SELECT AtencionId, IngresoId, MedicoId, Informe, FechaAtencion
                FROM dbo.Atencion
                ORDER BY FechaAtencion DESC";

            var result = await _connection.QueryAsync<Atencion>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<Guid> CrearAsync(Atencion atencion)
        {
            var sql = @"
                INSERT INTO dbo.Atencion 
                    (AtencionId, IngresoId, MedicoId, Informe, FechaAtencion)
                VALUES 
                    (@AtencionId, @IngresoId, @MedicoId, @Informe, @FechaAtencion)";

            await _connection.ExecuteAsync(sql, atencion, _transaction);
            return atencion.AtencionId;
        }

        public async Task<List<Atencion>> ObtenerPorMedicoAsync(Guid medicoId)
        {
            var sql = @"
                SELECT a.AtencionId, a.IngresoId, a.MedicoId, a.Informe, a.FechaAtencion,
                       i.IngresoId, i.PacienteId, i.FechaIngreso, i.Estado,
                       pac.PersonaId,
                       ppac.PersonaId, ppac.Apellido, ppac.Nombre
                FROM dbo.Atencion a
                INNER JOIN dbo.Ingreso i ON a.IngresoId = i.IngresoId
                INNER JOIN dbo.Paciente pac ON i.PacienteId = pac.PersonaId
                INNER JOIN dbo.Persona ppac ON pac.PersonaId = ppac.PersonaId
                WHERE a.MedicoId = @MedicoId
                ORDER BY a.FechaAtencion DESC";

            var atenciones = await _connection.QueryAsync<Atencion, Ingreso, Paciente, Persona, Atencion>(
                sql,
                (atencion, ingreso, paciente, personaPaciente) =>
                {
                    paciente.Persona = personaPaciente;
                    ingreso.Paciente = paciente;
                    atencion.Ingreso = ingreso;
                    return atencion;
                },
                new { MedicoId = medicoId },
                _transaction,
                splitOn: "IngresoId,PersonaId,PersonaId"
            );

            return atenciones.ToList();
        }

        public async Task<List<Atencion>> ObtenerPorPacienteAsync(Guid pacienteId)
        {
            var sql = @"
                SELECT a.AtencionId, a.IngresoId, a.MedicoId, a.Informe, a.FechaAtencion,
                       d.PersonaId, d.Matricula,
                       pd.PersonaId, pd.Apellido, pd.Nombre
                FROM dbo.Atencion a
                INNER JOIN dbo.Ingreso i ON a.IngresoId = i.IngresoId
                INNER JOIN dbo.Doctor d ON a.MedicoId = d.PersonaId
                INNER JOIN dbo.Persona pd ON d.PersonaId = pd.PersonaId
                WHERE i.PacienteId = @PacienteId
                ORDER BY a.FechaAtencion DESC";

            var atenciones = await _connection.QueryAsync<Atencion, Doctor, Persona, Atencion>(
                sql,
                (atencion, doctor, personaDoctor) =>
                {
                    doctor.Persona = personaDoctor;
                    atencion.Medico = doctor;
                    return atencion;
                },
                new { PacienteId = pacienteId },
                _transaction,
                splitOn: "PersonaId,PersonaId"
            );

            return atenciones.ToList();
        }

        public async Task<bool> ExisteAtencionParaIngresoAsync(Guid ingresoId)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM dbo.Atencion
                WHERE IngresoId = @IngresoId";

            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { IngresoId = ingresoId },
                _transaction
            );

            return count > 0;
        }
    }
}