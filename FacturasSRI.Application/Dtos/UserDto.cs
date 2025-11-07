using System;
using System.Collections.Generic;

namespace FacturasSRI.Application.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool EstaActivo { get; set; } = true;

        public List<Guid> RolesId { get; set; } = new List<Guid>();
        public List<string> Roles { get; set; } = new List<string>();
    }
}