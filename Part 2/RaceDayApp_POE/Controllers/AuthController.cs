using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
using System.Security.Claims;

namespace api_proj.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDTO dto)
        {
            // Check whether the email is already registered
            bool emailExists = await _context.User
                .AnyAsync(u => u.Email == dto.email);

            if (emailExists)
                return Conflict("Email is already registered.");

            // Verify that the selected role exists
            var role = await _context.UserRole
                .FirstOrDefaultAsync(r => r.roleID == dto.roleID);

            if (role == null ||
                (role.roleType != "Organiser" &&
                 role.roleType != "Participant"))
            {
                return BadRequest("Invalid user role.");
            }

            // Hash the password using BCrypt
            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(dto.password);

            // Create a new user
            var user = new Users
            {
                userID = Guid.NewGuid().ToString("N")[..20],
                firstName = dto.firstName,
                Surname = dto.surname,
                Email = dto.email,
                roleID = dto.roleID,
                PasswordHash = hashedPassword
            };

            _context.User.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                userID = user.userID,
                role = role.roleType
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDTO dto)
        {
            // Find the user and their role
            var user = await _context.User
                .Include(u => u.userRole)
                .FirstOrDefaultAsync(u => u.Email == dto.email);

            // Verify the password
            if (user == null ||
                string.IsNullOrEmpty(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.password, user.PasswordHash))
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            // Create claims containing user identity and role
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier, user.userID),

                new Claim(
                    ClaimTypes.Email, user.Email),

                new Claim(
                    ClaimTypes.Role, user.userRole.roleType)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            // Create the authentication cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return Ok(new
            {
                message = "Login successful.",
                userID = user.userID,
                role = user.userRole.roleType
            });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                userID = User.FindFirstValue(
                    ClaimTypes.NameIdentifier),

                email = User.FindFirstValue(
                    ClaimTypes.Email),

                role = User.FindFirstValue(
                    ClaimTypes.Role)
            });
        }

    }//end of class

}//end of namespace