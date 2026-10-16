using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace api_proj.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/users/me
        [HttpGet("me")]
        [SwaggerOperation(
        Summary = "View your profile",
        Description = "Retrieve your personal account information, including your name, email address and " +
            "assigned user role."
        )]
        public async Task<IActionResult> GetMyProfile()
        {
            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var user = await _context.User
                .AsNoTracking()
                .Where(u => u.userID == userID)
                .Select(u => new
                {
                    u.userID,
                    u.firstName,
                    u.Surname,
                    u.Email,
                    role = u.userRole.roleType
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        // PUT: api/users/me
        [HttpPut("me")]
        [SwaggerOperation(
        Summary = "Update your profile",
        Description = "Update your first name, surname or email address. Changes are applied only to your " +
            "own account."
        )]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateUserDTO dto)
        {
            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var user = await _context.User
                .FindAsync(userID);

            if (user == null)
                return NotFound("User not found.");

            bool emailExists = await _context.User
                .AnyAsync(u =>
                    u.Email == dto.email &&
                    u.userID != userID);

            if (emailExists)
                return Conflict("Email is already registered.");

            user.firstName = dto.firstName;
            user.Surname = dto.surname;
            user.Email = dto.email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.userID,
                user.firstName,
                user.Surname,
                user.Email
            });
        }
    }
}