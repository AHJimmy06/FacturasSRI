using System;
using System.Collections.Generic;

namespace FacturasSRI.Domain.Entities
{
    public class Impuesto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoSRI { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool EstaActivo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        
        public virtual ICollection<ProductoImpuesto> ProductoImpuestos { get; set; } = new List<ProductoImpuesto>();
    }
}