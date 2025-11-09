using FacturasSRI.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface ITaxService
    {
        Task<List<TaxDto>> GetTaxesAsync();
        Task<TaxDto?> GetTaxByIdAsync(Guid id);
        Task<TaxDto> CreateTaxAsync(TaxDto tax);
        Task UpdateTaxAsync(TaxDto tax);
        Task DeleteTaxAsync(Guid id);
        Task<List<TaxDto>> GetActiveTaxesAsync(); // New method
    }
}
