using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FacturasSRI.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly FacturasSRIDbContext _context;

        public ProductService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            var product = new Producto
            {
                Id = Guid.NewGuid(),
                CodigoPrincipal = productDto.CodigoPrincipal,
                Nombre = productDto.Nombre,
                Descripcion = productDto.Descripcion,
                PrecioVentaUnitario = productDto.PrecioVentaUnitario,
                ManejaInventario = productDto.ManejaInventario,
                ManejaLotes = productDto.ManejaLotes,
                UsuarioIdCreador = productDto.UsuarioIdCreador,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();
            productDto.Id = product.Id;
            return productDto;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            return await (from product in _context.Productos
                          where product.Id == id
                          join usuario in _context.Usuarios on product.UsuarioIdCreador equals usuario.Id into usuarioJoin
                          from usuario in usuarioJoin.DefaultIfEmpty()
                          // Left join with ProductoImpuesto to get associated taxes
                          join pi in _context.ProductoImpuestos on product.Id equals pi.ProductoId into productTaxes
                          from pi in productTaxes.DefaultIfEmpty()
                          // Left join with Impuesto to get tax details
                          join tax in _context.Impuestos on pi.ImpuestoId equals tax.Id into taxDetails
                          from tax in taxDetails.DefaultIfEmpty()
                          group new { product, usuario, tax } by product.Id into g // Group by product to handle multiple taxes
                          select new ProductDto
                          {
                              Id = g.Key,
                              CodigoPrincipal = g.First().product.CodigoPrincipal,
                              Nombre = g.First().product.Nombre,
                              Descripcion = g.First().product.Descripcion,
                              PrecioVentaUnitario = g.First().product.PrecioVentaUnitario,
                              ManejaInventario = g.First().product.ManejaInventario,
                              ManejaLotes = g.First().product.ManejaLotes,
                              StockTotal = g.First().product.ManejaLotes ? g.First().product.Lotes.Sum(l => l.CantidadDisponible) : g.First().product.StockTotal,
                              CreadoPor = g.First().usuario != null ? g.First().usuario.PrimerNombre + " " + g.First().usuario.PrimerApellido : "Usuario no encontrado",
                              IsActive = g.First().product.EstaActivo,
                              FechaCreacion = g.First().product.FechaCreacion,
                              FechaModificacion = g.First().product.FechaModificacion,
                              ImpuestoPrincipalNombre = g.Where(x => x.tax != null).Select(x => x.tax!.Nombre).FirstOrDefault() ?? "N/A", // Get first tax name
                              ImpuestoPrincipalPorcentaje = g.Where(x => x.tax != null).Select(x => x.tax!.Porcentaje).FirstOrDefault() // Get first tax percentage
                          }).FirstOrDefaultAsync();
        }

        public async Task<List<ProductDto>> GetProductsAsync()
        {
            return await (from product in _context.Productos
                          join usuario in _context.Usuarios on product.UsuarioIdCreador equals usuario.Id into usuarioJoin
                          from usuario in usuarioJoin.DefaultIfEmpty()
                          // Left join with ProductoImpuesto to get associated taxes
                          join pi in _context.ProductoImpuestos on product.Id equals pi.ProductoId into productTaxes
                          from pi in productTaxes.DefaultIfEmpty()
                          // Left join with Impuesto to get tax details
                          join tax in _context.Impuestos on pi.ImpuestoId equals tax.Id into taxDetails
                          from tax in taxDetails.DefaultIfEmpty()
                          group new { product, usuario, tax } by product.Id into g // Group by product to handle multiple taxes
                          select new ProductDto
                          {
                              Id = g.Key,
                              CodigoPrincipal = g.First().product.CodigoPrincipal,
                              Nombre = g.First().product.Nombre,
                              Descripcion = g.First().product.Descripcion,
                              PrecioVentaUnitario = g.First().product.PrecioVentaUnitario,
                              ManejaInventario = g.First().product.ManejaInventario,
                              ManejaLotes = g.First().product.ManejaLotes,
                              StockTotal = g.First().product.ManejaLotes ? g.First().product.Lotes.Sum(l => l.CantidadDisponible) : g.First().product.StockTotal,
                              CreadoPor = g.First().usuario != null ? g.First().usuario.PrimerNombre + " " + g.First().usuario.PrimerApellido : "Usuario no encontrado",
                              IsActive = g.First().product.EstaActivo,
                              ImpuestoPrincipalNombre = g.Where(x => x.tax != null).Select(x => x.tax!.Nombre).FirstOrDefault() ?? "N/A", // Get first tax name
                              ImpuestoPrincipalPorcentaje = g.Where(x => x.tax != null).Select(x => x.tax!.Porcentaje).FirstOrDefault() // Get first tax percentage
                          }).ToListAsync();
        }

        public async Task<List<ProductDto>> GetActiveProductsAsync()
        {
            return await (from product in _context.Productos
                          join usuario in _context.Usuarios on product.UsuarioIdCreador equals usuario.Id into usuarioJoin
                          from usuario in usuarioJoin.DefaultIfEmpty()
                          // Left join with ProductoImpuesto to get associated taxes
                          join pi in _context.ProductoImpuestos on product.Id equals pi.ProductoId into productTaxes
                          from pi in productTaxes.DefaultIfEmpty()
                          // Left join with Impuesto to get tax details
                          join tax in _context.Impuestos on pi.ImpuestoId equals tax.Id into taxDetails
                          from tax in taxDetails.DefaultIfEmpty()
                          where product.EstaActivo == true // Filter for active products
                          group new { product, usuario, tax } by product.Id into g // Group by product to handle multiple taxes
                          select new ProductDto
                          {
                              Id = g.Key,
                              CodigoPrincipal = g.First().product.CodigoPrincipal,
                              Nombre = g.First().product.Nombre,
                              Descripcion = g.First().product.Descripcion,
                              PrecioVentaUnitario = g.First().product.PrecioVentaUnitario,
                              ManejaInventario = g.First().product.ManejaInventario,
                              ManejaLotes = g.First().product.ManejaLotes,
                              StockTotal = g.First().product.ManejaLotes ? g.First().product.Lotes.Sum(l => l.CantidadDisponible) : g.First().product.StockTotal,
                              CreadoPor = g.First().usuario != null ? g.First().usuario.PrimerNombre + " " + g.First().usuario.PrimerApellido : "Usuario no encontrado",
                              IsActive = g.First().product.EstaActivo,
                              ImpuestoPrincipalNombre = g.Where(x => x.tax != null).Select(x => x.tax!.Nombre).FirstOrDefault() ?? "N/A", // Get first tax name
                              ImpuestoPrincipalPorcentaje = g.Where(x => x.tax != null).Select(x => x.tax!.Porcentaje).FirstOrDefault() // Get first tax percentage
                          }).ToListAsync();
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var product = await _context.Productos.FindAsync(productDto.Id);
            if (product != null)
            {
                product.CodigoPrincipal = productDto.CodigoPrincipal;
                product.Nombre = productDto.Nombre;
                product.Descripcion = productDto.Descripcion;
                product.PrecioVentaUnitario = productDto.PrecioVentaUnitario;
                product.ManejaInventario = productDto.ManejaInventario;
                product.ManejaLotes = productDto.ManejaLotes;
                product.EstaActivo = productDto.IsActive; // Update EstaActivo from ProductDto.IsActive
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignTaxesToProductAsync(Guid productId, List<Guid> taxIds)
        {
            var product = await _context.Productos.Include(p => p.ProductoImpuestos).FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.ProductoImpuestos.Clear();
                foreach (var taxId in taxIds)
                {
                    product.ProductoImpuestos.Add(new ProductoImpuesto { ProductoId = productId, ImpuestoId = taxId });
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product != null)
            {
                product.EstaActivo = !product.EstaActivo; // Toggle the active status
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ProductStockDto?> GetProductStockDetailsAsync(Guid productId)
        {
            var product = await _context.Productos
                .Include(p => p.Lotes)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return null;
            }

            var stockDetails = new ProductStockDto
            {
                ProductId = product.Id,
                ProductName = product.Nombre,
                ManejaLotes = product.ManejaLotes
            };

            if (product.ManejaLotes)
            {
                var lotesActivos = product.Lotes.Where(l => l.CantidadDisponible > 0).ToList();
                stockDetails.TotalStock = lotesActivos.Sum(l => l.CantidadDisponible);
                stockDetails.Lotes = lotesActivos.Select(l => new LoteDto
                {
                    Id = l.Id,
                    CantidadDisponible = l.CantidadDisponible,
                    PrecioCompraUnitario = l.PrecioCompraUnitario,
                    FechaCaducidad = l.FechaCaducidad
                }).ToList();
            }
            else
            {
                stockDetails.TotalStock = product.StockTotal;
            }

            return stockDetails;
        }
    }
}