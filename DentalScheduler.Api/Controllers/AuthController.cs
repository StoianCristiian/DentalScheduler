using System.Security.Claims;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Added this one back explicitely for IFormFile

namespace DentalScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIdentityService _identityService;
        private readonly IAwsS3Service _awsS3Service;

        public AuthController(UserManager<ApplicationUser> userManager, IIdentityService identityService, IAwsS3Service awsS3Service)
        {
            _userManager = userManager;
            _identityService = identityService;
            _awsS3Service = awsS3Service;
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

            var profilePicUrl = user.ProfilePictureUrl;
            if (!string.IsNullOrEmpty(profilePicUrl) && !profilePicUrl.StartsWith("http"))
            {
                profilePicUrl = await _awsS3Service.GetPresignedUrlAsync(profilePicUrl);
            }

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Patient",
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ProfilePictureUrl = profilePicUrl
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

            // Șterge poza veche de pe S3 dacă există
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && !user.ProfilePictureUrl.StartsWith("http"))
            {
                await _awsS3Service.DeleteFileAsync(user.ProfilePictureUrl);
            }

            // Uploadează noua poză
            using var fileStream = file.OpenReadStream();
            var fileKey = await _awsS3Service.UploadFileAsync(fileStream, file.FileName, file.ContentType, user.Id);
            
            user.ProfilePictureUrl = fileKey;
            
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            // Returnează un Presigned URL valid pentru următoarea oră
            var presignedUrl = await _awsS3Service.GetPresignedUrlAsync(fileKey);

            return Ok(new { 
                fileUrl = presignedUrl, 
                message = "Poză de profil actualizată cu succes." 
            });
        }
    }
}
