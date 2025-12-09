using AccesoDatos.Contratos;
using Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NivelController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NivelController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var _context =  _unitOfWork.Create(false);
            var niveles = await _context.Repositorios.NivelEmergenciaRepositorio.ObtenerTodosAsync();
            return Ok(niveles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NivelEmergencia>> GetById(int id) 
        {
            var _context =  _unitOfWork.Create(false);
            var nivel = await _context.Repositorios.NivelEmergenciaRepositorio.ObtenerPorIdAsync(id);
            if (nivel == null)
                return NotFound(new { mensaje = "Nivel de emergencia no encontrado" });
            return Ok(nivel);
        }

    }
}
