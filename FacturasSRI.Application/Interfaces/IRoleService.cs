using FacturasSRI.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(Guid id);
        Task<RoleDto> CreateRoleAsync(RoleDto role);
        Task UpdateRoleAsync(RoleDto role);
        Task DeleteRoleAsync(Guid id);
    }
}
