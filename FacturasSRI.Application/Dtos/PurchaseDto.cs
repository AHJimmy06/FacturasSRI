using System;

namespace FacturasSRI.Application.Dtos
{
    public class PurchaseDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCosto { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string NumeroFactura { get; set; } = string.Empty;
        public Guid UsuarioIdCreador { get; set; }
    }
}
