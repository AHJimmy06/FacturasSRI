using System;

namespace FacturasSRI.Application.Dtos
{
    public class RegistrarCobroDto
    {
        public Guid FacturaId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoDePago { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public Guid UsuarioIdCreador { get; set; }
        public DateTime FechaCobro { get; set; } = DateTime.UtcNow;
    }
}
