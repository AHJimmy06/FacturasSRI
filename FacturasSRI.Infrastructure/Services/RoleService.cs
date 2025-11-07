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
    public class RoleService : IRoleService
    {
        private readonly FacturasSRIDbContext _context;

        public RoleService(FacturasSRIDbContext context)
        {
            _context = context;
        }

        public async Task<RoleDto> CreateRoleAsync(RoleDto roleDto)
        {
            var role = new Rol
            {
                Id = Guid.NewGuid(),
                Nombre = roleDto.Nombre,
                Descripcion = roleDto.Descripcion,
                EstaActivo = roleDto.EstaActivo
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            roleDto.Id = role.Id;
            return roleDto;
        }

        public async Task DeleteRoleAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                role.EstaActivo = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null || !role.EstaActivo)
            {
                return null;
            }
            return new RoleDto
            {
                Id = role.Id,
                Nombre = role.Nombre,
                Descripcion = role.Descripcion,
                EstaActivo = role.EstaActivo
            };
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return await _context.Roles.Where(r => r.EstaActivo).Select(role => new RoleDto
            {
                Id = role.Id,
                Nombre = role.Nombre,
                Descripcion = role.Descripcion,
                EstaActivo = role.EstaActivo
            }).ToListAsync();
        }

        public async Task UpdateRoleAsync(RoleDto roleDto)
        {
            var role = await _context.Roles.FindAsync(roleDto.Id);
            if (role != null)
            {
                role.Nombre = roleDto.Nombre;
                role.Descripcion = roleDto.Descripcion;
                role.EstaActivo = roleDto.EstaActivo;
                await _context.SaveChangesAsync();
            }
        }
    }
}
