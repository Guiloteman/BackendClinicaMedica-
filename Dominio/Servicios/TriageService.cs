using AccesoDatos.Contratos;
using Dominio.DTOs;
using Entidades;

namespace Dominio.Servicios
{
    /// <summary>
    /// Implementación del servicio de Triage según IS2025-001
    /// Gestiona la cola de espera y priorización de pacientes
    /// </summary>
    public class TriageService : ITriageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TriageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Registra un nuevo ingreso a urgencias (IS2025-001)
        /// Reglas:
        /// - Validar datos obligatorios
        /// - Validar valores no negativos
        /// - Crear paciente si no existe
        /// - Asignar estado PENDIENTE
        /// - Insertar en cola de espera
        /// </summary>
        public async Task<ResultadoOperacion<Ingreso>> RegistrarIngresoAsync(RegistrarIngresoDto dto)
        {
            using var uow = _unitOfWork.Create(false);
            
            try
            {
                // ===== 1. VALIDACIONES OBLIGATORIAS =====
                var erroresValidacion = ValidarDatosIngreso(dto);
                if (erroresValidacion.Any())
                {
                    return ResultadoOperacion<Ingreso>.Failure(
                        "Datos de ingreso inválidos",
                        erroresValidacion
                    );
                }

                // ===== 2. VERIFICAR/CREAR PACIENTE =====
                var paciente = await uow.Repositorios.PacienteRepositorio.ObtenerPorCuilAsync(dto.CuilPaciente);
                
                if (paciente == null)
                {
                    return ResultadoOperacion<Ingreso>.Failure(
                        $"No se encontró un paciente con CUIL {dto.CuilPaciente}. " +
                        "Debe registrar al paciente primero (IS2025-002)."
                    );
                }

                // ===== 3. VALIDAR ENFERMERA =====
                var enfermeraExiste = await uow.Repositorios.EnfermeraRepositorio.ExisteAsync(dto.EnfermeraId);
                if (!enfermeraExiste)
                {
                    return ResultadoOperacion<Ingreso>.Failure(
                        $"La enfermera con ID {dto.EnfermeraId} no existe en el sistema."
                    );
                }

                // ===== 4. VALIDAR NIVEL DE EMERGENCIA =====
                var nivelEmergencia = await uow.Repositorios.NivelEmergenciaRepositorio
                    .ObtenerConNivelAsync(dto.NivelEmergenciaId);
                
                if (nivelEmergencia == null)
                {
                    return ResultadoOperacion<Ingreso>.Failure(
                        $"El nivel de emergencia con ID {dto.NivelEmergenciaId} no existe."
                    );
                }

                // ===== 5. CREAR INGRESO =====
                var ingreso = new Ingreso
                {
                    PacienteId = paciente.PersonaId,
                    EnfermeraId = dto.EnfermeraId,
                    FechaIngreso = DateTime.UtcNow,
                    Informe = dto.Informe,
                    NivelEmergenciaId = dto.NivelEmergenciaId,
                    Estado = "PENDIENTE", // IS2025-001: Estado inicial
                    TemperaturaC = dto.TemperaturaC,
                    FrecuenciaCardiacaLpm = dto.FrecuenciaCardiacaLpm,
                    FrecuenciaRespRpm = dto.FrecuenciaRespiratoriaRpm,
                    TensionSistolicaMmHg = dto.TensionSistolicaMmHg,
                    TensionDiastolicaMmHg = dto.TensionDiastolicaMmHg
                };

                // ===== 6. PERSISTIR =====
                await uow.Repositorios.IngresoRepositorio.CrearAsync(ingreso);
                await uow.GuardarCambios();

                // ===== 7. RETORNAR INGRESO CON RELACIONES =====
                var ingresoCompleto = await uow.Repositorios.IngresoRepositorio
                    .ObtenerConRelacionesAsync(ingreso.IngresoId);

                return ResultadoOperacion<Ingreso>.Success(
                    ingresoCompleto!,
                    $"Ingreso registrado exitosamente. El paciente {paciente.Persona?.NombreCompleto} " +
                    $"ha sido agregado a la cola de espera con nivel {nivelEmergencia.Nombre}."
                );
            }
            catch (Exception ex)
            {
                await uow.CancelarCambios();
                return ResultadoOperacion<Ingreso>.Failure(
                    "Error al registrar el ingreso",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Obtiene la cola de espera ordenada por prioridad (IS2025-001)
        /// Orden: NivelEmergencia.Prioridad DESC, FechaIngreso ASC
        /// </summary>
        public async Task<ResultadoOperacion<List<ColaEsperaDto>>> ObtenerColaEsperaAsync()
        {
            using var uow = _unitOfWork.Create(false);
            
            try
            {
                var ingresos = await uow.Repositorios.IngresoRepositorio.ObtenerColaEsperaAsync();
                
                var colaEspera = ingresos.Select((ingreso, index) =>
                {
                    var tiempoEspera = DateTime.UtcNow - ingreso.FechaIngreso;
                    var tiempoMaximo = TimeSpan.FromMinutes(ingreso.NivelEmergencia.Nivel.TiempoEsperaMinutos);
                    
                    return new ColaEsperaDto
                    {
                        IngresoId = ingreso.IngresoId,
                        FechaIngreso = ingreso.FechaIngreso,
                        PosicionEnCola = index + 1,
                        
                        // Paciente
                        PacienteId = ingreso.PacienteId,
                        NombrePaciente = ingreso.Paciente?.Persona?.Nombre ?? "",
                        ApellidoPaciente = ingreso.Paciente?.Persona?.Apellido ?? "",
                        CuilPaciente = ingreso.Paciente?.Persona?.Cuil ?? "",
                        
                        // Nivel de emergencia
                        NivelEmergenciaId = ingreso.NivelEmergenciaId,
                        NombreNivelEmergencia = ingreso.NivelEmergencia?.Nombre ?? "",
                        ColorNivel = ingreso.NivelEmergencia?.Nivel?.Color ?? "",
                        PrioridadNivel = ingreso.NivelEmergencia?.Nivel?.Prioridad ?? 0,
                        TiempoEsperaMaximoMinutos = ingreso.NivelEmergencia?.Nivel?.TiempoEsperaMinutos ?? 0,
                        
                        // Signos vitales
                        TemperaturaC = ingreso.TemperaturaC,
                        FrecuenciaCardiacaLpm = ingreso.FrecuenciaCardiacaLpm,
                        FrecuenciaRespiratoriaRpm = ingreso.FrecuenciaRespRpm,
                        TensionArterial = ingreso.TensionArterialFormato,
                        
                        // Tiempo de espera
                        TiempoEsperaActual = tiempoEspera,
                        SuperaTiempoMaximo = tiempoEspera > tiempoMaximo
                    };
                }).ToList();

                return ResultadoOperacion<List<ColaEsperaDto>>.Success(
                    colaEspera,
                    $"Se encontraron {colaEspera.Count} pacientes en espera."
                );
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<List<ColaEsperaDto>>.Failure(
                    "Error al obtener la cola de espera",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Reclama el próximo paciente a atender (IS2025-003)
        /// Usa bloqueo optimista (UPDLOCK, READPAST) para evitar condiciones de carrera
        /// </summary>
        public async Task<ResultadoOperacion<Ingreso>> ReclamarProximoPacienteAsync(Guid medicoId)
        {
            using var uow = _unitOfWork.Create(false);
            
            try
            {
                // Validar que el médico existe
                var medicoExiste = await uow.Repositorios.DoctorRepositorio.ExisteAsync(medicoId);
                if (!medicoExiste)
                {
                    return ResultadoOperacion<Ingreso>.Failure(
                        $"El médico con ID {medicoId} no existe en el sistema."
                    );
                }

                // Obtener próximo paciente con bloqueo
                var ingreso = await uow.Repositorios.IngresoRepositorio.ObtenerProximoPacienteAsync();
                
                if (ingreso == null)
                {
                    return ResultadoOperacion<Ingreso>.Failure(
                        "No hay pacientes en la lista de espera."
                    );
                }

                // Cambiar estado a EN_PROCESO
                await uow.Repositorios.IngresoRepositorio.CambiarEstadoAsync(
                    ingreso.IngresoId,
                    "EN_PROCESO"
                );

                await uow.GuardarCambios();

                // Obtener ingreso completo con todas las relaciones
                var ingresoCompleto = await uow.Repositorios.IngresoRepositorio
                    .ObtenerConRelacionesAsync(ingreso.IngresoId);

                return ResultadoOperacion<Ingreso>.Success(
                    ingresoCompleto!,
                    $"Paciente {ingreso.Paciente?.Persona?.NombreCompleto} reclamado exitosamente. " +
                    $"Nivel: {ingreso.NivelEmergencia?.Nombre}"
                );
            }
            catch (Exception ex)
            {
                await uow.CancelarCambios();
                return ResultadoOperacion<Ingreso>.Failure(
                    "Error al reclamar el paciente",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Calcula la posición actual de un ingreso en la cola
        /// </summary>
        public async Task<ResultadoOperacion<int>> CalcularPosicionEnColaAsync(Guid ingresoId)
        {
            using var uow = _unitOfWork.Create(false);
            
            try
            {
                var ingreso = await uow.Repositorios.IngresoRepositorio
                    .ObtenerConRelacionesAsync(ingresoId);
                
                if (ingreso == null)
                {
                    return ResultadoOperacion<int>.Failure("El ingreso no existe.");
                }

                if (ingreso.Estado != "PENDIENTE")
                {
                    return ResultadoOperacion<int>.Failure(
                        "El ingreso no está en estado PENDIENTE."
                    );
                }

                var colaEspera = await uow.Repositorios.IngresoRepositorio.ObtenerColaEsperaAsync();
                var posicion = colaEspera.FindIndex(i => i.IngresoId == ingresoId) + 1;

                return ResultadoOperacion<int>.Success(
                    posicion,
                    $"El ingreso está en la posición {posicion} de {colaEspera.Count}."
                );
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<int>.Failure(
                    "Error al calcular la posición",
                    new List<string> { ex.Message }
                );
            }
        }

        /// <summary>
        /// Obtiene ingresos que superan el tiempo máximo de espera
        /// </summary>
        public async Task<ResultadoOperacion<List<Ingreso>>> ObtenerIngresosConTiempoExcedidoAsync()
        {
            using var uow = _unitOfWork.Create(false);
            
            try
            {
                var ingresos = await uow.Repositorios.IngresoRepositorio.ObtenerColaEsperaAsync();
                var ahora = DateTime.UtcNow;

                var excedidos = ingresos.Where(i =>
                {
                    var tiempoEspera = ahora - i.FechaIngreso;
                    var tiempoMaximo = TimeSpan.FromMinutes(i.NivelEmergencia.Nivel.TiempoEsperaMinutos);
                    return tiempoEspera > tiempoMaximo;
                }).ToList();

                return ResultadoOperacion<List<Ingreso>>.Success(
                    excedidos,
                    $"Se encontraron {excedidos.Count} pacientes con tiempo de espera excedido."
                );
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<List<Ingreso>>.Failure(
                    "Error al obtener ingresos con tiempo excedido",
                    new List<string> { ex.Message }
                );
            }
        }

        #region Validaciones

        /// <summary>
        /// Valida los datos del ingreso según IS2025-001
        /// </summary>
        private List<string> ValidarDatosIngreso(RegistrarIngresoDto dto)
        {
            var errores = new List<string>();

            // Validar CUIL
            if (string.IsNullOrWhiteSpace(dto.CuilPaciente))
                errores.Add("El CUIL del paciente es obligatorio.");

            // Validar Enfermera
            if (dto.EnfermeraId == Guid.Empty)
                errores.Add("El ID de la enfermera es obligatorio.");

            // Validar Informe
            if (string.IsNullOrWhiteSpace(dto.Informe))
                errores.Add("El informe es obligatorio.");

            // Validar Nivel de Emergencia
            if (dto.NivelEmergenciaId <= 0)
                errores.Add("El nivel de emergencia es obligatorio.");

            // Validar Frecuencia Cardíaca (obligatorio, no negativo)
            if (dto.FrecuenciaCardiacaLpm < 0)
                errores.Add("La frecuencia cardíaca no puede ser negativa.");

            // Validar Frecuencia Respiratoria (obligatorio, no negativo)
            if (dto.FrecuenciaRespiratoriaRpm < 0)
                errores.Add("La frecuencia respiratoria no puede ser negativa.");

            // Validar Tensión Arterial Sistólica (obligatorio, no negativo)
            if (dto.TensionSistolicaMmHg < 0)
                errores.Add("La tensión sistólica no puede ser negativa.");

            // Validar Tensión Arterial Diastólica (obligatorio, no negativo)
            if (dto.TensionDiastolicaMmHg < 0)
                errores.Add("La tensión diastólica no puede ser negativa.");

            return errores;
        }

        #endregion
    }
}