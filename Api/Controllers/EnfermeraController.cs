using AccesoDatos.Contratos;
using Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnfermeraController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EnfermeraController> _logger;

        public EnfermeraController(
            IUnitOfWork unitOfWork, 
            IWebHostEnvironment environment,
            ILogger<EnfermeraController> logger)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
            _logger = logger;
        }
        //[HttpGet]
        //[ProducesResponseType(typeof(List<Enfermera>), StatusCodes.Status200OK)]
        //public async Task<ActionResult<List<Enfermera>>> ObtenerTodos()
        //{
        //    using var uow = _unitOfWork.Create(_environment.IsDevelopment());

        //    try
        //    {
        //        var enfermeras = await uow.Repositorios.EnfermeraRepositorio.ObtenerTodosAsync();
        //        return Ok(enfermeras);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener todos los pacientes");
        //        return StatusCode(500, new { mensaje = "Error interno del servidor" });
        //    }
        //}

        /// <summary>
        /// Crea un nuevo paciente (IS2025-002)
        /// </summary>
        [HttpGet("{matricula}")]

        [ProducesResponseType(typeof(Enfermera), StatusCodes.Status200OK)]

        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<Enfermera>> ObtenerPorMatricula(string matricula)

        {

            using var uow = _unitOfWork.Create(_environment.IsDevelopment());

            try

            {

                // Buscar por la Matrícula. Asume la existencia del método en el repositorio.

                var enfermera = await uow.Repositorios.EnfermeraRepositorio.ObtenerPorMatriculaAsync(matricula);

                if (enfermera == null)

                {

                    // Maneja el error de "Enfermera no encontrada (Matrícula: 222222)"

                    return NotFound(new { mensaje = $"Error de Matrícula: No se encontró la Enfermera (Matrícula: {matricula})." });

                }

                return Ok(enfermera);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error al obtener Enfermera por Matrícula: {Matricula}", matricula);

                return StatusCode(500, new { mensaje = "Error interno del servidor" });

            }

        }

        [HttpPost]
        [ProducesResponseType(typeof(Enfermera), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Enfermera>> Crear([FromBody] CrearEnfermeraDto dto)
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

                // 3. Crear Persona
                var persona = new Persona
                {
                    Cuil = dto.Cuil,
                    Apellido = dto.Apellido,
                    Nombre = dto.Nombre,
                    Email = dto.Email
                };
                await uow.Repositorios.PersonaRepositorio.CrearAsync(persona);

                // 4. Crear Enfermera
                var Enfermera = new Enfermera
                {
                    PersonaId = persona.PersonaId,
                    Matricula = dto.Matricula
                  
                };
                await uow.Repositorios.EnfermeraRepositorio.CrearAsync(Enfermera);

                await uow.GuardarCambios();

                return Created("", Enfermera);
            }
            catch (Exception ex)
            {
                await uow.CancelarCambios();
                _logger.LogError(ex, "Error al crear Enfermera");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }

    // DTO para crear paciente
    public record CrearEnfermeraDto(
        string Cuil,
        string Apellido,
        string Nombre,
        string? Email,
        string Matricula
          );
}
