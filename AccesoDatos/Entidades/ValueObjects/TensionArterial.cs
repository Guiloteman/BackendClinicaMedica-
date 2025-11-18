namespace Entidades.ValueObjects
{
    public class TensionArterial
    {
        public decimal Sistolica { get; set; }
        public decimal Diastolica { get; set; }

        public TensionArterial() { }

        public TensionArterial(decimal sistolica, decimal diastolica)
        {
            Sistolica = sistolica;
            Diastolica = diastolica;
        }

        public string FormatoCompleto => $"{Sistolica}/{Diastolica}";
        
        public static TensionArterial Crear(decimal sistolica, decimal diastolica)
        {
            if (sistolica < 0 || diastolica < 0)
                throw new ArgumentException("Los valores de tensión arterial no pueden ser negativos");
            
            return new TensionArterial(sistolica, diastolica);
        }
    }
}