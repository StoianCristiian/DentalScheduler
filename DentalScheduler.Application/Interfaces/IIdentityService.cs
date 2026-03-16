using DentalScheduler.Application.Admin.DTOs;

namespace DentalScheduler.Application.Interfaces;

public interface IIdentityService
{
    Task<int> GetCountInRoleAsync(string roleName);
    Task<List<UserWithRoleDto>> GetAllUsersWithRolesAsync();
    Task<bool> UpdateUserRoleAsync(string userId, string newRole);
    Task<bool> RoleExistsAsync(string roleName);
}

