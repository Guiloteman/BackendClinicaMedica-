using Dominio.DTOs;
using Dominio.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriageController : ControllerBase
    {
        private readonly ITriageService _triageService;
        private readonly ILogger<TriageController> _logger;

        public TriageController(
            ITriageService triageService,
            ILogger<TriageController> logger)
        {
            _triageService = triageService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo ingreso a urgencias (IS2025-001)
        /// </summary>
        [HttpPost("registrar-ingreso")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarIngreso([FromBody] RegistrarIngresoDto dto)
        {
            var resultado = await _triageService.RegistrarIngresoAsync(dto);

            if (!resultado.Exitoso)
            {
                return BadRequest(new
                {
                    mensaje = resultado.Mensaje,
                    errores = resultado.Errores
                });
            }

            return CreatedAtAction(
                nameof(ObtenerColaEspera),
                new { mensaje = resultado.Mensaje, ingreso = resultado.Datos }
            );
        }

        /// <summary>
        /// Obtiene la cola de espera ordenada por prioridad (IS2025-001)
        /// </summary>
        [HttpGet("cola-espera")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerColaEspera()
        {
            var resultado = await _triageService.ObtenerColaEsperaAsync();

            if (!resultado.Exitoso)
            {
                return BadRequest(new { mensaje = resultado.Mensaje, errores = resultado.Errores });
            }

            return Ok(new
            {
                total = resultado.Datos?.Count ?? 0,
                pacientes = resultado.Datos,
                mensaje = resultado.Mensaje
            });
        }

        /// <summary>
        /// Reclama el próximo paciente a atender (IS2025-003)
        /// </summary>
        [HttpPost("reclamar-paciente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReclamarPaciente([FromBody] ReclamarPacienteRequest request)
        {
            var resultado = await _triageService.ReclamarProximoPacienteAsync(request.MedicoId);

            if (!resultado.Exitoso)
            {
                return NotFound(new { mensaje = resultado.Mensaje, errores = resultado.Errores });
            }

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                ingreso = resultado.Datos
            });
        }

        /// <summary>
        /// Calcula la posición de un ingreso en la cola
        /// </summary>
        [HttpGet("{ingresoId}/posicion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPosicionEnCola(Guid ingresoId)
        {
            var resultado = await _triageService.CalcularPosicionEnColaAsync(ingresoId);

            if (!resultado.Exitoso)
            {
                return BadRequest(new { mensaje = resultado.Mensaje });
            }

            return Ok(new
            {
                ingresoId,
                posicion = resultado.Datos,
                mensaje = resultado.Mensaje
            });
        }

        /// <summary>
        /// Obtiene pacientes con tiempo de espera excedido
        /// </summary>
        [HttpGet("alertas/tiempo-excedido")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTiempoExcedido()
        {
            var resultado = await _triageService.ObtenerIngresosConTiempoExcedidoAsync();

            if (!resultado.Exitoso)
            {
                return BadRequest(new { mensaje = resultado.Mensaje });
            }

            return Ok(new
            {
                total = resultado.Datos?.Count ?? 0,
                ingresosExcedidos = resultado.Datos,
                mensaje = resultado.Mensaje
            });
        }
    }

    public record ReclamarPacienteRequest(Guid MedicoId);
}