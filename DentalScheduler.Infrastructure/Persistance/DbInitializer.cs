using DentalScheduler.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DentalScheduler.Infrastructure.Persistance;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Seed Roles
        string[] roleNames = { "Admin", "Doctor", "Patient" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // 2. Seed Admin User
        var adminEmail = "admin@dental.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "System",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 3. Seed Doctor Users
        var doctors = new[]
        {
            new { Email = "popescu@dental.com", FirstName = "Andrei", LastName = "Popescu" },
            new { Email = "ionescu@dental.com", FirstName = "Maria",  LastName = "Ionescu" },
        };

        foreach (var d in doctors)
        {
            var existing = await userManager.FindByEmailAsync(d.Email);
            if (existing == null)
            {
                var doctor = new ApplicationUser
                {
                    UserName = d.Email,
                    Email = d.Email,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(doctor, "Doctor123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(doctor, "Doctor");
            }
        }
    }
}
