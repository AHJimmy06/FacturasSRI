using System;
using System.Collections.Generic;
using FacturasSRI.Domain.Enums; // Added for TipoIdentificacion

namespace FacturasSRI.Application.Dtos
{
    public class CreateInvoiceDto
    {
        public Guid? ClienteId { get; set; } // Made nullable
        public Guid UsuarioIdCreador { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();

        // Optional fields for ConsumidorFinal
        public TipoIdentificacion? TipoIdentificacionComprador { get; set; }
        public string? RazonSocialComprador { get; set; }
        public string? IdentificacionComprador { get; set; }
        public string? DireccionComprador { get; set; }
        public string? EmailComprador { get; set; }
    }
}