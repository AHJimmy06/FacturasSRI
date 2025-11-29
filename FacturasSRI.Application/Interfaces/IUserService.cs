using FacturasSRI.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
        public interface IUserService
        {
            Task<PaginatedList<UserDto>> GetUsersAsync(int pageNumber, int pageSize, string? searchTerm, Guid? rolId, bool? isActive);
            Task<UserDto?> GetUserByIdAsync(Guid id);
            Task<UserDto> CreateUserAsync(UserDto user);                Task UpdateUserAsync(UserDto user);
                Task DeleteUserAsync(Guid id);
                Task<List<UserDto>> GetActiveUsersAsync();
                Task<UserDto?> GetUserProfileAsync(string userId);
                Task UpdateUserProfileAsync(string userId, UpdateProfileDto profileDto);
                        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto passwordDto);
                                Task<bool> GeneratePasswordResetTokenAsync(string email);
                                Task<bool> ResetPasswordAsync(string token, string newPassword);
                                Task<UserDto?> AuthenticateAsync(string email, string password);
                            }
                        }                