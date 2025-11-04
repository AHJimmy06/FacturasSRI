using System;

namespace FacturasSRI.Application.Dtos.Clientes
{
    public class ClienteDto
    {
        public Guid Id { get; set; }
        public int TipoIdentificacion { get; set; }
        public string NumeroIdentificacion { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public bool EstaActivo { get; set; }
    }
}
