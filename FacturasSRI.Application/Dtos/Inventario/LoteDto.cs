using System;

namespace FacturasSRI.Application.Dtos.Inventario
{
    public class LoteDto
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public int CantidadComprada { get; set; }
        public int CantidadDisponible { get; set; }
        public decimal PrecioCompraUnitario { get; set; }
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaCaducidad { get; set; }
    }
}
