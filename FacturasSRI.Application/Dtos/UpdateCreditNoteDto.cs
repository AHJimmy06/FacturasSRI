using System;
using System.Collections.Generic;

namespace FacturasSRI.Application.Dtos
{
    public class UpdateCreditNoteDto
    {
        public Guid Id { get; set; }
        public string RazonModificacion { get; set; } = string.Empty;
        public Guid UsuarioIdModificador { get; set; }
        public List<CreditNoteItemDto> Items { get; set; } = new List<CreditNoteItemDto>();
        public bool EmitirTrasGuardar { get; set; }
    }
}
