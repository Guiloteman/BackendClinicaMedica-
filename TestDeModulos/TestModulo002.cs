using AccesoDatos.Contratos;
using Api.Controllers;
using Entidades;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestDeModulos.Api.Controllers
{
    [TestClass]
    public class PacientesControllerTests
    {
        // Mocks de las dependencias
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IUnitOfWorkAdapter> _mockUnitOfWorkAdapter;
        private Mock<IUnitOfWorkRepository> _mockRepository;
        private Mock<IPacienteRepositorio> _mockPacienteRepositorio;
        private Mock<IPersonaRepositorio> _mockPersonaRepositorio;
        private Mock<IObraSocialRepositorio> _mockObraSocialRepositorio;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private Mock<ILogger<PacientesController>> _mockLogger;

        // Instancia de la clase a probar
        private PacientesController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Inicializar Mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWorkAdapter = new Mock<IUnitOfWorkAdapter>();
            _mockRepository = new Mock<IUnitOfWorkRepository>();
            _mockPacienteRepositorio = new Mock<IPacienteRepositorio>();
            _mockPersonaRepositorio = new Mock<IPersonaRepositorio>();
            _mockObraSocialRepositorio = new Mock<IObraSocialRepositorio>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<PacientesController>>();

            // 1. Configurar la cadena Unit of Work para que devuelva los repositorios mockeados
            _mockUnitOfWork.Setup(u => u.Create(It.IsAny<bool>()))
                           .Returns(_mockUnitOfWorkAdapter.Object);

            _mockUnitOfWorkAdapter.SetupGet(u => u.Repositorios)
                                 .Returns(_mockRepository.Object);

            // 2. Configurar los repositorios dentro de IRepositorios
            _mockRepository.SetupGet(r => r.PacienteRepositorio)
                               .Returns(_mockPacienteRepositorio.Object);

            _mockRepository.SetupGet(r => r.PersonaRepositorio)
                               .Returns(_mockPersonaRepositorio.Object);

            _mockRepository.SetupGet(r => r.ObraSocialRepositorio)
                               .Returns(_mockObraSocialRepositorio.Object);

            // 3. Crear la instancia del Controlador con los mocks
            _controller = new PacientesController(
                _mockUnitOfWork.Object,
                _mockEnvironment.Object,
                _mockLogger.Object
            );
        }

        // ... (Test ObtenerPorCuil mantienen su lógica) ...

        [TestMethod]
        public async Task ObtenerPorCuil_DebeRetornarOkConPaciente_CuandoExiste()
        {
            // Arrange
            var cuilPrueba = "20-12345678-0";
            var pacienteEsperado = new Paciente { PersonaId = Guid.NewGuid() };

            // Configurar el mock para que devuelva un paciente
            _mockPacienteRepositorio.Setup(r => r.ObtenerPorCuilAsync(cuilPrueba))
                                     .ReturnsAsync(pacienteEsperado);

            // Act
            var resultado = await _controller.ObtenerPorCuil(cuilPrueba);

            // Assert
            // 1. Verificar que el resultado es un OkObjectResult (código 200)
            Assert.IsInstanceOfType(resultado.Result, typeof(OkObjectResult));
            var okResult = resultado.Result as OkObjectResult;

            // 2. Verificar que el objeto devuelto es el paciente esperado
            Assert.AreEqual(pacienteEsperado, okResult.Value);

            // 3. (Opcional) Verificar que el método del repositorio fue llamado
            _mockPacienteRepositorio.Verify(r => r.ObtenerPorCuilAsync(cuilPrueba), Times.Once);
        }

        [TestMethod]
        public async Task ObtenerPorCuil_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var cuilPrueba = "99-99999999-9";

            // Configurar el mock para que devuelva null (no encontrado)
            _mockPacienteRepositorio.Setup(r => r.ObtenerPorCuilAsync(cuilPrueba))
                                     .ReturnsAsync((Paciente)null);

            // Act
            var resultado = await _controller.ObtenerPorCuil(cuilPrueba);

            // Assert
            // 1. Verificar que el resultado es un NotFoundObjectResult (código 404)
            Assert.IsInstanceOfType(resultado.Result, typeof(NotFoundObjectResult));
            var notFoundResult = resultado.Result as NotFoundObjectResult;
        }

        [TestMethod]
        public async Task ObtenerPorCuil_DebeRetornarStatusCode500_EnCasoDeExcepcion()
        {
            // Arrange
            var cuilPrueba = "20-12345678-0";

            // Configurar el mock para que lance una excepción
            _mockPacienteRepositorio.Setup(r => r.ObtenerPorCuilAsync(cuilPrueba))
                                     .ThrowsAsync(new InvalidOperationException("Error de DB simulado"));

            // Act
            var resultado = await _controller.ObtenerPorCuil(cuilPrueba);

            // Assert
            // 1. Verificar que el resultado es un ObjectResult con StatusCode 500
            Assert.IsInstanceOfType(resultado.Result, typeof(ObjectResult));
            var objectResult = resultado.Result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);

            // 2. Verificar que la excepción fue logueada
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al obtener paciente por CUIL")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Crear_DebeRetornarCreated_CuandoPacienteEsValido()
        {
            // Arrange
            var dto = new CrearPacienteDto(
                Cuil: "20-87654321-0",
                Apellido: "Gomez",
                Nombre: "Ana",
                Email: "a.gomez@test.com",
                Calle: "Calle Test",
                Numero: 456,
                Localidad: "Localidad Test",
                ObraSocialId: null, // Sin Obra Social
                NumeroAfiliado: null
            );

            // 1. Configurar mocks de validación (deben retornar que NO existen)
            _mockPersonaRepositorio.Setup(r => r.ExisteCuilAsync(dto.Cuil)).ReturnsAsync(false);

            // 2. Configurar mocks de creación (AQUÍ ESTÁN LAS CORRECCIONES)
            // *** CORRECCIÓN 1: PersonaRepositorio.CrearAsync debe devolver Task<Guid> ***
            _mockPersonaRepositorio.Setup(r => r.CrearAsync(It.IsAny<Persona>()))
                                     .Callback<Persona>(p => p.PersonaId = Guid.NewGuid()) // Captura y asigna ID (para el CreatedAtAction)
                                     .ReturnsAsync(Guid.NewGuid()); // Devuelve el Task<Guid> que necesita la interfaz

            // *** CORRECCIÓN 2: PacienteRepositorio.CrearAsync debe devolver Task<Guid> ***
            _mockPacienteRepositorio.Setup(r => r.CrearAsync(It.IsAny<Paciente>()))
                                     .ReturnsAsync(Guid.NewGuid()); // Devuelve el Task<Guid>

            // 3. Configurar GuardarCambios
            _mockUnitOfWorkAdapter.Setup(u => u.GuardarCambios()).Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            // 1. Verificar que el resultado es un CreatedAtActionResult (código 201)
            Assert.IsInstanceOfType(resultado.Result, typeof(CreatedAtActionResult));
            var createdResult = resultado.Result as CreatedAtActionResult;

            // 2. Verificar la acción y los valores devueltos
            Assert.AreEqual(nameof(PacientesController.ObtenerPorCuil), createdResult.ActionName);
            var pacienteCreado = createdResult.Value as Paciente;
            Assert.IsNotNull(pacienteCreado);
            Assert.AreEqual(dto.Cuil, pacienteCreado.Persona.Cuil);

            // 3. Verificar que los métodos de creación y transacción fueron llamados
            _mockPersonaRepositorio.Verify(r => r.CrearAsync(It.IsAny<Persona>()), Times.Once);
            _mockPacienteRepositorio.Verify(r => r.CrearAsync(It.IsAny<Paciente>()), Times.Once);
            _mockUnitOfWorkAdapter.Verify(u => u.GuardarCambios(), Times.Once);
            _mockUnitOfWorkAdapter.Verify(u => u.CancelarCambios(), Times.Never);
        }

        [TestMethod]
        public async Task Crear_DebeRetornarBadRequest_CuandoCuilYaExiste()
        {
            // ... (Este test no crea, por lo que no necesita corrección) ...
            // Arrange
            var dto = new CrearPacienteDto(
                Cuil: "20-87654321-0",
                Apellido: "Gomez",
                Nombre: "Ana",
                Email: "a.gomez@test.com",
                Calle: "Calle Test",
                Numero: 456,
                Localidad: "Localidad Test",
                ObraSocialId: null,
                NumeroAfiliado: null
            );

            // Configurar mock para indicar que el CUIL YA existe
            _mockPersonaRepositorio.Setup(r => r.ExisteCuilAsync(dto.Cuil)).ReturnsAsync(true);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            // 1. Verificar que el resultado es un BadRequestObjectResult (código 400)
            Assert.IsInstanceOfType(resultado.Result, typeof(BadRequestObjectResult));

            // 2. Verificar que NO se intentó guardar nada
            _mockUnitOfWorkAdapter.Verify(u => u.GuardarCambios(), Times.Never);
        }

        [TestMethod]
        public async Task Crear_DebeRetornarBadRequest_CuandoObraSocialNoExiste()
        {
            // ... (Este test no crea, por lo que no necesita corrección) ...
            // Arrange
            var obraSocialIdInvalido = Guid.NewGuid();
            var dto = new CrearPacienteDto(
                Cuil: "20-87654321-0",
                Apellido: "Gomez",
                Nombre: "Ana",
                Email: "a.gomez@test.com",
                Calle: "Calle Test",
                Numero: 456,
                Localidad: "Localidad Test",
                ObraSocialId: obraSocialIdInvalido,
                NumeroAfiliado: "AFIL123"
            );

            // 1. Validaciones
            _mockPersonaRepositorio.Setup(r => r.ExisteCuilAsync(dto.Cuil)).ReturnsAsync(false);
            // Configurar mock para indicar que la Obra Social NO existe
            _mockObraSocialRepositorio.Setup(r => r.ExisteAsync(obraSocialIdInvalido)).ReturnsAsync(false);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            // 1. Verificar que el resultado es un BadRequestObjectResult (código 400)
            Assert.IsInstanceOfType(resultado.Result, typeof(BadRequestObjectResult));

            // 2. Verificar que NO se intentó guardar nada
            _mockUnitOfWorkAdapter.Verify(u => u.GuardarCambios(), Times.Never);
        }

        [TestMethod]
        public async Task Crear_DebeRetornarBadRequest_CuandoFaltaNumeroAfiliadoConObraSocial()
        {
            // ... (Este test no crea, por lo que no necesita corrección) ...
            // Arrange
            var obraSocialIdValido = Guid.NewGuid();
            var dto = new CrearPacienteDto(
                Cuil: "20-87654321-0",
                Apellido: "Gomez",
                Nombre: "Ana",
                Email: "a.gomez@test.com",
                Calle: "Calle Test",
                Numero: 456,
                Localidad: "Localidad Test",
                ObraSocialId: obraSocialIdValido,
                NumeroAfiliado: "" // Número de afiliado vacío
            );

            // 1. Validaciones
            _mockPersonaRepositorio.Setup(r => r.ExisteCuilAsync(dto.Cuil)).ReturnsAsync(false);
            // Configurar mock para indicar que la Obra Social SÍ existe
            _mockObraSocialRepositorio.Setup(r => r.ExisteAsync(obraSocialIdValido)).ReturnsAsync(true);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            // 1. Verificar que el resultado es un BadRequestObjectResult (código 400)
            Assert.IsInstanceOfType(resultado.Result, typeof(BadRequestObjectResult));

            // 2. Verificar que NO se intentó guardar nada
            _mockUnitOfWorkAdapter.Verify(u => u.GuardarCambios(), Times.Never);
        }

        [TestMethod]
        public async Task Crear_DebeRetornarStatusCode500_EnCasoDeFalloEnGuardarCambios()
        {
            // Arrange
            var dto = new CrearPacienteDto(
                Cuil: "20-87654321-0",
                Apellido: "Gomez",
                Nombre: "Ana",
                Email: "a.gomez@test.com",
                Calle: "Calle Test",
                Numero: 456,
                Localidad: "Localidad Test",
                ObraSocialId: null,
                NumeroAfiliado: null
            );

            // 1. Mocks OK para las validaciones
            _mockPersonaRepositorio.Setup(r => r.ExisteCuilAsync(dto.Cuil)).ReturnsAsync(false);

            // 2. Mocks OK para las creaciones (AQUÍ ESTÁN LAS CORRECCIONES)
            // *** CORRECCIÓN 3: PersonaRepositorio.CrearAsync debe devolver Task<Guid> ***
            _mockPersonaRepositorio.Setup(r => r.CrearAsync(It.IsAny<Persona>()))
                                     .Callback<Persona>(p => p.PersonaId = Guid.NewGuid())
                                     .ReturnsAsync(Guid.NewGuid()); // Devuelve el Task<Guid>

            // *** CORRECCIÓN 4: PacienteRepositorio.CrearAsync debe devolver Task<Guid> ***
            _mockPacienteRepositorio.Setup(r => r.CrearAsync(It.IsAny<Paciente>()))
                                     .ReturnsAsync(Guid.NewGuid()); // Devuelve el Task<Guid>

            // 3. Configurar GuardarCambios para que lance una excepción (ej. fallo de conexión)
            _mockUnitOfWorkAdapter.Setup(u => u.GuardarCambios()).ThrowsAsync(new Exception("Fallo de GuardarCambios simulado"));

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            // 1. Verificar que el resultado es un ObjectResult con StatusCode 500
            Assert.IsInstanceOfType(resultado.Result, typeof(ObjectResult));
            var objectResult = resultado.Result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);

            // 2. Verificar que se llamó a CancelarCambios para hacer Rollback
            _mockUnitOfWorkAdapter.Verify(u => u.CancelarCambios(), Times.Once);

            // 3. Verificar que la excepción fue logueada
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al crear paciente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task ObtenerTodos_DebeRetornarOkConListaDePacientes_CuandoExistenDatos()
        {
            // Arrange
            var pacientesEsperados = new List<Paciente>
    {
        new Paciente { PersonaId = Guid.NewGuid(), Persona = new Persona { Nombre = "Juan" } },
        new Paciente { PersonaId = Guid.NewGuid(), Persona = new Persona { Nombre = "Maria" } }
    };

            // Configurar el mock para que devuelva la lista esperada
            _mockPacienteRepositorio.Setup(r => r.ObtenerTodosAsync())
                                    .ReturnsAsync(pacientesEsperados);

            // Act
            var resultado = await _controller.ObtenerTodos();

            // Assert
            // 1. Verificar que el resultado es un OkObjectResult (código 200)
            Assert.IsInstanceOfType(resultado.Result, typeof(OkObjectResult));
            var okResult = resultado.Result as OkObjectResult;

            // 2. Verificar que el valor devuelto es la lista de pacientes esperada
            Assert.AreEqual(pacientesEsperados, okResult.Value);

            // 3. Verificar que el método del repositorio se llamó una sola vez
            _mockPacienteRepositorio.Verify(r => r.ObtenerTodosAsync(), Times.Once);
        }

        [TestMethod]
        public async Task ObtenerTodos_DebeRetornarStatusCode500_EnCasoDeExcepcion()
        {
            // Arrange
            // Configurar el mock para que lance una excepción de base de datos simulada
            _mockPacienteRepositorio.Setup(r => r.ObtenerTodosAsync())
                                    .ThrowsAsync(new InvalidOperationException("Error de conexión simulado"));

            // Act
            var resultado = await _controller.ObtenerTodos();

            // Assert
            // 1. Verificar que el resultado es un ObjectResult con StatusCode 500
            Assert.IsInstanceOfType(resultado.Result, typeof(ObjectResult));
            var objectResult = resultado.Result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);

            // 2. Verificar que la excepción fue logueada
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al obtener todos los pacientes")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task ObtenerPorId_DebeRetornarOkConPaciente_CuandoExiste()
        {
            // Arrange
            var idPrueba = Guid.NewGuid();
            var pacienteEsperado = new Paciente { PersonaId = idPrueba, Persona = new Persona { Nombre = "ID Test" } };

            // Configurar el mock para que devuelva el paciente
            // Usamos ObtenerPorIdAsync, que está en la interfaz
            _mockPacienteRepositorio.Setup(r => r.ObtenerPorIdAsync(idPrueba))
                                    .ReturnsAsync(pacienteEsperado);

            // Act
            var resultado = await _controller.ObtenerPorId(idPrueba);

            // Assert
            // 1. Verificar que el resultado es un OkObjectResult (código 200)
            Assert.IsInstanceOfType(resultado.Result, typeof(OkObjectResult));
            var okResult = resultado.Result as OkObjectResult;

            // 2. Verificar que el objeto devuelto es el paciente esperado
            Assert.AreEqual(pacienteEsperado, okResult.Value);

            // 3. Verificar que el método del repositorio fue llamado
            _mockPacienteRepositorio.Verify(r => r.ObtenerPorIdAsync(idPrueba), Times.Once);
        }

        [TestMethod]
        public async Task ObtenerPorId_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var idPrueba = Guid.NewGuid();

            // Configurar el mock para que devuelva null (no encontrado)
            _mockPacienteRepositorio.Setup(r => r.ObtenerPorIdAsync(idPrueba))
                                    .ReturnsAsync((Paciente)null);

            // Act
            var resultado = await _controller.ObtenerPorId(idPrueba);

            // Assert
            // 1. Verificar que el resultado es un NotFoundObjectResult (código 404)
            Assert.IsInstanceOfType(resultado.Result, typeof(NotFoundObjectResult));
            var notFoundResult = resultado.Result as NotFoundObjectResult;

            // 2. Opcional: Verificar el mensaje de error en el objeto devuelto
            var mensajeError = notFoundResult.Value.GetType().GetProperty("mensaje").GetValue(notFoundResult.Value, null);
            Assert.IsTrue(mensajeError.ToString().Contains($"Paciente con ID {idPrueba} no encontrado."));
        }
    }
}