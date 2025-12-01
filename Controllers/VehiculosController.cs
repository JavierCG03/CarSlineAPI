using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
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

        [HttpPut("actualizar-placas/{id}")]
        [ProducesResponseType(typeof(VehiculoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPlacasVehiculo(int id, [FromBody] VehiculoRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Placas))
            {
                return BadRequest(new VehiculoResponse
                {
                    Success = false,
                    Message = "Las placas son requeridas"
                });
            }

            try
            {
                // Buscar el vehículo existente
                var vehiculo = await _db.Vehiculos.FindAsync(id);

                if (vehiculo == null || !vehiculo.Activo)
                {
                    _logger.LogWarning($"Vehículo con ID {id} no encontrado");
                    return NotFound(new VehiculoResponse
                    {
                        Success = false,
                        Message = "Vehículo no encontrado"
                    });
                }

                // Actualizar solo las placas
                vehiculo.Placas = req.Placas.ToUpperInvariant();

                await _db.SaveChangesAsync();

                _logger.LogInformation($"Placas del vehículo ID {id} actualizadas a: {vehiculo.Placas}");

                return Ok(new VehiculoResponse
                {
                    Success = true,
                    Message = "Placas actualizadas exitosamente",
                    VehiculoId = vehiculo.Id
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error al actualizar placas del vehículo ID {id}");
                return StatusCode(500, new VehiculoResponse
                {
                    Success = false,
                    Message = "Error al actualizar placas"
                });
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
}
