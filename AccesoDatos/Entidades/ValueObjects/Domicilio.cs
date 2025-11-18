namespace Entidades.ValueObjects
{
    public class Domicilio
    {
        public string Calle { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string Localidad { get; set; } = string.Empty;

        public Domicilio() { }

        public Domicilio(string calle, int numero, string localidad)
        {
            Calle = calle;
            Numero = numero;
            Localidad = localidad;
        }

        public string DireccionCompleta => $"{Calle} {Numero}, {Localidad}";
    }
}