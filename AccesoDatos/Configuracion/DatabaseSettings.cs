using System;
using System.ComponentModel.DataAnnotations;

namespace AccesoDatos.Configuracion
{
    /// <summary>
    /// Configuración de conexiones a base de datos.
    /// Se vincula a la sección "ConnectionStrings" del appsettings.json
    /// </summary>
    public class DatabaseSettings
    {
        public const string SectionName = "ConnectionStrings";

        /// <summary>
        /// Connection string principal para producción
        /// </summary>
        [Required(ErrorMessage = "La connection string 'UrgenciasDB' es requerida")]
        public string UrgenciasDB { get; set; } = string.Empty;

        /// <summary>
        /// Connection string para desarrollo/testing
        /// </summary>
        public string UrgenciasDBDev { get; set; } = string.Empty;

        /// <summary>
        /// Valida que al menos exista una connection string configurada
        /// </summary>
        public bool IsValid() => !string.IsNullOrWhiteSpace(UrgenciasDB) || 
                                  !string.IsNullOrWhiteSpace(UrgenciasDBDev);
    }
}
