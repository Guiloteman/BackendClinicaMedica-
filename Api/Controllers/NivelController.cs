using AccesoDatos.Contratos;
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
            var niveles = await _context.Repositorios.NivelRepositorio.ObtenerTodosAsync();
            return Ok(niveles);
        }
    }
}
