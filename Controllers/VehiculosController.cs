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
