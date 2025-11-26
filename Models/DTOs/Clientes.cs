using System.ComponentModel.DataAnnotations;

namespace CarSlineAPI.Models.DTOs
{
    public class ClienteRequest
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string TelefonoMovil { get; set; } = string.Empty;
        public string? TelefonoCasa { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Colonia { get; set; }
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? Pais { get; set; }
        public string? CodigoPostal { get; set; }
    }

}
