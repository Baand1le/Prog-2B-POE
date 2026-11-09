using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Controllers;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
using System.Security.Claims;

namespace appTesting
{
    public class EventsControllerTests
    {
        private static RaceDayDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private static void SetLoggedInUser(
            ControllerBase controller, string userID, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userID),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims, "TestAuthentication");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
        }

        private static EventDTO CreateEventDTO()
        {
            return new EventDTO
            {
                Name = "Johannesburg Marathon",
                Description = "Annual road race",
                Date = DateTime.Now.AddDays(30),
                Location = "Johannesburg",
                Distance = "42 km",
                eventType = "Marathon"
            };
        }

        [Fact]
        public async Task CreateEvent_WithOrganiser_SavesEvent()
        {
            // ARRANGE
            using var context = CreateContext();

            var controller = new EventsController(context);
            SetLoggedInUser(
                controller, "organiser-001", "Organiser");

            var dto = CreateEventDTO();

            // ACT
            var result = await controller.CreateEvent(dto);

            // ASSERT
            Assert.IsType<CreatedAtActionResult>(result);

            var savedEvent = await context.Event.SingleAsync();

            Assert.Equal("organiser-001", savedEvent.organiserID);
            Assert.Equal(dto.Name, savedEvent.Name);
            Assert.Equal(dto.Location, savedEvent.Location);
        }

        [Fact]
        public async Task UpdateEvent_ByDifferentOrganiser_ReturnsForbid()
        {
            // ARRANGE
            using var context = CreateContext();

            context.Event.Add(new Events
            {
                eventID = "event-001",
                organiserID = "organiser-001",
                Name = "Original Event",
                Description = "Original description",
                Date = DateTime.Now.AddDays(30),
                Location = "Johannesburg",
                Distance = "10 km",
                eventType = "Road Race"
            });

            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            // Another Organiser attempts the update.
            SetLoggedInUser(
                controller, "organiser-002", "Organiser");

            var dto = CreateEventDTO();

            // ACT
            var result = await controller.UpdateEvent(
                "event-001", dto);

            // ASSERT
            Assert.IsType<ForbidResult>(result);

            var savedEvent = await context.Event
                .SingleAsync(e => e.eventID == "event-001");

            Assert.Equal("Original Event", savedEvent.Name);
        }

        [Fact]
        public async Task DeleteEvent_WithExistingCategory_ReturnsConflict()
        {
            // ARRANGE
            using var context = CreateContext();

            context.Event.Add(new Events
            {
                eventID = "event-001",
                organiserID = "organiser-001",
                Name = "Test Race",
                Description = "Race for testing",
                Date = DateTime.Now.AddDays(30),
                Location = "Johannesburg",
                Distance = "5 km",
                eventType = "Road Race"
            });

            context.Category.Add(new Categories
            {
                categoryID = "category-001",
                eventID = "event-001",
                categoryName = "Open",
                ageGroup = "18+"
            });

            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetLoggedInUser(
                controller, "organiser-001", "Organiser");

            // ACT
            var result = await controller.DeleteEvent("event-001");

            // ASSERT
            Assert.IsType<ConflictObjectResult>(result);

            Assert.True(await context.Event
                .AnyAsync(e => e.eventID == "event-001"));
        }
    }
}
