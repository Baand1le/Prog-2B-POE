using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Controllers;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
using System.Security.Claims;
using System.Collections;

namespace appTesting
{
    public class EnrollmentsControllerTests
    {
        private static RaceDayDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private static void SetParticipant(
            EnrollmentsController controller)
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier, "participant-001"),
                new Claim(
                    ClaimTypes.Role, "Participant")
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            claims, "TestAuthentication"))
                }
            };
        }

        private static async Task SeedEvent(
            RaceDayDbContext context,
            DateTime eventDate)
        {
            context.UserRole.AddRange(
                new UserRoles
                {
                    roleID = 1,
                    roleType = "Organiser"
                },
                new UserRoles
                {
                    roleID = 2,
                    roleType = "Participant"
                });

            context.User.AddRange(
                new Users
                {
                    userID = "organiser-001",
                    roleID = 1,
                    firstName = "Jane",
                    Surname = "Smith",
                    Email = "organiser@example.com",
                    PasswordHash = "test-hash"
                },
                new Users
                {
                    userID = "participant-001",
                    roleID = 2,
                    firstName = "John",
                    Surname = "Doe",
                    Email = "participant@example.com",
                    PasswordHash = "test-hash"
                });

            context.Event.Add(new Events
            {
                eventID = "event-001",
                organiserID = "organiser-001",
                Name = "Test Marathon",
                Description = "Annual marathon",
                Date = eventDate,
                Location = "Johannesburg",
                Distance = "42 km",
                eventType = "Marathon"
            });

            context.Category.Add(new Categories
            {
                categoryID = "category-001",
                eventID = "event-001",
                categoryName = "Open",
                ageGroup = "18+"
            });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateEnrollment_WithValidDetails_SavesEnrollment()
        {
            // ARRANGE
            using var context = CreateContext();

            await SeedEvent(context, DateTime.Now.AddDays(30));

            var controller = new EnrollmentsController(context);
            SetParticipant(controller);

            var dto = new CreateEnrollmentDTO
            {
                categoryID = "category-001"
            };

            // ACT
            var result = await controller.CreateEnrollment(dto);

            // ASSERT
            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, response.StatusCode);

            var savedEnrollment =
                await context.Enrollment.SingleAsync();

            Assert.Equal(
                "participant-001",
                savedEnrollment.participantID);

            Assert.Equal(
                "event-001",
                savedEnrollment.eventID);

            Assert.Equal(
                "category-001",
                savedEnrollment.categoryID);

            // Verify that the Participant can retrieve it.
            var getResult = await controller.GetMyEnrollments();

            var okResult = Assert.IsType<OkObjectResult>(getResult);

            var entries = Assert.IsAssignableFrom<IEnumerable>(
                okResult.Value);

            Assert.Single(entries.Cast<object>());
        }

        [Fact]
        public async Task CreateEnrollment_WhenAlreadyEnrolled_ReturnsConflict()
        {
            // ARRANGE
            using var context = CreateContext();

            await SeedEvent(context, DateTime.Now.AddDays(30));

            context.Enrollment.Add(new Enrollments
            {
                enrollmentID = "enrollment-001",
                participantID = "participant-001",
                eventID = "event-001",
                categoryID = "category-001",
                enrollmentDate = DateTime.Now
            });

            await context.SaveChangesAsync();

            var controller = new EnrollmentsController(context);
            SetParticipant(controller);

            var dto = new CreateEnrollmentDTO
            {
                categoryID = "category-001"
            };

            // ACT
            var result = await controller.CreateEnrollment(dto);

            // ASSERT
            Assert.IsType<ConflictObjectResult>(result);

            Assert.Equal(
                1,
                await context.Enrollment.CountAsync());
        }

        [Fact]
        public async Task CreateEnrollment_ForPastEvent_ReturnsBadRequest()
        {
            // ARRANGE
            using var context = CreateContext();

            await SeedEvent(context, DateTime.Now.AddDays(-7));

            var controller = new EnrollmentsController(context);
            SetParticipant(controller);

            var dto = new CreateEnrollmentDTO
            {
                categoryID = "category-001"
            };

            // ACT
            var result = await controller.CreateEnrollment(dto);

            // ASSERT
            Assert.IsType<BadRequestObjectResult>(result);

            Assert.Empty(await context.Enrollment.ToListAsync());
        }
    }
}
