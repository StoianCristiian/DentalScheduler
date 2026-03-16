using DentalScheduler.Application.Admin.DTOs;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Infrastructure.Identity;
using DentalScheduler.Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<int> GetCountInRoleAsync(string roleName)
    {
        // Reutilizăm logica din GetAllUsersWithRolesAsync pentru consistență
        // Utilizatorii fără rol sunt considerați Pacienți, iar ierarhia rolurilor este respectată
        var allUsers = await GetAllUsersWithRolesAsync();
        return allUsers.Count(u => u.Role == roleName);
    }

    public async Task<List<UserWithRoleDto>> GetAllUsersWithRolesAsync()
    {
        var query = from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId into userRoles
                    from ur in userRoles.DefaultIfEmpty()
                    join r in _context.Roles on ur.RoleId equals r.Id into roles
                    from r in roles.DefaultIfEmpty()
                    select new UserWithRoleDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = r.Name ?? "Patient"
                    };

        var users = await query.ToListAsync();

        return users
            .GroupBy(u => u.Id)
            .Select(g => 
            {
                var user = g.First();
                var allRoles = g.Select(x => x.Role).Where(r => r != "None").ToList();
                var displayRole = allRoles.Count > 0 ? allRoles.First() : "Patient";
                
                if (allRoles.Contains("Admin")) displayRole = "Admin";
                else if (allRoles.Contains("Doctor")) displayRole = "Doctor";
                
                user.Role = displayRole;
                return user;
            })
            .ToList();
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return false;

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        return addResult.Succeeded;
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }
}
