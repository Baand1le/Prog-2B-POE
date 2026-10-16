using Xunit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
using RaceDayApp_POE.Controllers;
using System.Security.Claims;

namespace appTesting
{
    public class AuthControllerTests
    {
        // Creates an independent temporary database per test.
        private static RaceDayDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // Seeds the Participant role.
        private static async Task SeedRole(RaceDayDbContext context)
        {
            context.UserRole.Add(new UserRoles
            {
                roleID = 1,
                roleType = "Participant"
            });

            await context.SaveChangesAsync();
        }

        // TEST 1: Valid registration
        [Fact]
        public async Task Register_WithValidDetails_ReturnsOk()
        {
            // Arrange
            using var context = CreateContext();
            await SeedRole(context);

            var controller = new AuthController(context);

            var dto = new RegisterDTO
            {
                firstName = "John",
                surname = "Doe",
                email = "john@example.com",
                password = "Password123!",
                roleType = "Participant"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var user = await context.User.SingleOrDefaultAsync(
                u => u.Email == dto.email);

            Assert.NotNull(user);
            Assert.Equal(1, user.roleID);
            Assert.NotEqual(dto.password, user.PasswordHash);

            Assert.True(BCrypt.Net.BCrypt.Verify(
                dto.password, user.PasswordHash));
        }

        // TEST 2: Duplicate email
        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            // Arrange
            using var context = CreateContext();
            await SeedRole(context);

            context.User.Add(new Users
            {
                userID = "existing-user",
                firstName = "John",
                Surname = "Doe",
                Email = "john@example.com",
                roleID = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    "Password123!")
            });

            await context.SaveChangesAsync();

            var controller = new AuthController(context);

            var dto = new RegisterDTO
            {
                firstName = "Jane",
                surname = "Doe",
                email = "john@example.com",
                password = "AnotherPassword123!",
                roleType = "Participant"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(1, await context.User.CountAsync());
        }

        // TEST 3: Incorrect login password
        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            using var context = CreateContext();
            await SeedRole(context);

            context.User.Add(new Users
            {
                userID = "participant-001",
                firstName = "John",
                Surname = "Doe",
                Email = "john@example.com",
                roleID = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    "CorrectPassword123!")
            });

            await context.SaveChangesAsync();

            var controller = new AuthController(context);

            var dto = new LoginDTO
            {
                email = "john@example.com",
                password = "WrongPassword123!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 4: Successful login
        [Fact]
        public async Task Login_WithValidCredentials_SignsInSuccessfully()
        {
            // Arrange
            using var context = CreateContext();
            await SeedRole(context);

            context.User.Add(new Users
            {
                userID = "participant-001",
                firstName = "John",
                Surname = "Doe",
                Email = "john@example.com",
                roleID = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    "CorrectPassword123!")
            });

            await context.SaveChangesAsync();

            var fakeAuth = new FakeAuthenticationService();

            var services = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(fakeAuth)
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };

            var controller = new AuthController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            var dto = new LoginDTO
            {
                email = "john@example.com",
                password = "CorrectPassword123!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(fakeAuth.SignedInPrincipal);

            Assert.Equal(
                "participant-001",
                fakeAuth.SignedInPrincipal.FindFirstValue(
                    ClaimTypes.NameIdentifier));

            Assert.Equal(
                "Participant",
                fakeAuth.SignedInPrincipal.FindFirstValue(
                    ClaimTypes.Role));

            Assert.Equal(
                CookieAuthenticationDefaults.AuthenticationScheme,
                fakeAuth.SignedInScheme);
        }

        // Simulates the authentication service so a successful
        // login can be unit-tested without running a web server.
        private sealed class FakeAuthenticationService
            : IAuthenticationService
        {
            public ClaimsPrincipal? SignedInPrincipal { get; private set; }
            public string? SignedInScheme { get; private set; }

            public Task<AuthenticateResult> AuthenticateAsync(
                HttpContext context, string? scheme)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            public Task ChallengeAsync(
                HttpContext context, string? scheme,
                AuthenticationProperties? properties)
            {
                return Task.CompletedTask;
            }

            public Task ForbidAsync(
                HttpContext context, string? scheme,
                AuthenticationProperties? properties)
            {
                return Task.CompletedTask;
            }

            public Task SignInAsync(
                HttpContext context, string? scheme,
                ClaimsPrincipal principal,
                AuthenticationProperties? properties)
            {
                SignedInPrincipal = principal;
                SignedInScheme = scheme;

                return Task.CompletedTask;
            }

            public Task SignOutAsync(
                HttpContext context, string? scheme,
                AuthenticationProperties? properties)
            {
                return Task.CompletedTask;
            }
        }
    }
}
