using FacturasSRI.Application.Dtos.Productos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Application.Mappings;
using FacturasSRI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacturasSRI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;

        public ProductosController(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
        {
            var productos = await _productoRepository.GetAllProductsAsync();
            return Ok(productos.Select(p => p.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetById(Guid id)
        {
            var producto = await _productoRepository.GetProductByIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            return Ok(producto.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<ProductoDto>> Create([FromBody] CreateProductoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var nuevoProducto = new Producto
            {
                Id = Guid.NewGuid(),
                CodigoPrincipal = dto.CodigoPrincipal,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioVentaUnitario = dto.PrecioVentaUnitario,
                TipoImpuestoIVA = dto.TipoImpuestoIVA,
                Tipo = Domain.Enums.TipoProducto.Simple,
                EstaActivo = true,
                FechaCreacion = DateTime.UtcNow,
                UsuarioIdCreador = Guid.Parse(userId)
            };

            await _productoRepository.CreateProductAsync(nuevoProducto);

            return CreatedAtAction(nameof(GetById), new { id = nuevoProducto.Id }, nuevoProducto.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductoDto dto)
        {
            var productoExistente = await _productoRepository.GetProductByIdAsync(id);
            if (productoExistente == null)
            {
                return NotFound();
            }

            productoExistente.Nombre = dto.Nombre;
            productoExistente.Descripcion = dto.Descripcion;
            productoExistente.PrecioVentaUnitario = dto.PrecioVentaUnitario;

            await _productoRepository.UpdateProductAsync(productoExistente);

            return NoContent(); 
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var producto = await _productoRepository.GetProductByIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            await _productoRepository.DeactivateProductAsync(id);
            return NoContent();
        }
    }
}