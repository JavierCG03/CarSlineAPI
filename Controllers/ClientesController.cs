using CarSlineAPI.Data;
using CarSlineAPI.Models.DTOs;
using CarSlineAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarSlineAPI.Controllers
{
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
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos inválidos" });

            var cliente = new Cliente
            {
                NombreCompleto = req.NombreCompleto,
                RFC = req.RFC,
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

        [HttpPut("actualizar/{id}")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ClienteRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ClienteResponse
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            try
            {
                // Buscar el cliente existente
                var cliente = await _db.Clientes.FindAsync(id);

                if (cliente == null || !cliente.Activo)
                {
                    _logger.LogWarning($"Cliente con ID {id} no encontrado");
                    return NotFound(new ClienteResponse
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    });
                }
                cliente.NombreCompleto = req.NombreCompleto;
                cliente.RFC = req.RFC;
                cliente.TelefonoMovil = req.TelefonoMovil;
                cliente.TelefonoCasa = req.TelefonoCasa;
                cliente.CorreoElectronico = req.CorreoElectronico;
                cliente.Colonia = req.Colonia;
                cliente.Calle = req.Calle;
                cliente.NumeroExterior = req.NumeroExterior;
                cliente.Municipio = req.Municipio;
                cliente.Estado = req.Estado;
                cliente.Pais = string.IsNullOrWhiteSpace(req.Pais) ? "México" : req.Pais;
                cliente.CodigoPostal = req.CodigoPostal;

                await _db.SaveChangesAsync();

                _logger.LogInformation($"Cliente ID {id} actualizado exitosamente");

                return Ok(new ClienteResponse
                {
                    Success = true,
                    Message = "Cliente actualizado exitosamente",
                    ClienteId = cliente.Id
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error al actualizar cliente ID {id}");
                return StatusCode(500, new ClienteResponse
                {
                    Success = false,
                    Message = "Error al actualizar cliente"
                });
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
                    cliente.RFC,
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

}
