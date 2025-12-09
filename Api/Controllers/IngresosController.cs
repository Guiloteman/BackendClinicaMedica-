using AccesoDatos.Contratos;
using Entidades;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngresosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<IngresosController> _logger;

        public IngresosController(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment,
            ILogger<IngresosController> logger)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var uow = _unitOfWork.Create(false);
            var ingresos = await uow.Repositorios.IngresoRepositorio.ObtenerTodosAsync();
            return Ok(ingresos);
        }

        /// <summary>
        /// Obtiene la lista de pacientes en cola de espera.
        /// </summary>
        [HttpGet("ColaEspera")]
        [ProducesResponseType(typeof(List<Ingreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Ingreso>>> ObtenerColaEspera()
        {
            using var uow = _unitOfWork.Create(_environment.IsDevelopment());

            try
            {
                var ingresos = await uow.Repositorios.IngresoRepositorio.ObtenerColaEsperaAsync();

                if (ingresos == null)
                {
                    ingresos = new List<Ingreso>();
                }

                return Ok(ingresos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la cola de espera de pacientes.");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearIngresoDto dto)
        {
            using var uow = _unitOfWork.Create(false);
            try
            {
                var ingreso = new Ingreso
                {
                    IngresoId = Guid.NewGuid(),
                    PacienteId = dto.PacienteId,
                    EnfermeraId = dto.EnfermeraId,
                    FechaIngreso = dto.FechaIngreso,
                    Informe = dto.Informe,
                    NivelEmergenciaId = dto.NivelEmergenciaId,
                    Estado = dto.Estado,
                    TemperaturaC = dto.TemperaturaC,
                    FrecuenciaCardiacaLpm = dto.FrecuenciaCardiacaLpm,
                    FrecuenciaRespRpm = dto.FrecuenciaRespRpm,
                    TensionSistolicaMmHg = dto.TensionSistolicaMmHg,
                    TensionDiastolicaMmHg = dto.TensionDiastolicaMmHg
                };

                await uow.Repositorios.IngresoRepositorio.CrearAsync(ingreso);
                await uow.GuardarCambios();

                return CreatedAtAction(nameof(GetAll), new { id = ingreso.IngresoId }, ingreso);
            }
            catch (Exception ex)
            {
                await uow.CancelarCambios();
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Ingreso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ingreso>> ObtenerPorId(Guid id)
        {
            using var uow = _unitOfWork.Create(_environment.IsDevelopment());

            // 1. Obtener el Ingreso del repositorio
            var ingreso = await uow.Repositorios.IngresoRepositorio.ObtenerPorIdAsync(id);

            if (ingreso == null)
            {
                return NotFound(new { mensaje = $"Ingreso con ID {id} no encontrado." });
            }

            // 2. Devolver el Ingreso
            return Ok(ingreso);
        }
    }
    public record CrearIngresoDto(
        Guid PacienteId,
        Guid EnfermeraId,
        DateTime FechaIngreso,
        string Informe,
        int NivelEmergenciaId,
        string Estado,
        decimal? TemperaturaC,
        decimal FrecuenciaCardiacaLpm,
        decimal FrecuenciaRespRpm,
        decimal TensionSistolicaMmHg,
        decimal TensionDiastolicaMmHg
    );
}

