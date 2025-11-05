using FacturasSRI.Application.Dtos.Inventario;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Application.Mappings;
using FacturasSRI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace FacturasSRI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Bodeguero")]
    public class InventarioController : ControllerBase
    {
        private readonly ILoteRepository _loteRepository;

        public InventarioController(ILoteRepository loteRepository)
        {
            _loteRepository = loteRepository;
        }

        [HttpGet("lotes")]
        public async Task<IActionResult> GetAll()
        {
            var lotes = await _loteRepository.GetAllAsync();
            var loteDtos = lotes.Select(l => new LoteDto
            {
                Id = l.Id,
                ProductoId = l.ProductoId,
                CantidadComprada = l.CantidadComprada,
                CantidadDisponible = l.CantidadDisponible,
                PrecioCompraUnitario = l.PrecioCompraUnitario,
                FechaCompra = l.FechaCompra,
                FechaCaducidad = l.FechaCaducidad
            });

            return Ok(loteDtos);
        }

        // POST: /api/inventario/compra
        [HttpPost("compra")]
        public async Task<IActionResult> RegistrarCompra([FromBody] CreateLoteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (dto.Cantidad <= 0 || dto.PrecioCompra < 0)
                return BadRequest("La cantidad y el precio deben ser valores positivos.");

            var productoExiste = await _loteRepository.ProductoExistsAsync(dto.ProductoId);
            if (!productoExiste)
                return NotFound($"El producto con ID {dto.ProductoId} no fue encontrado.");

            var lote = new Lote
            {
                Id = Guid.NewGuid(),
                ProductoId = dto.ProductoId,
                CantidadComprada = dto.Cantidad,
                CantidadDisponible = dto.Cantidad,
                PrecioCompraUnitario = dto.PrecioCompra,
                FechaCompra = dto.FechaCompra ?? DateTime.UtcNow,
                FechaCaducidad = dto.FechaCaducidad,
                UsuarioIdCreador = Guid.Parse(userId),
                FechaCreacion = DateTime.UtcNow
            };

            var nuevoLote = await _loteRepository.AddAsync(lote);

            return CreatedAtAction(nameof(GetLoteById), new { id = nuevoLote.Id }, nuevoLote.ToDto());
        }

        [HttpGet("lotes/{id}")]
        public async Task<IActionResult> GetLoteById(Guid id)
        {
            var lote = await _loteRepository.GetByIdAsync(id);
            if (lote == null)
                return NotFound();

            return Ok(lote.ToDto());
        }
    }
}