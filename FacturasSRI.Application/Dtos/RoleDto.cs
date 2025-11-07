using System;

namespace FacturasSRI.Application.Dtos
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool EstaActivo { get; set; } = true;
    }
}
