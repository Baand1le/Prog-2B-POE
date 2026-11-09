using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace RaceDayApp_POE.Controllers
{
    [Route("api/events/{eventID}/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventID}/categories
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "View event categories",
            Description = "Retrieve all categories available for a specific race event, including category" +
            " names and age groups."
        )]
        [SwaggerResponse(200, "Event categories retrieved successfully.")]
        [SwaggerResponse(404, "Event not found.")]
        public async Task<IActionResult> GetCategories(string eventID)
        {
            bool eventExists = await _context.Event
                .AnyAsync(e => e.eventID == eventID);

            if (!eventExists)
                return NotFound(new
                {
                    status = 404,
                    message = "The requested event could not be found."
                });

            var categories = await _context.Category
                .AsNoTracking()
                .Where(c => c.eventID == eventID)
                .Select(c => new
                {
                    c.categoryID,
                    c.eventID,
                    c.categoryName,
                    c.ageGroup
                })
                .ToListAsync();

            return Ok(categories);
        }//end of async: GetCategories

        // POST: api/events/{eventID}/categories
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "Create an event category",
            Description = "Allow the Organiser who owns an event to create a new category with a category " +
            "name and age group."
        )]
        [SwaggerResponse(201, "Category created successfully.")]
        [SwaggerResponse(400, "Invalid category information.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "You do not have permission to manage this event.")]
        [SwaggerResponse(404, "Event not found.")]
        public async Task<IActionResult> CreateCategory(
            string eventID,
            [FromBody] CreateCategoryDTO dto)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The requested event could not be found."
                });

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            var category = new Categories
            {
                categoryID = Guid.NewGuid()
                    .ToString("N")[..20],
                eventID = eventID,
                categoryName = dto.categoryName,
                ageGroup = dto.ageGroup
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created successfully.",
                category.categoryID,
                category.eventID,
                category.categoryName,
                category.ageGroup
            });
        }//end of async: CreateCategory

        // PUT: this is a description
        [HttpPut("{categoryID}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "Update an event category",
            Description = "Allow the event's Organiser to update an existing category's name or age group."
        )]
        [SwaggerResponse(200, "Category updated successfully.")]
        [SwaggerResponse(400, "Invalid category information.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "You do not have permission to update this category.")]
        [SwaggerResponse(404, "Event or category not found.")]
        public async Task<IActionResult> UpdateCategory(
            string eventID,
            string categoryID,
            [FromBody] CreateCategoryDTO dto)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The requested event could not be found."
                });

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            var category = await _context.Category
                .FirstOrDefaultAsync(c =>
                    c.categoryID == categoryID &&
                    c.eventID == eventID);

            if (category == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The requested category could not be found."
                });

            category.categoryName = dto.categoryName;
            category.ageGroup = dto.ageGroup;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                category.categoryID,
                category.eventID,
                category.categoryName,
                category.ageGroup
            });
        }//end of async: UpdateCategory

        // DELETE: api/events/{eventID}/categories/{categoryID}
        [HttpDelete("{categoryID}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "Delete an event category",
            Description = "Allow the event's Organiser to delete a category, provided no Participants are " +
            "enrolled in it."
        )]
        [SwaggerResponse(200, "Category deleted successfully.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "You do not have permission to delete this category.")]
        [SwaggerResponse(404, "Event or category not found.")]
        [SwaggerResponse(409, "Cannot delete a category containing enrollments.")]
        public async Task<IActionResult> DeleteCategory(
            string eventID,
            string categoryID)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The requested event could not be found."
                });

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            var category = await _context.Category
                .FirstOrDefaultAsync(c =>
                    c.categoryID == categoryID &&
                    c.eventID == eventID);

            if (category == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The requested category could not be found."
                });

            bool hasEnrollments = await _context.Enrollment
                .AnyAsync(e => e.categoryID == categoryID);

            if (hasEnrollments)
                return Conflict(new
                {
                    status = 409,
                    message = "Cannot delete this category because Participants are already enrolled in it."
                });

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully.",
                categoryID = category.categoryID,
                eventID = category.eventID
            });

        }//end of async: DeleteCategory

    }//end of class: CategoryController

}//end of namespace