using FacturasSRI.Application.Dtos;
using FacturasSRI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface ICreditNoteService
    {
        Task<List<CreditNoteDto>> GetCreditNotesAsync();
        Task<CreditNoteDetailViewDto?> GetCreditNoteDetailByIdAsync(Guid id);
        Task<NotaDeCredito> CreateCreditNoteAsync(CreateCreditNoteDto dto);
        Task CheckSriStatusAsync(Guid ncId);
        Task CancelCreditNoteAsync(Guid creditNoteId);
        Task ReactivateCancelledCreditNoteAsync(Guid creditNoteId);
        Task<CreditNoteDetailViewDto?> IssueDraftCreditNoteAsync(Guid creditNoteId);
        Task<CreditNoteDto?> UpdateCreditNoteAsync(UpdateCreditNoteDto dto);
    }
}
