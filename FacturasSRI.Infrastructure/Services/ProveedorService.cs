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
    public class ProveedorService : IProveedorService
    {
        private readonly FacturasSRIDbContext _context;

        public ProveedorService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProveedorDto>> GetProveedoresAsync()
        {
            return await _context.Proveedores
                                 .Where(p => p.EstaActivo)
                                 .Select(p => new ProveedorDto
                                 {
                                     Id = p.Id,
                                     RUC = p.RUC,
                                     RazonSocial = p.RazonSocial,
                                     Direccion = p.Direccion,
                                     Telefono = p.Telefono,
                                     Email = p.Email,
                                     EstaActivo = p.EstaActivo,
                                     FechaCreacion = p.FechaCreacion,
                                     UsuarioIdCreador = p.UsuarioIdCreador,
                                     // CreadoPor will be mapped in application layer or UI if needed
                                 })
                                 .ToListAsync();
        }

        public async Task<ProveedorDto?> GetProveedorByIdAsync(Guid id) // Changed to nullable
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return null;

            return new ProveedorDto
            {
                Id = proveedor.Id,
                RUC = proveedor.RUC,
                RazonSocial = proveedor.RazonSocial,
                Direccion = proveedor.Direccion,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                EstaActivo = proveedor.EstaActivo,
                FechaCreacion = proveedor.FechaCreacion,
                UsuarioIdCreador = proveedor.UsuarioIdCreador,
            };
        }

        public async Task CreateProveedorAsync(ProveedorDto proveedorDto)
        {
            var proveedor = new Proveedor
            {
                Id = Guid.NewGuid(),
                RUC = proveedorDto.RUC,
                RazonSocial = proveedorDto.RazonSocial,
                Direccion = proveedorDto.Direccion,
                Telefono = proveedorDto.Telefono,
                Email = proveedorDto.Email,
                EstaActivo = true,
                FechaCreacion = DateTime.UtcNow,
                UsuarioIdCreador = proveedorDto.UsuarioIdCreador // Assuming this comes from authenticated user
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProveedorAsync(ProveedorDto proveedorDto)
        {
            var proveedor = await _context.Proveedores.FindAsync(proveedorDto.Id);
            if (proveedor == null) return;

            proveedor.RUC = proveedorDto.RUC;
            proveedor.RazonSocial = proveedorDto.RazonSocial;
            proveedor.Direccion = proveedorDto.Direccion;
            proveedor.Telefono = proveedorDto.Telefono;
            proveedor.Email = proveedorDto.Email;
            proveedor.EstaActivo = proveedorDto.EstaActivo;
            // FechaCreacion and UsuarioIdCreador should not be updated here
            // FechaModificacion could be added here if implemented in entity

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProveedorAsync(Guid id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return;

            proveedor.EstaActivo = !proveedor.EstaActivo; // Logical delete/activate
            await _context.SaveChangesAsync();
        }
    }
}
