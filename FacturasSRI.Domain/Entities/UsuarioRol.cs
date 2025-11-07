using System;

namespace FacturasSRI.Domain.Entities
{
    public class UsuarioRol
    {
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;
        public Guid RolId { get; set; }
        public virtual Rol Rol { get; set; } = null!;
    }
}