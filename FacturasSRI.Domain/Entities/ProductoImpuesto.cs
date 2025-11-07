using System;

namespace FacturasSRI.Domain.Entities
{
    public class ProductoImpuesto
    {
        public Guid ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;
        public Guid ImpuestoId { get; set; }
        public virtual Impuesto Impuesto { get; set; } = null!;
    }
}