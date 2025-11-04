using FacturasSRI.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario> AddAsync(Usuario usuario);
    }
}