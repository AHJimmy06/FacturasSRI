using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FacturasSRI.Domain.Entities
{
    public class Proveedor
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string RUC { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(250)]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public bool EstaActivo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioIdCreador { get; set; }

        public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
