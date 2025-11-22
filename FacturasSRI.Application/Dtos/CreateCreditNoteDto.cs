using System;
using System.Collections.Generic;

namespace FacturasSRI.Application.Dtos
{
    public class CreateCreditNoteDto
    {
        public Guid FacturaId { get; set; }
        public string RazonModificacion { get; set; } = string.Empty;
        public Guid UsuarioIdCreador { get; set; }
        
        // Lista de items a devolver
        public List<CreditNoteItemDto> Items { get; set; } = new List<CreditNoteItemDto>();
    }

    public class CreditNoteItemDto
    {
        public Guid ProductoId { get; set; }
        public int CantidadDevolucion { get; set; } // Cuántos se devuelven
    }
}