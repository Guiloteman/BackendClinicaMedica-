using AccesoDatos.Configuracion;
using AccesoDatos.Contratos;
using Microsoft.Extensions.Options;

namespace AccesoDatos.Repositorios
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseSettings _settings;

        public UnitOfWork(IOptions<DatabaseSettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            
            // Validación temprana de configuración
            if (!_settings.IsValid())
            {
                throw new InvalidOperationException(
                    "No se encontró ninguna connection string configurada en appsettings.json. " +
                    "Asegúrese de configurar 'ConnectionStrings:UrgenciasDB' o 'ConnectionStrings:UrgenciasDBDev'."
                );
            }
        }
        
        public IUnitOfWorkAdapter Create(bool isDevelopment = false)
        {
            var connectionString = GetConnectionString(isDevelopment);
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"La connection string para el entorno {(isDevelopment ? "desarrollo" : "producción")} no está configurada."
                );
            }
            
            return new UnitOfWorkAdapter(connectionString);
        }

        private string GetConnectionString(bool isDevelopment)
        {
            // Prioridad: Dev si está en desarrollo y existe, sino usar producción
            if (isDevelopment && !string.IsNullOrWhiteSpace(_settings.UrgenciasDBDev))
            {
                return _settings.UrgenciasDBDev;
            }

            // Fallback a producción o dev si no hay producción configurada
            return !string.IsNullOrWhiteSpace(_settings.UrgenciasDB) 
                ? _settings.UrgenciasDB 
                : _settings.UrgenciasDBDev;
        }
    }
}

