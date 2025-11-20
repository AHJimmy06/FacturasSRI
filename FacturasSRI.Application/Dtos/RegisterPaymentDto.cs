using System;
using System.ComponentModel.DataAnnotations;

namespace FacturasSRI.Application.Dtos
{
    public class RegisterPaymentDto
    {
        [Required]
        public Guid PurchaseId { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }
        
        [Required]
        public Guid UsuarioId { get; set; }
    }
}
