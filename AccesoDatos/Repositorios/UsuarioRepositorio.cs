using AccesoDatos.Contratos;
using Dapper;
using Entidades;
using Microsoft.Data.SqlClient;

namespace AccesoDatos.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public UsuarioRepositorio(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<Usuario?> ObtenerPorIdAsync(Guid id)
        {
            var sql = @"
                SELECT UsuarioId, Email, PasswordHash, Rol, MedicoId, EnfermeraId, CreadoEn
                FROM dbo.Usuario
                WHERE UsuarioId = @Id";

            return await _connection.QueryFirstOrDefaultAsync<Usuario>(
                sql,
                new { Id = id },
                _transaction
            );
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            var sql = @"
                SELECT UsuarioId, Email, PasswordHash, Rol, MedicoId, EnfermeraId, CreadoEn
                FROM dbo.Usuario
                WHERE Email = @Email";

            return await _connection.QueryFirstOrDefaultAsync<Usuario>(
                sql,
                new { Email = email },
                _transaction
            );
        }

        public async Task<Usuario?> ObtenerConRelacionesAsync(Guid id)
        {
            var sql = @"
                SELECT u.UsuarioId, u.Email, u.PasswordHash, u.Rol, u.MedicoId, u.EnfermeraId, u.CreadoEn,
                       d.PersonaId, d.Matricula,
                       pd.PersonaId, pd.Cuil, pd.Apellido, pd.Nombre, pd.Email,
                       e.PersonaId, e.Matricula,
                       pe.PersonaId, pe.Cuil, pe.Apellido, pe.Nombre, pe.Email
                FROM dbo.Usuario u
                LEFT JOIN dbo.Doctor d ON u.MedicoId = d.PersonaId
                LEFT JOIN dbo.Persona pd ON d.PersonaId = pd.PersonaId
                LEFT JOIN dbo.Enfermera e ON u.EnfermeraId = e.PersonaId
                LEFT JOIN dbo.Persona pe ON e.PersonaId = pe.PersonaId
                WHERE u.UsuarioId = @Id";

            var usuarios = await _connection.QueryAsync<Usuario, Doctor, Persona, Enfermera, Persona, Usuario>(
                sql,
                (usuario, doctor, personaDoctor, enfermera, personaEnfermera) =>
                {
                    if (doctor != null)
                    {
                        doctor.Persona = personaDoctor;
                        usuario.Medico = doctor;
                    }
                    if (enfermera != null)
                    {
                        enfermera.Persona = personaEnfermera;
                        usuario.Enfermera = enfermera;
                    }
                    return usuario;
                },
                new { Id = id },
                _transaction,
                splitOn: "PersonaId,PersonaId,PersonaId,PersonaId"
            );

            return usuarios.FirstOrDefault();
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            var sql = @"
                SELECT UsuarioId, Email, PasswordHash, Rol, MedicoId, EnfermeraId, CreadoEn
                FROM dbo.Usuario
                ORDER BY Email";

            var result = await _connection.QueryAsync<Usuario>(sql, transaction: _transaction);
            return result.ToList();
        }

        public async Task<Guid> CrearAsync(Usuario usuario)
        {
            var sql = @"
                INSERT INTO dbo.Usuario 
                    (UsuarioId, Email, PasswordHash, Rol, MedicoId, EnfermeraId, CreadoEn)
                VALUES 
                    (@UsuarioId, @Email, @PasswordHash, @Rol, @MedicoId, @EnfermeraId, @CreadoEn)";

            await _connection.ExecuteAsync(sql, usuario, _transaction);
            return usuario.UsuarioId;
        }

        public async Task<int> ActualizarAsync(Usuario usuario)
        {
            var sql = @"
                UPDATE dbo.Usuario
                SET Email = @Email,
                    PasswordHash = @PasswordHash,
                    Rol = @Rol,
                    MedicoId = @MedicoId,
                    EnfermeraId = @EnfermeraId
                WHERE UsuarioId = @UsuarioId";

            return await _connection.ExecuteAsync(sql, usuario, _transaction);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Usuario WHERE Email = @Email";
            var count = await _connection.ExecuteScalarAsync<int>(
                sql,
                new { Email = email },
                _transaction
            );
            return count > 0;
        }
    }
}