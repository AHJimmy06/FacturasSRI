using FacturasSRI.Application.Dtos;
using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly FacturasSRIDbContext _context;

        public CustomerService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
        {
            var customer = new Cliente
            {
                Id = Guid.NewGuid(),
                TipoIdentificacion = customerDto.TipoIdentificacion,
                NumeroIdentificacion = customerDto.NumeroIdentificacion,
                RazonSocial = customerDto.RazonSocial,
                Email = customerDto.Email,
                Direccion = customerDto.Direccion,
                Telefono = customerDto.Telefono,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Clientes.Add(customer);
            await _context.SaveChangesAsync();
            customerDto.Id = customer.Id;
            return customerDto;
        }

        public async Task DeleteCustomerAsync(Guid id)
        {
            var customer = await _context.Clientes.FindAsync(id);
            if (customer != null)
            {
                customer.EstaActivo = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _context.Clientes.FindAsync(id);
            if (customer == null || !customer.EstaActivo)
            {
                return null;
            }
            return new CustomerDto
            {
                Id = customer.Id,
                TipoIdentificacion = customer.TipoIdentificacion,
                NumeroIdentificacion = customer.NumeroIdentificacion,
                RazonSocial = customer.RazonSocial,
                Email = customer.Email,
                Direccion = customer.Direccion,
                Telefono = customer.Telefono,
                EstaActivo = customer.EstaActivo
            };
        }

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            return await _context.Clientes.Where(c => c.EstaActivo).Select(customer => new CustomerDto
            {
                Id = customer.Id,
                TipoIdentificacion = customer.TipoIdentificacion,
                NumeroIdentificacion = customer.NumeroIdentificacion,
                RazonSocial = customer.RazonSocial,
                Email = customer.Email,
                Direccion = customer.Direccion,
                Telefono = customer.Telefono,
                EstaActivo = customer.EstaActivo
            }).ToListAsync();
        }

        public async Task UpdateCustomerAsync(CustomerDto customerDto)
        {
            var customer = await _context.Clientes.FindAsync(customerDto.Id);
            if (customer != null)
            {
                customer.TipoIdentificacion = customerDto.TipoIdentificacion;
                customer.NumeroIdentificacion = customerDto.NumeroIdentificacion;
                customer.RazonSocial = customerDto.RazonSocial;
                customer.Email = customerDto.Email;
                customer.Direccion = customerDto.Direccion;
                customer.Telefono = customerDto.Telefono;
                await _context.SaveChangesAsync();
            }
        }
    }
}
