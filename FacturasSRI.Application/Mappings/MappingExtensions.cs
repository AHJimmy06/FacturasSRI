using FacturasSRI.Application.Dtos.Clientes;
using FacturasSRI.Application.Dtos.Inventario;
using FacturasSRI.Application.Dtos.Productos;
using FacturasSRI.Domain.Entities;
using System.Linq;

namespace FacturasSRI.Application.Mappings
{
    public static class MappingExtensions
    {
        public static ClienteDto ToDto(this Cliente cliente)
        {
            return new ClienteDto
            {
                Id = cliente.Id,
                TipoIdentificacion = cliente.TipoIdentificacion,
                NumeroIdentificacion = cliente.NumeroIdentificacion,
                RazonSocial = cliente.RazonSocial,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                EstaActivo = cliente.EstaActivo
            };
        }

        public static ProductoDto ToDto(this Producto producto)
        {
            return new ProductoDto
            {
                Id = producto.Id,
                CodigoPrincipal = producto.CodigoPrincipal,
                Nombre = producto.Nombre,
                PrecioVentaUnitario = producto.PrecioVentaUnitario,
                EstaActivo = producto.EstaActivo
            };
        }
        
        public static LoteDto ToDto(this Lote lote)
        {
            return new LoteDto
            {
                Id = lote.Id,
                ProductoId = lote.ProductoId,
                CantidadComprada = lote.CantidadComprada,
                CantidadDisponible = lote.CantidadDisponible,
                PrecioCompraUnitario = lote.PrecioCompraUnitario,
                FechaCompra = lote.FechaCompra,
                FechaCaducidad = lote.FechaCaducidad
            };
        }
    }
}