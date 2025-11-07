using System;

namespace FacturasSRI.Application.Dtos
{
    public class InvoiceDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioVentaUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
    }
}
