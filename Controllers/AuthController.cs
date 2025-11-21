using CarSlineAPI.Data;
using CarSlineAPI.Models;
using CarSlineAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
    /// <summary>
    /// Controlador de autenticación y gestión de usuarios
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login de usuario
        /// POST api/Auth/login
        /// </summary>
        /// <param name="request">Credenciales de usuario</param>
        /// <returns>Información del usuario autenticado</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });
            }

            _logger.LogInformation($"Intento de login para: {request.NombreUsuario}");

            var response = await _authService.Login(request);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Crear nuevo usuario (Solo Administrador)
        /// POST api/Auth/crear-usuario
        /// </summary>
        /// <param name="request">Datos del nuevo usuario</param>
        /// <param name="adminId">ID del administrador que crea el usuario</param>
        /// <returns>Información del usuario creado</returns>
        [HttpPost("crear-usuario")]
        [ProducesResponseType(typeof(CrearUsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CrearUsuarioResponse>> CrearUsuario(
            [FromBody] CrearUsuarioRequest request,
            [FromHeader(Name = "X-Admin-Id")] int adminId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CrearUsuarioResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });
            }

            if (adminId <= 0)
            {
                return Unauthorized(new CrearUsuarioResponse
                {
                    Success = false,
                    Message = "ID de administrador no válido"
                });
            }

            _logger.LogInformation($"Intento de crear usuario: {request.NombreUsuario} por Admin ID: {adminId}");

            var response = await _authService.CrearUsuario(request, adminId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtener roles disponibles
        /// GET api/Auth/roles
        /// </summary>
        /// <returns>Lista de roles (excepto Administrador)</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RolDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RolDto>>> ObtenerRoles()
        {
            _logger.LogInformation("Consultando roles disponibles");
            var roles = await _authService.ObtenerRolesDisponibles();
            return Ok(roles);
        }

        /// <summary>
        /// Obtener todos los usuarios (Solo Administrador)
        /// GET api/Auth/usuarios
        /// </summary>
        /// <param name="adminId">ID del administrador</param>
        /// <returns>Lista de usuarios</returns>
        [HttpGet("usuarios")]
        [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<UsuarioDto>>> ObtenerUsuarios(
            [FromHeader(Name = "X-Admin-Id")] int adminId)
        {
            if (adminId <= 0)
            {
                return Unauthorized(new { message = "ID de administrador no válido" });
            }

            _logger.LogInformation($"Consultando usuarios por Admin ID: {adminId}");
            var usuarios = await _authService.ObtenerTodosLosUsuarios(adminId);
            return Ok(usuarios);
        }

        /// <summary>
        /// Verificar estado de la API
        /// GET api/Auth/health
        /// </summary>
        /// <returns>Estado de la API</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "OK",
                service = "CarSline API",
                timestamp = DateTime.Now,
                message = "API funcionando correctamente"
            });
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(ApplicationDbContext db, ILogger<ClientesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var cliente = new Cliente
            {
                NombreCompleto = req.NombreCompleto,
                TelefonoMovil = req.TelefonoMovil,
                TelefonoCasa = req.TelefonoCasa,
                CorreoElectronico = req.CorreoElectronico,
                Colonia = req.Colonia,
                Calle = req.Calle,
                NumeroExterior = req.NumeroExterior,
                Municipio = req.Municipio,
                Estado = req.Estado,
                Pais = string.IsNullOrWhiteSpace(req.Pais) ? "México" : req.Pais,
                CodigoPostal = req.CodigoPostal,
                Activo = true
            };

            try
            {
                _db.Clientes.Add(cliente);
                await _db.SaveChangesAsync();
                return Ok(new { Success = true, ClienteId = cliente.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                return StatusCode(500, new { Success = false, Message = "Error al registrar cliente" });
            }
        }

        [HttpGet("buscar-telefono/{telefono}")]
        public async Task<IActionResult> BuscarPorTelefono(string telefono)
        {
            var cliente = await _db.Clientes
                .Where(c => c.TelefonoMovil == telefono && c.Activo)
                .FirstOrDefaultAsync();

            if (cliente == null) return Ok(new { Success = false, Message = "Cliente no encontrado" });

            return Ok(new
            {
                Success = true,
                Cliente = new
                {
                    cliente.Id,
                    cliente.NombreCompleto,
                    cliente.TelefonoMovil,
                    cliente.TelefonoCasa,
                    cliente.CorreoElectronico,
                    cliente.Colonia,
                    cliente.Calle,
                    cliente.NumeroExterior,
                    cliente.Municipio,
                    cliente.Estado,
                    cliente.Pais,
                    cliente.CodigoPostal
                }
            });
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class VehiculosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<VehiculosController> _logger;

        public VehiculosController(ApplicationDbContext db, ILogger<VehiculosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> CrearVehiculo([FromBody] VehiculoRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var veh = new Vehiculo
            {
                ClienteId = req.ClienteId,
                VIN = req.VIN?.ToUpperInvariant() ?? string.Empty,
                Marca = req.Marca,
                Modelo = req.Modelo,
                Anio = req.Anio,
                Color = req.Color,
                Placas = req.Placas?.ToUpperInvariant(),
                KilometrajeInicial = req.KilometrajeInicial,
                Activo = true
            };

            try
            {
                _db.Vehiculos.Add(veh);
                await _db.SaveChangesAsync();
                return Ok(new { Success = true, VehiculoId = veh.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al crear vehículo");
                return StatusCode(500, new { Success = false, Message = "Error al registrar vehículo" });
            }
        }

        [HttpGet("buscar-vin/{vin}")]
        public async Task<IActionResult> BuscarPorVIN(string vin)
        {
            var veh = await _db.Vehiculos
                .Include(v => v.Cliente)
                .Where(v => v.VIN == vin.ToUpperInvariant() && v.Activo)
                .FirstOrDefaultAsync();

            if (veh == null) return Ok(new { Success = false, Message = "Vehículo no encontrado" });

            return Ok(new
            {
                Success = true,
                Vehiculo = new
                {
                    veh.Id,
                    veh.ClienteId,
                    veh.VIN,
                    veh.Marca,
                    veh.Modelo,
                    veh.Anio,
                    veh.Color,
                    veh.Placas,
                    veh.KilometrajeInicial,
                    NombreCliente = veh.Cliente?.NombreCompleto ?? ""
                }
            });
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CatalogosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CatalogosController> _logger;

        public CatalogosController(ApplicationDbContext db, ILogger<CatalogosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("tipos-servicio")]
        public async Task<IActionResult> ObtenerTiposServicio()
        {
            var tipos = await _db.TiposServicio
                .Where(t => t.Activo)
                .Select(t => new { t.Id, Nombre = t.NombreServicio, t.Descripcion, Precio = t.PrecioBase })
                .ToListAsync();

            return Ok(tipos);
        }

        [HttpGet("servicios-extra")]
        public async Task<IActionResult> ObtenerServiciosExtra()
        {
            var servicios = await _db.ServiciosExtra
                .Where(s => s.Activo)
                .OrderBy(s => s.Categoria).ThenBy(s => s.NombreServicio)
                .Select(s => new { s.Id, Nombre = s.NombreServicio, Descripcion = s.Descripcion, Precio = s.Precio, Categoria = s.Categoria })
                .ToListAsync();

            return Ok(servicios);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class OrdenesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrdenesController> _logger;

        public OrdenesController(ApplicationDbContext db, ILogger<OrdenesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request, [FromHeader(Name = "X-User-Id")] int asesorId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    // prefijo
                    var prefijo = request.TipoOrdenId switch
                    {
                        1 => "SRV",
                        2 => "DIA",
                        3 => "REP",
                        4 => "GAR",
                        _ => "ORD"
                    };

                    // obtener max correlativo
                    var maxNumero = await _db.Ordenes
                        .Where(o => o.NumeroOrden.StartsWith(prefijo + "-"))
                        .Select(o => o.NumeroOrden)
                        .ToListAsync();

                    int siguiente = 1;
                    if (maxNumero.Any())
                    {
                        var maxInt = maxNumero
                            .Select(s =>
                            {
                                var parts = s.Split('-', 2);
                                if (parts.Length < 2) return 0;
                                return int.TryParse(parts[1], out var n) ? n : 0;
                            })
                            .DefaultIfEmpty(0)
                            .Max();
                        siguiente = maxInt + 1;
                    }

                    var numeroOrden = $"{prefijo}-{siguiente:D6}";

                    // costo base
                    decimal costoTotal = 0;
                    if (request.TipoServicioId.HasValue)
                    {
                        var tipo = await _db.TiposServicio.FindAsync(request.TipoServicioId.Value);
                        if (tipo != null) costoTotal = tipo.PrecioBase;
                    }

                    // costo extras
                    if (request.ServiciosExtraIds != null && request.ServiciosExtraIds.Any())
                    {
                        var extras = await _db.ServiciosExtra
                            .Where(e => request.ServiciosExtraIds.Contains(e.Id))
                            .ToListAsync();

                        costoTotal += extras.Sum(e => e.Precio);
                    }

                    var orden = new Orden
                    {
                        NumeroOrden = numeroOrden,
                        TipoOrdenId = request.TipoOrdenId,
                        ClienteId = request.ClienteId,
                        VehiculoId = request.VehiculoId,
                        AsesorId = asesorId,
                        TipoServicioId = request.TipoServicioId,
                        KilometrajeActual = request.KilometrajeActual,
                        EstadoOrdenId = 1,
                        FechaHoraPromesaEntrega = request.FechaHoraPromesaEntrega,
                        ObservacionesAsesor = request.ObservacionesAsesor,
                        CostoTotal = costoTotal,
                        FechaCreacion = DateTime.Now,
                        Activo = true
                    };

                    _db.Ordenes.Add(orden);
                    await _db.SaveChangesAsync();

                    // insertar servicios extra aplicados
                    if (request.ServiciosExtraIds != null && request.ServiciosExtraIds.Any())
                    {
                        var extras = await _db.ServiciosExtra
                            .Where(e => request.ServiciosExtraIds.Contains(e.Id))
                            .ToListAsync();

                        foreach (var se in extras)
                        {
                            var oe = new OrdenServicioExtra
                            {
                                OrdenId = orden.Id,
                                ServicioExtraId = se.Id,
                                PrecioAplicado = se.Precio
                            };
                            _db.OrdenesServiciosExtra.Add(oe);
                        }

                        await _db.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Ok(new { Success = true, NumeroOrden = numeroOrden, OrdenId = orden.Id, CostoTotal = costoTotal });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear orden");
                    return StatusCode(500, new { Success = false, Message = "Error al crear orden" });
                }
            });
        }
        [HttpGet("asesor/{tipoOrdenId}")]
        public async Task<IActionResult> ObtenerOrdenesPorTipo(int tipoOrdenId, [FromHeader(Name = "X-User-Id")] int asesorId)
        {
            var ordenes = await _db.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Include(o => o.TipoServicio)
                //.Include(o => o.Tecnico)
                .Where(o => o.TipoOrdenId == tipoOrdenId
                         && o.AsesorId == asesorId
                         && o.Activo
                         && new[] { 1, 2, 3 }.Contains(o.EstadoOrdenId))
                .OrderBy(o => o.FechaHoraPromesaEntrega)
                .Select(o => new
                {
                    o.Id,
                    o.NumeroOrden,
                    VehiculoCompleto = $"{o.Vehiculo.Marca} {o.Vehiculo.Modelo} {o.Vehiculo.Anio}",
                    ClienteNombre = o.Cliente.NombreCompleto,
                    ClienteTelefono = o.Cliente.TelefonoMovil,
                    HoraPromesa = o.FechaHoraPromesaEntrega.ToString("HH:mm"),
                    HoraInicio = o.FechaInicioProceso.HasValue ? o.FechaInicioProceso.Value.ToString("HH:mm") : "-",
                    HoraFin = o.FechaFinalizacion.HasValue ? o.FechaFinalizacion.Value.ToString("HH:mm") : "-",
                    TipoServicio = o.TipoServicio != null ? o.TipoServicio.NombreServicio : "Sin servicio",
                    //NombreTecnico = o.Tecnico != null ? o.Tecnico.NombreCompleto : "Sin asignar",
                    o.CostoTotal,
                    EstadoId = o.EstadoOrdenId  // ⭐ Mapeo clave
                })
                .ToListAsync();

            return Ok(ordenes);
        }
        /*[HttpGet("asesor/{tipoOrdenId}")]
        public async Task<IActionResult> ObtenerOrdenesPorTipo(int tipoOrdenId, [FromHeader(Name = "X-User-Id")] int asesorId)
        {
            var ordenes = await _db.Ordenes
                .Where(o => o.TipoOrdenId == tipoOrdenId && o.AsesorId == asesorId && o.Activo && new[] { 1, 2, 3 }.Contains(o.EstadoOrdenId))
                .OrderBy(o => o.FechaHoraPromesaEntrega)
                .Select(o => new
                {
                    o.Id,
                    o.NumeroOrden,
                    o.FechaCreacion,
                    FechaPromesaEntrega = o.FechaHoraPromesaEntrega,
                    o.FechaInicioProceso,
                    o.FechaFinalizacion,
                    o.KilometrajeActual,
                    o.CostoTotal,
                    o.ClienteId,
                    o.VehiculoId,
                    o.TipoServicioId,
                    o.EstadoOrdenId,
                    o.ObservacionesAsesor
                })
                .ToListAsync();

            return Ok(ordenes);
        }*/

        [HttpPut("cancelar/{ordenId}")]
        public async Task<IActionResult> CancelarOrden(int ordenId)
        {
            var orden = await _db.Ordenes.FindAsync(ordenId);
            if (orden == null) return NotFound(new { Success = false, Message = "Orden no encontrada" });

            orden.EstadoOrdenId = 5;
            orden.Activo = false;
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Orden cancelada" });
        }

        [HttpPut("entregar/{ordenId}")]
        public async Task<IActionResult> EntregarOrden(int ordenId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var orden = await _db.Ordenes.FindAsync(ordenId);
                if (orden == null) return NotFound(new { Success = false, Message = "Orden no encontrada" });

                orden.EstadoOrdenId = 4;
                orden.FechaEntrega = DateTime.Now;
                await _db.SaveChangesAsync();

                if (orden.TipoServicioId.HasValue)
                {
                    // lógica para calcular próximo servicio — aquí llamamos una función simple de ejemplo.
                    // Si tienes un stored procedure en BD, lo puedes reproducir aquí.
                    int proximoKm = orden.KilometrajeActual + 10000; // ejemplo
                    DateTime? proximaFecha = DateTime.Now.AddMonths(6);

                    var historial = new HistorialServicio
                    {
                        VehiculoId = orden.VehiculoId,
                        OrdenId = orden.Id,
                        TipoServicioId = orden.TipoServicioId.Value,
                        KilometrajeRegistrado = orden.KilometrajeActual,
                        FechaServicio = DateTime.Now,
                        ProximoServicioKm = proximoKm,
                        ProximoServicioFecha = proximaFecha,
                        CostoTotal = orden.CostoTotal
                    };
                    _db.HistorialServicios.Add(historial);
                    await _db.SaveChangesAsync();
                }

                await tx.CommitAsync();
                return Ok(new { Success = true, Message = "Vehículo entregado y registrado en historial" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error al entregar orden");
                return StatusCode(500, new { Success = false, Message = "Error al entregar orden" });
            }
        }
    }
}