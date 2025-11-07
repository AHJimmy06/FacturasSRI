using System;
using System.Collections.Generic;

namespace FacturasSRI.Domain.Entities
{
    public class Rol
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool EstaActivo { get; set; } = true;

        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
        public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
}