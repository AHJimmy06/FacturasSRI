using System;
using System.Collections.Generic;

namespace FacturasSRI.Domain.Entities
{
    public class Establecimiento
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public virtual Empresa Empresa { get; set; } = null!;
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool EstaActivo { get; set; } = true;

        public virtual ICollection<PuntoEmision> PuntosEmision { get; set; } = new List<PuntoEmision>();
    }
}