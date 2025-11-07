using System;

namespace FacturasSRI.Domain.Entities
{
    public class PuntoEmision
    {
        public Guid Id { get; set; }
        public Guid EstablecimientoId { get; set; }
        public virtual Establecimiento Establecimiento { get; set; } = null!;
        public string Codigo { get; set; } = string.Empty;
        public int SecuencialFactura { get; set; }
        public int SecuencialNotaCredito { get; set; }
        public bool EstaActivo { get; set; } = true;
    }
}