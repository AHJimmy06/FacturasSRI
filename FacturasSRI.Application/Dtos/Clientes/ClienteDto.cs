using FacturasSRI.Domain.Enums;
using System;

namespace FacturasSRI.Application.Dtos.Clientes
{
    public class ClienteDto
    {
        public Guid Id { get; set; }
        public TipoIdentificacion TipoIdentificacion { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public bool EstaActivo { get; set; }
    }
}