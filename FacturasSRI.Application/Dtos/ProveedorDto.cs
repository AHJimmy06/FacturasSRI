using System;
using System.ComponentModel.DataAnnotations;

namespace FacturasSRI.Application.Dtos
{
    public class ProveedorDto
    {
        public Guid Id { get; set; }

        [Required]
        public string RUC { get; set; } = string.Empty;

        [Required]
        public string RazonSocial { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool EstaActivo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioIdCreador { get; set; }
        public string CreadoPor { get; set; } = string.Empty; // For display purposes
    }
}
