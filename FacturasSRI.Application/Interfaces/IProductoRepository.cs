using FacturasSRI.Application.Dtos.Productos;
using FacturasSRI.Domain.Entities;

namespace FacturasSRI.Application.Interfaces
{
    public interface IProductoRepository
    {
        Task<IEnumerable<ProductoDto>> GetAllProductsAsync();
        Task<Producto?> GetProductByIdAsync(Guid id);
        Task<Producto> CreateProductAsync(Producto producto);
        Task UpdateProductAsync(Producto producto);
        Task DeactivateProductAsync(Producto producto);
    }
}