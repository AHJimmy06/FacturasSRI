using System;

namespace FacturasSRI.Application.Dtos.Inventario
{
    public class CreateLoteDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public DateTime? FechaCompra { get; set; }
        // Opcional: quien registra la compra (si aplica)
        public Guid? UsuarioId { get; set; }
    }
}
