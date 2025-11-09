using FacturasSRI.Domain.Enums;
using System;

namespace FacturasSRI.Application.Dtos
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public TipoIdentificacion TipoIdentificacion { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool EstaActivo { get; set; } = true;
        public string CreadoPor { get; set; } = string.Empty;
        public Guid UsuarioIdCreador { get; set; }
    }
}
