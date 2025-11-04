using System;

namespace FacturasSRI.Application.Dtos.Clientes
{
    public class CreateClienteDto
    {
        public int TipoIdentificacion { get; set; }
        public string NumeroIdentificacion { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
    }
}
