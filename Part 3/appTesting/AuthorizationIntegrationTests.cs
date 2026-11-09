using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;

namespace appTesting
{
    public class AuthorizationIntegrationTests
    {
        private static EventDTO CreateEventDTO()
        {
            return new EventDTO
            {
                Name = "Johannesburg Marathon",
                Description = "Annual marathon event",
                Date = DateTime.Now.AddDays(30),
                Location = "Johannesburg",
                Distance = "42 km",
                eventType = "Marathon"
            };
        }

        // TEST 1: Organiser can create an event.
        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            // ARRANGE
            using var factory = new RaceDayTestFactory();

            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            // Seed an Organiser in the temporary database.
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<RaceDayDbContext>();

                var organiserRole = await context.UserRole
                    .SingleAsync(r => r.roleType == "Organiser");

                context.User.Add(new Users
                {
                    userID = "organiser-001",
                    roleID = organiserRole.roleID,
                    firstName = "Test",
                    Surname = "Organiser",
                    Email = "organiser@example.com",
                    PasswordHash = "test-only-hash"
                });

                await context.SaveChangesAsync();
            }

            var request = new HttpRequestMessage(
                HttpMethod.Post, "/api/events");

            // Simulate an authenticated Organiser.
            request.Headers.Add("X-Test-Role", "Organiser");
            request.Headers.Add(
                "X-Test-UserId", "organiser-001");

            request.Content = JsonContent.Create(CreateEventDTO());

            // ACT
            var response = await client.SendAsync(request);

            // ASSERT
            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            using var verifyScope =
                factory.Services.CreateScope();

            var verifyContext = verifyScope.ServiceProvider
                .GetRequiredService<RaceDayDbContext>();

            var savedEvent =
                await verifyContext.Event.SingleAsync();

            Assert.Equal(
                "organiser-001",
                savedEvent.organiserID);
        }

        // TEST 2: Participant cannot create an event.
        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            // ARRANGE
            using var factory = new RaceDayTestFactory();

            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post, "/api/events");

            // Simulate a logged-in Participant.
            request.Headers.Add(
                "X-Test-Role", "Participant");

            request.Headers.Add(
                "X-Test-UserId", "participant-001");

            request.Content = JsonContent.Create(CreateEventDTO());

            // ACT
            var response = await client.SendAsync(request);

            // ASSERT
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify no event was created.
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<RaceDayDbContext>();

            Assert.False(await context.Event.AnyAsync());
        }

        // TEST 3: Unauthenticated user cannot create an event.
        [Fact]
        public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
        {
            // ARRANGE
            using var factory = new RaceDayTestFactory();

            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            // No authentication headers are provided.

            // ACT
            var response = await client.PostAsJsonAsync(
                "/api/events", CreateEventDTO());

            // ASSERT
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }

    // Starts RaceDay using a temporary database and
    // a test-only authentication scheme.
    public class RaceDayTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName =
            Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // Replace the SQL Server DbContext options.
                services.RemoveAll<
                    DbContextOptions<RaceDayDbContext>>();

                // Register the InMemory database.
                services.AddDbContext<RaceDayDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Replace default authentication with
                // a test scheme.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        "TestScheme";

                    options.DefaultChallengeScheme =
                        "TestScheme";

                    options.DefaultForbidScheme =
                        "TestScheme";
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                        "TestScheme", options => { });
            });
        }
    }

    // This handler creates a test identity from headers.
    // It does not change production authentication.
    public class TestAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            // Missing role header means no logged-in user.
            if (!Request.Headers.TryGetValue(
                "X-Test-Role", out var role))
            {
                return Task.FromResult(
                    AuthenticateResult.NoResult());
            }

            var userID = Request.Headers.TryGetValue(
                "X-Test-UserId", out var id)
                ? id.ToString()
                : "test-user";

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier, userID),

                new Claim(
                    ClaimTypes.Role, role.ToString())
            };

            var identity = new ClaimsIdentity(
                claims, Scheme.Name);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(
                principal, Scheme.Name);

            return Task.FromResult(
                AuthenticateResult.Success(ticket));
        }
    }
}