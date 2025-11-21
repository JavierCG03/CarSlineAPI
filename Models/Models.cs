using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarSlineAPI.Models
{
    // ============================================
    // ENTIDADES DE BASE DE DATOS
    // ============================================

    /// <summary>
    /// Modelo de Rol - Tipos de usuario del sistema
    /// </summary>
    [Table("roles")]
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreRol { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con usuarios
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }

    /// <summary>
    /// Modelo de Usuario - Usuarios del sistema
    /// </summary>
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RolId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? UltimoAcceso { get; set; }

        public bool Activo { get; set; } = true;

        public int? CreadoPorId { get; set; }

        // Navegación
        [ForeignKey("RolId")]
        public virtual Rol? Rol { get; set; }

        [ForeignKey("CreadoPorId")]
        public virtual Usuario? CreadoPor { get; set; }
    }

    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string TelefonoMovil { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TelefonoCasa { get; set; }

        [MaxLength(150)]
        public string? CorreoElectronico { get; set; }

        [MaxLength(150)]
        public string? Colonia { get; set; }

        [MaxLength(150)]
        public string? Calle { get; set; }

        [MaxLength(50)]
        public string? NumeroExterior { get; set; }

        [MaxLength(150)]
        public string? Municipio { get; set; }

        [MaxLength(150)]
        public string? Estado { get; set; }

        [MaxLength(100)]
        public string? Pais { get; set; }

        [MaxLength(20)]
        public string? CodigoPostal { get; set; }

        public bool Activo { get; set; } = true;
    }

    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required, MaxLength(50)]
        public string VIN { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Modelo { get; set; }

        public int? Anio { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(20)]
        public string? Placas { get; set; }

        public int KilometrajeInicial { get; set; }

        public bool Activo { get; set; } = true;

        // navegación
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }
    }

    [Table("TiposServicio")]
    public class TipoServicio
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string NombreServicio { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public decimal PrecioBase { get; set; }

        public bool Activo { get; set; } = true;
    }

    [Table("ServiciosExtra")]
    public class ServicioExtra
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string NombreServicio { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        [MaxLength(100)]
        public string? Categoria { get; set; }

        public bool Activo { get; set; } = true;
    }

    [Table("Ordenes")]
    public class Orden
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string NumeroOrden { get; set; } = string.Empty;

        public int TipoOrdenId { get; set; } // 1=SRV, etc.

        public int ClienteId { get; set; }

        public int VehiculoId { get; set; }

        public int AsesorId { get; set; }

        public int? TipoServicioId { get; set; }

        public int KilometrajeActual { get; set; }

        public int EstadoOrdenId { get; set; } = 1;

        public DateTime FechaHoraPromesaEntrega { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaInicioProceso { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string? ObservacionesAsesor { get; set; }

        public decimal CostoTotal { get; set; }

        public bool Activo { get; set; } = true;

        // relaciones
        public virtual ICollection<OrdenServicioExtra> ServiciosExtra { get; set; } = new List<OrdenServicioExtra>();
        public virtual Cliente Cliente { get; set; }
        public virtual Vehiculo Vehiculo { get; set; }
        public virtual TipoServicio TipoServicio { get; set; }
        //public virtual Usuario Tecnico { get; set; }
        public virtual Usuario Asesor { get; set; }
    }

    [Table("OrdenServiciosExtra")]
    public class OrdenServicioExtra
    {
        [Key]
        public int Id { get; set; }

        public int OrdenId { get; set; }

        public int ServicioExtraId { get; set; }

        public decimal PrecioAplicado { get; set; }

        // navegación
        [ForeignKey("OrdenId")]
        public Orden? Orden { get; set; }

        [ForeignKey("ServicioExtraId")]
        public ServicioExtra? ServicioExtra { get; set; }
    }

    [Table("HistorialServicios")]
    public class HistorialServicio
    {
        [Key]
        public int Id { get; set; }

        public int VehiculoId { get; set; }

        public int OrdenId { get; set; }

        public int TipoServicioId { get; set; }

        public int KilometrajeRegistrado { get; set; }

        public DateTime FechaServicio { get; set; }

        public int? ProximoServicioKm { get; set; }

        public DateTime? ProximoServicioFecha { get; set; }

        public decimal CostoTotal { get; set; }
    }
    // ============================================
    // DTOs - DATA TRANSFER OBJECTS
    // ============================================

    /// <summary>
    /// DTO para solicitud de login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para crear usuario (solo Admin)
    /// </summary>
    public class CrearUsuarioRequest
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        [Range(2, 5, ErrorMessage = "El rol debe ser entre 2 y 5 (no puede crear administradores)")]
        public int RolId { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de autenticación
    /// </summary>
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UsuarioDto? Usuario { get; set; }
        public string? Token { get; set; }
    }

    /// <summary>
    /// DTO del usuario (sin información sensible)
    /// </summary>
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? DescripcionRol { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de crear usuario
    /// </summary>
    public class CrearUsuarioResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UsuarioDto? Usuario { get; set; }
    }

    /// <summary>
    /// DTO para información de rol
    /// </summary>
    public class RolDto
    {
        public int Id { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

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

    public class CrearOrdenRequest
    {
        public int TipoOrdenId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int? TipoServicioId { get; set; }
        public int KilometrajeActual { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public string? ObservacionesAsesor { get; set; }
        public List<int>? ServiciosExtraIds { get; set; }
    }



}