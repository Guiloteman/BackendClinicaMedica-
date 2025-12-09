namespace Dominio.DTOs
{
    /// <summary>
    /// Resultado genérico de una operación del dominio
    /// </summary>
    public class ResultadoOperacion<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public List<string> Errores { get; set; } = new();
        public T? Datos { get; set; }

        public static ResultadoOperacion<T> Success(T datos, string mensaje = "Operación exitosa")
        {
            return new ResultadoOperacion<T>
            {
                Exitoso = true,
                Mensaje = mensaje,
                Datos = datos
            };
        }

        public static ResultadoOperacion<T> Failure(string mensaje, List<string>? errores = null)
        {
            return new ResultadoOperacion<T>
            {
                Exitoso = false,
                Mensaje = mensaje,
                Errores = errores ?? new List<string>()
            };
        }
    }
}