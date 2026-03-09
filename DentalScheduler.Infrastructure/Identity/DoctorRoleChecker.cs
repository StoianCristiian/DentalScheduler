using DentalScheduler.Application.Appointments.Queries.GetDoctors;
using Microsoft.AspNetCore.Identity;

namespace DentalScheduler.Infrastructure.Identity;

public class DoctorRoleChecker : IDoctorRoleChecker
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorRoleChecker(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<DoctorDto>> GetUsersInRoleAsync(string role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);

        return users
            .Select(u => new DoctorDto(
                u.Id,
                $"{u.FirstName} {u.LastName}".Trim(),
                u.Email ?? string.Empty))
            .ToList();
    }
}
