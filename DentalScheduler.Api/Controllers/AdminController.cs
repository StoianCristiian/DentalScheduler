using DentalScheduler.Domain.Entities;
using DentalScheduler.Infrastructure.Identity;
using DentalScheduler.Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStats>> GetStats()
    {
        // Count users in roles
        // We can do this efficiently by grouping UserRoles or just counting
        // Since we need specific roles, let's query the specific RoleIds first
        
        var patientRoleId = await _context.Roles
            .Where(r => r.Name == "Patient")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var doctorRoleId = await _context.Roles
            .Where(r => r.Name == "Doctor")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var totalPatients = patientRoleId != null 
            ? await _context.UserRoles.CountAsync(ur => ur.RoleId == patientRoleId)
            : 0;

        var totalDoctors = doctorRoleId != null
            ? await _context.UserRoles.CountAsync(ur => ur.RoleId == doctorRoleId)
            : 0;

        var totalAppointments = await _context.Appointments.CountAsync();
        
        // Revenue (sum of cost of completed appointments)
        var totalRevenue = await _context.Appointments
            .Where(a => a.Status == DentalScheduler.Domain.Enums.AppointmentStatus.Completed && a.Cost != null)
            .SumAsync(a => a.Cost) ?? 0;

        return Ok(new AdminDashboardStats
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            TotalRevenue = totalRevenue
        });
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserWithRoleDto>>> GetUsers()
    {
        // LINQ Query to get User + Role Name
        // Warning: This assumes 1 role per user mostly. If multiple, it takes one.
        
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
                        Role = r.Name ?? "Patient" // Implicit userii fara rol sunt Pacienti
                    };

        var users = await query.ToListAsync();
        
        // Remove duplicates if a user has multiple roles (though UI might want to see all)
        // For this simple dashboard, we'll group by ID and take the "most important" role or comma join
        // But for simplicity of editing, let's assume we want to work with the primary role.
        // The join above produces multiple rows for multiple roles.
        // Let's do it in memory for cleaner grouping if the list isn't huge.
        
        var groupedUsers = users
            .GroupBy(u => u.Id)
            .Select(g => 
            {
                var user = g.First();
                // Prefer non-Patient role if exists (e.g. Admin or Doctor) for display
                var allRoles = g.Select(x => x.Role).Where(r => r != "None").ToList();
                var displayRole = allRoles.Count > 0 ? allRoles.First() : "Patient";
                
                // If user is Admin, ensure we show Admin. If Doctor, show Doctor.
                if (allRoles.Contains("Admin")) displayRole = "Admin";
                else if (allRoles.Contains("Doctor")) displayRole = "Doctor";
                
                user.Role = displayRole;
                return user;
            })
            .ToList();

        return Ok(groupedUsers);
    }
    
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Utilizatorul nu a fost găsit.");
            
        // Prevent changing own role if you are the only admin? 
        // For now, let's just allow it but be careful.
        
        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Remove from all current roles
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return BadRequest("Eroare la eliminarea rolurilor vechi.");
            
        // Add to new role
        if (!await _roleManager.RoleExistsAsync(request.NewRole))
            return BadRequest("Rolul specificat nu există.");
            
        var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
        if (!addResult.Succeeded)
            return BadRequest("Eroare la adăugarea noului rol.");
            
        return Ok(new { message = "Rol actualizat cu succes." });
    }
}

public class AdminDashboardStats
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class UserWithRoleDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}

public class UpdateRoleRequest
{
    public string NewRole { get; set; } // "Admin", "Doctor", "Patient"
}
