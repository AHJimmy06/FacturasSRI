using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly FacturasSRIDbContext _db;

        public ClienteRepository(FacturasSRIDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await _db.Clientes.AsNoTracking().ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(Guid id)
        {
            return await _db.Clientes.FindAsync(id);
        }

        public async Task<Cliente> AddAsync(Cliente cliente)
        {
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();
            return cliente;
        }

        public async Task UpdateAsync(Cliente cliente)
        {
            _db.Clientes.Update(cliente);
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente != null)
            {
                cliente.EstaActivo = false;
                await _db.SaveChangesAsync();
            }
        }
    }
}
