using System;
using FacturasSRI.Domain.Entities; // Added for Proveedor

namespace FacturasSRI.Domain.Entities
{
    public class Lote
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;
        public Guid? ProveedorId { get; set; } // Changed to nullable
        public virtual Proveedor? Proveedor { get; set; } // Changed to nullable
        public int CantidadComprada { get; set; }
        public int CantidadDisponible { get; set; }
        public decimal PrecioCompraUnitario { get; set; }
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public Guid UsuarioIdCreador { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}