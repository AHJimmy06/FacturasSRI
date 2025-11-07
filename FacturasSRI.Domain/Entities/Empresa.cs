using System;
using System.Collections.Generic;

namespace FacturasSRI.Domain.Entities
{
    public class Empresa
    {
        public Guid Id { get; set; }
        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string DireccionMatriz { get; set; } = string.Empty;
        public string ContribuyenteEspecial { get; set; } = string.Empty;
        public bool ObligadoContabilidad { get; set; }
        public byte[]? Logo { get; set; }
        public bool EstaActiva { get; set; } = true;

        public virtual ICollection<Establecimiento> Establecimientos { get; set; } = new List<Establecimiento>();
    }
}