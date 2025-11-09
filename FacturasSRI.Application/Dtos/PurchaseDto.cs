using System;
using System.ComponentModel.DataAnnotations;

namespace FacturasSRI.Application.Dtos
{
    public class PurchaseDto
    {
        [Required]
        public Guid ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal PrecioCosto { get; set; }
        public DateTime? FechaCaducidad { get; set; }

        [Required]
        public Guid ProveedorId { get; set; } // Changed from string Proveedor

        [Required]
        public string NumeroFactura { get; set; } = string.Empty;
        public Guid UsuarioIdCreador { get; set; }
    }
}