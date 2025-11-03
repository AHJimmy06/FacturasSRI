using FacturasSRI.Application.Dtos.Productos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FacturasSRI.Infrastructure.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly FacturasSRIDbContext _context;

        public ProductoRepository(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductoDto>> GetAllProductsAsync()
        {
            return await _context.Productos
                .Where(p => p.EstaActivo) // Opcional: solo mostrar activos en la lista principal
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    CodigoPrincipal = p.CodigoPrincipal,
                    Nombre = p.Nombre,
                    PrecioVentaUnitario = p.PrecioVentaUnitario,
                    EstaActivo = p.EstaActivo
                })
                .ToListAsync();
        }

        public async Task<Producto?> GetProductByIdAsync(Guid id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<Producto> CreateProductAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }
        
        public async Task UpdateProductAsync(Producto producto)
        {
            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateProductAsync(Producto producto)
        {
            producto.EstaActivo = false;
            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}