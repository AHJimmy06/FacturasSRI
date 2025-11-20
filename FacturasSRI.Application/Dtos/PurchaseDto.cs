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

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor que cero.")]
        public decimal MontoTotal { get; set; }
        
        public DateTime? FechaCaducidad { get; set; }
        
        [Required]
        public string NombreProveedor { get; set; } = string.Empty;
        
        public string? FacturaCompraPath { get; set; }

        public Guid UsuarioIdCreador { get; set; }

        // Nuevos campos para compras a crédito
        public bool EsCredito { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }
}