using FacturasSRI.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface IProveedorService
    {
        Task<List<ProveedorDto>> GetProveedoresAsync();
        Task<ProveedorDto?> GetProveedorByIdAsync(Guid id); // Changed to nullable
        Task CreateProveedorAsync(ProveedorDto proveedor);
        Task UpdateProveedorAsync(ProveedorDto proveedor);
        Task DeleteProveedorAsync(Guid id); // Logical delete (EstaActivo = false)
    }
}
