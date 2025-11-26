using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
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
            var orden = await _db.Ordenes.FindAsync(ordenId);
            if (orden == null) return NotFound(new { Success = false, Message = "Orden no encontrada" });

            orden.EstadoOrdenId = 4;
            orden.Activo = false;
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Orden Entregada" });
        }

        /*
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
        */
    }

}
