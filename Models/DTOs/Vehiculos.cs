namespace CarSlineAPI.Models.DTOs
{
    public class VehiculoRequest
    {
        public int ClienteId { get; set; }
        public string VIN { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Anio { get; set; }
        public string? Color { get; set; }
        public string? Placas { get; set; }
        public int KilometrajeInicial { get; set; }
    }
}
