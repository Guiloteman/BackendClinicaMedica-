using AccesoDatos.Contratos;
using Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PacientesController> _logger;

        public PacientesController(
            IUnitOfWork unitOfWork, 
            IWebHostEnvironment environment,
            ILogger<PacientesController> logger)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un paciente por su CUIL
        /// </summary>
        [HttpGet("cuil/{cuil}")]
        [ProducesResponseType(typeof(Paciente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Paciente>> ObtenerPorCuil(string cuil)
        {
            using var uow = _unitOfWork.Create(_environment.IsDevelopment());
            
            try
            {
                var paciente = await uow.Repositorios.PacienteRepositorio.ObtenerPorCuilAsync(cuil);
                
                if (paciente == null)
                {
                    return NotFound(new { mensaje = $"No se encontró un paciente con CUIL {cuil}" });
                }

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente por CUIL {Cuil}", cuil);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los pacientes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Paciente>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Paciente>>> ObtenerTodos()
        {
            using var uow = _unitOfWork.Create(_environment.IsDevelopment());
            
            try
            {
                var pacientes = await uow.Repositorios.PacienteRepositorio.ObtenerTodosAsync();
                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pacientes");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo paciente (IS2025-002)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Paciente), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Paciente>> Crear([FromBody] CrearPacienteDto dto)
        {
            using var uow = _unitOfWork.Create(_environment.IsDevelopment());
            
            try
            {
                // 1. Validar que el CUIL no exista
                var existente = await uow.Repositorios.PersonaRepositorio.ExisteCuilAsync(dto.Cuil);
                if (existente)
                {
                    return BadRequest(new { mensaje = "Ya existe una persona con ese CUIL" });
                }

                // 2. Validar obra social si aplica
                if (dto.ObraSocialId.HasValue)
                {
                    var obraSocialExiste = await uow.Repositorios.ObraSocialRepositorio.ExisteAsync(dto.ObraSocialId.Value);
                    if (!obraSocialExiste)
                    {
                        return BadRequest(new { mensaje = "La obra social especificada no existe" });
                    }

                    if (string.IsNullOrWhiteSpace(dto.NumeroAfiliado))
                    {
                        return BadRequest(new { mensaje = "Debe proporcionar el número de afiliado" });
                    }
                }

                // 3. Crear Persona
                var persona = new Persona
                {
                    Cuil = dto.Cuil,
                    Apellido = dto.Apellido,
                    Nombre = dto.Nombre,
                    Email = dto.Email
                };
                await uow.Repositorios.PersonaRepositorio.CrearAsync(persona);

                // 4. Crear Paciente
                var paciente = new Paciente
                {
                    PersonaId = persona.PersonaId,
                    Calle = dto.Calle,
                    Numero = dto.Numero,
                    Localidad = dto.Localidad,
                    ObraSocialId = dto.ObraSocialId,
                    NumeroAfiliado = dto.NumeroAfiliado,
                    Persona = persona
                };
                await uow.Repositorios.PacienteRepositorio.CrearAsync(paciente);

                await uow.GuardarCambios();

                return CreatedAtAction(
                    nameof(ObtenerPorCuil), 
                    new { cuil = paciente.Persona.Cuil }, 
                    paciente
                );
            }
            catch (Exception ex)
            {
                await uow.CancelarCambios();
                _logger.LogError(ex, "Error al crear paciente");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }

    // DTO para crear paciente
    public record CrearPacienteDto(
        string Cuil,
        string Apellido,
        string Nombre,
        string? Email,
        string Calle,
        int Numero,
        string Localidad,
        Guid? ObraSocialId,
        string? NumeroAfiliado
    );
}