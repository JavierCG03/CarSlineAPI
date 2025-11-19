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
}