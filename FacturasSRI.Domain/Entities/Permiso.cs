using System;
using System.Collections.Generic;

namespace FacturasSRI.Domain.Entities
{
    public class Permiso
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public virtual ICollection<Rol> Roles { get; set; } = new List<Rol>();
    }
}