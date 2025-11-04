using FacturasSRI.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface ILoteRepository
    {
        Task<Lote> AddAsync(Lote lote);
        Task<Lote?> GetByIdAsync(Guid id);
        Task<bool> ProductoExistsAsync(Guid productoId);
    }
}
