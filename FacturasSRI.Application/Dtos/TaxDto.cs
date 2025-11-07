using System;

namespace FacturasSRI.Application.Dtos
{
    public class TaxDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoSRI { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool EstaActivo { get; set; } = true;
    }
}
