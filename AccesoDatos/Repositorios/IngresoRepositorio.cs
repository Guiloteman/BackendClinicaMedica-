using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class IngresoRepositorio : IIngresoRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public IngresoRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Ingreso?> ObtenerPorIdAsync(Guid id)
        {
            var sql = @"
                SELECT IngresoId, PacienteId, EnfermeraId, FechaIngreso, Informe,
                       NivelEmergenciaId, Estado, TemperaturaC, FrecuenciaCardiacaLpm,
                       FrecuenciaRespRpm, TensionSistolicaMmHg, TensionDiastolicaMmHg
                FROM dbo.Ingreso
                WHERE IngresoId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<Ingreso>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<Ingreso?> ObtenerConRelacionesAsync(Guid id)
        {
            var sql = @"
                SELECT i.IngresoId, i.PacienteId, i.EnfermeraId, i.FechaIngreso, i.Informe,
                       i.NivelEmergenciaId, i.Estado, i.TemperaturaC, i.FrecuenciaCardiacaLpm,
                       i.FrecuenciaRespRpm, i.TensionSistolicaMmHg, i.TensionDiastolicaMmHg,
                       pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad, pac.ObraSocialId, pac.NumeroAfiliado,
                       ppac.PersonaId, ppac.Cuil, ppac.Apellido, ppac.Nombre, ppac.Email,
                       enf.PersonaId, enf.Matricula,
                       penf.PersonaId, penf.Cuil, penf.Apellido, penf.Nombre, penf.Email,
                       ne.NivelEmergenciaId, ne.Nombre, ne.NivelId,
                       n.NivelId, n.Nombre, n.Color, n.TiempoEsperaMinutos, n.Prioridad
                FROM dbo.Ingreso i
                INNER JOIN dbo.Paciente pac ON i.PacienteId = pac.PersonaId
                INNER JOIN dbo.Persona ppac ON pac.PersonaId = ppac.PersonaId
                INNER JOIN dbo.Enfermera enf ON i.EnfermeraId = enf.PersonaId
                INNER JOIN dbo.Persona penf ON enf.PersonaId = penf.PersonaId
                INNER JOIN dbo.NivelEmergencia ne ON i.NivelEmergenciaId = ne.NivelEmergenciaId
                INNER JOIN dbo.Nivel n ON ne.NivelId = n.NivelId
                WHERE i.IngresoId = @Id";

            var ingresos = await _connection.QueryAsync<Ingreso, Paciente, Persona, Enfermera, Persona, NivelEmergencia, Nivel, Ingreso>(
                sql,
                (ingreso, paciente, personaPaciente, enfermera, personaEnfermera, nivelEmergencia, nivel) =>
                {
                    paciente.Persona = personaPaciente;
                    ingreso.Paciente = paciente;
                    
                    enfermera.Persona = personaEnfermera;
                    ingreso.Enfermera = enfermera;
                    
                    nivelEmergencia.Nivel = nivel;
                    ingreso.NivelEmergencia = nivelEmergencia;
                    
                    return ingreso;
                },
                new { Id = id },
                _transaction,
                splitOn: "PersonaId,PersonaId,PersonaId,PersonaId,NivelEmergenciaId,NivelId"
            );

            return ingresos.FirstOrDefault();
        }

        public async Task<List<Ingreso>> ObtenerTodosAsync()
        {
            var sql = @"
                SELECT IngresoId, PacienteId, EnfermeraId, FechaIngreso, Informe,
                       NivelEmergenciaId, Estado, TemperaturaC, FrecuenciaCardiacaLpm,
                       FrecuenciaRespRpm, TensionSistolicaMmHg, TensionDiastolicaMmHg
                FROM dbo.Ingreso
                ORDER BY FechaIngreso DESC";

            var result = await _connection.QueryAsync<Ingreso>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<Guid> CrearAsync(Ingreso ingreso)
        {
            var sql = @"
                INSERT INTO dbo.Ingreso 
                    (IngresoId, PacienteId, EnfermeraId, FechaIngreso, Informe,
                     NivelEmergenciaId, Estado, TemperaturaC, FrecuenciaCardiacaLpm,
                     FrecuenciaRespRpm, TensionSistolicaMmHg, TensionDiastolicaMmHg)
                VALUES 
                    (@IngresoId, @PacienteId, @EnfermeraId, @FechaIngreso, @Informe,
                     @NivelEmergenciaId, @Estado, @TemperaturaC, @FrecuenciaCardiacaLpm,
                     @FrecuenciaRespRpm, @TensionSistolicaMmHg, @TensionDiastolicaMmHg)";

            await _connection.ExecuteAsync(sql, ingreso, _transaction);
            return ingreso.IngresoId;
        }

        public async Task<int> ActualizarAsync(Ingreso ingreso)
        {
            var sql = @"
                UPDATE dbo.Ingreso
                SET Estado = @Estado,
                    Informe = @Informe,
                    TemperaturaC = @TemperaturaC,
                    FrecuenciaCardiacaLpm = @FrecuenciaCardiacaLpm,
                    FrecuenciaRespRpm = @FrecuenciaRespRpm,
                    TensionSistolicaMmHg = @TensionSistolicaMmHg,
                    TensionDiastolicaMmHg = @TensionDiastolicaMmHg
                WHERE IngresoId = @IngresoId";

            return await _connection.ExecuteAsync(sql, ingreso, _transaction);
        }

        public async Task<int> CambiarEstadoAsync(Guid ingresoId, string nuevoEstado)
        {
            var sql = @"
                UPDATE dbo.Ingreso
                SET Estado = @NuevoEstado
                WHERE IngresoId = @IngresoId";

            return await _connection.ExecuteAsync(
                sql,
                new { IngresoId = ingresoId, NuevoEstado = nuevoEstado },
                _transaction
            );
        }

        // ====== MÉTODOS PARA COLA DE ESPERA (IS2025-001, IS2025-003) ======
        
        public async Task<List<Ingreso>> ObtenerColaEsperaAsync()
        {
            var sql = @"
                SELECT i.IngresoId, i.PacienteId, i.EnfermeraId, i.FechaIngreso, i.Informe,
                       i.NivelEmergenciaId, i.Estado, i.TemperaturaC, i.FrecuenciaCardiacaLpm,
                       i.FrecuenciaRespRpm, i.TensionSistolicaMmHg, i.TensionDiastolicaMmHg,
                       pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad, pac.ObraSocialId, pac.NumeroAfiliado,
                       ppac.PersonaId, ppac.Cuil, ppac.Apellido, ppac.Nombre, ppac.Email,
                       ne.NivelEmergenciaId, ne.Nombre, ne.NivelId,
                       n.NivelId, n.Nombre, n.Color, n.TiempoEsperaMinutos, n.Prioridad
                FROM dbo.Ingreso i
                INNER JOIN dbo.Paciente pac ON i.PacienteId = pac.PersonaId
                INNER JOIN dbo.Persona ppac ON pac.PersonaId = ppac.PersonaId
                INNER JOIN dbo.NivelEmergencia ne ON i.NivelEmergenciaId = ne.NivelEmergenciaId
                INNER JOIN dbo.Nivel n ON ne.NivelId = n.NivelId
                WHERE i.Estado = N'PENDIENTE'
                ORDER BY n.Prioridad ASC, i.FechaIngreso ASC";

            var ingresos = await _connection.QueryAsync<Ingreso, Paciente, Persona, NivelEmergencia, Nivel, Ingreso>(
                sql,
                (ingreso, paciente, personaPaciente, nivelEmergencia, nivel) =>
                {
                    paciente.Persona = personaPaciente;
                    ingreso.Paciente = paciente;
                    
                    nivelEmergencia.Nivel = nivel;
                    ingreso.NivelEmergencia = nivelEmergencia;
                    
                    return ingreso;
                },
                transaction: _transaction,
                splitOn: "PersonaId,PersonaId,NivelEmergenciaId,NivelId"
            );

            return ingresos.ToList();
        }

        public async Task<Ingreso?> ObtenerProximoPacienteAsync()
        {
            // Usa UPDLOCK y READPAST para evitar condiciones de carrera (IS2025-003)
            var sql = @"
                SELECT TOP 1 
                       i.IngresoId, i.PacienteId, i.EnfermeraId, i.FechaIngreso, i.Informe,
                       i.NivelEmergenciaId, i.Estado, i.TemperaturaC, i.FrecuenciaCardiacaLpm,
                       i.FrecuenciaRespRpm, i.TensionSistolicaMmHg, i.TensionDiastolicaMmHg,
                       pac.PersonaId, pac.Calle, pac.Numero, pac.Localidad, pac.ObraSocialId, pac.NumeroAfiliado,
                       ppac.PersonaId, ppac.Cuil, ppac.Apellido, ppac.Nombre, ppac.Email,
                       ne.NivelEmergenciaId, ne.Nombre, ne.NivelId,
                       n.NivelId, n.Nombre, n.Color, n.TiempoEsperaMinutos, n.Prioridad
                FROM dbo.Ingreso i WITH (UPDLOCK, READPAST, ROWLOCK)
                INNER JOIN dbo.Paciente pac ON i.PacienteId = pac.PersonaId
                INNER JOIN dbo.Persona ppac ON pac.PersonaId = ppac.PersonaId
                INNER JOIN dbo.NivelEmergencia ne ON i.NivelEmergenciaId = ne.NivelEmergenciaId
                INNER JOIN dbo.Nivel n ON ne.NivelId = n.NivelId
                WHERE i.Estado = N'PENDIENTE'
                ORDER BY n.Prioridad ASC, i.FechaIngreso ASC";

            var ingresos = await _connection.QueryAsync<Ingreso, Paciente, Persona, NivelEmergencia, Nivel, Ingreso>(
                sql,
                (ingreso, paciente, personaPaciente, nivelEmergencia, nivel) =>
                {
                    paciente.Persona = personaPaciente;
                    ingreso.Paciente = paciente;
                    
                    nivelEmergencia.Nivel = nivel;
                    ingreso.NivelEmergencia = nivelEmergencia;
                    
                    return ingreso;
                },
                transaction: _transaction,
                splitOn: "PersonaId,PersonaId,NivelEmergenciaId,NivelId"
            );

            return ingresos.FirstOrDefault();
        }

        public async Task<List<Ingreso>> ObtenerPorPacienteAsync(Guid pacienteId)
        {
            var sql = @"
                SELECT IngresoId, PacienteId, EnfermeraId, FechaIngreso, Informe,
                       NivelEmergenciaId, Estado, TemperaturaC, FrecuenciaCardiacaLpm,
                       FrecuenciaRespRpm, TensionSistolicaMmHg, TensionDiastolicaMmHg
                FROM dbo.Ingreso
                WHERE PacienteId = @PacienteId
                ORDER BY FechaIngreso DESC";

            var result = await _connection.QueryAsync<Ingreso>(
                sql,
                new { PacienteId = pacienteId },
                _transaction
            );

            return result.ToList();
        }

        public async Task<List<Ingreso>> ObtenerPorEnfermeraAsync(Guid enfermeraId)
        {
            var sql = @"
                SELECT IngresoId, PacienteId, EnfermeraId, FechaIngreso, Informe,
                       NivelEmergenciaId, Estado, TemperaturaC, FrecuenciaCardiacaLpm,
                       FrecuenciaRespRpm, TensionSistolicaMmHg, TensionDiastolicaMmHg
                FROM dbo.Ingreso
                WHERE EnfermeraId = @EnfermeraId
                ORDER BY FechaIngreso DESC";

            var result = await _connection.QueryAsync<Ingreso>(
                sql,
                new { EnfermeraId = enfermeraId },
                _transaction
            );

            return result.ToList();
        }

        public async Task<List<Ingreso>> ObtenerPorEstadoAsync(string estado)
        {
            var sql = @"
                SELECT IngresoId, PacienteId, EnfermeraId, FechaIngreso, Informe,
                       NivelEmergenciaId, Estado, TemperaturaC, FrecuenciaCardiacaLpm,
                       FrecuenciaRespRpm, TensionSistolicaMmHg, TensionDiastolicaMmHg
                FROM dbo.Ingreso
                WHERE Estado = @Estado
                ORDER BY FechaIngreso DESC";

            var result = await _connection.QueryAsync<Ingreso>(
                sql,
                new { Estado = estado },
                _transaction
            );

            return result.ToList();
        }

        public async Task<int> ContarPendientesAsync()
        {
            var sql = @"
                SELECT COUNT(1)
                FROM dbo.Ingreso
                WHERE Estado = N'PENDIENTE'";

            return await _connection.ExecuteScalarAsync<int>(sql, transaction: _transaction);
        }
    }
}