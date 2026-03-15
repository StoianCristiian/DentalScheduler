using DentalScheduler.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DentalScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Patient");

        return Ok(new { message = "Cont creat cu succes." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "Patient",
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.Address = request.Address;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = "Profil actualizat cu succes." });
    }

    [Authorize]
    [HttpPost("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("Niciun fișier încărcat.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Delete old picture if exists and not default
        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            var oldFileName = Path.GetFileName(user.ProfilePictureUrl);
            var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
            if (System.IO.File.Exists(oldFilePath))
            {
                try { System.IO.File.Delete(oldFilePath); } catch { }
            }
        }

        var fileUrl = $"/uploads/profiles/{uniqueFileName}";
        user.ProfilePictureUrl = fileUrl;
        
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest("Eroare la salvarea în baza de date.");

        return Ok(new { url = fileUrl });
    }
}
