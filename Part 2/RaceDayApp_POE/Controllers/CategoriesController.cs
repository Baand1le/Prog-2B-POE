using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
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
        public async Task<IActionResult> GetCategories(string eventID)
        {
            bool eventExists = await _context.Event
                .AnyAsync(e => e.eventID == eventID);

            if (!eventExists)
                return NotFound("Event not found.");

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
        public async Task<IActionResult> CreateCategory(
            string eventID,
            [FromBody] CreateCategoryDTO dto)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

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
        public async Task<IActionResult> UpdateCategory(
            string eventID,
            string categoryID,
            [FromBody] CreateCategoryDTO dto)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            var category = await _context.Category
                .FirstOrDefaultAsync(c =>
                    c.categoryID == categoryID &&
                    c.eventID == eventID);

            if (category == null)
                return NotFound("Category not found.");

            category.categoryName = dto.categoryName;
            category.ageGroup = dto.ageGroup;

            await _context.SaveChangesAsync();

            return Ok("Category updated successfully.");
        }//end of async: UpdateCategory

        // DELETE: api/events/{eventID}/categories/{categoryID}
        [HttpDelete("{categoryID}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(
            string eventID,
            string categoryID)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            var category = await _context.Category
                .FirstOrDefaultAsync(c =>
                    c.categoryID == categoryID &&
                    c.eventID == eventID);

            if (category == null)
                return NotFound("Category not found.");

            bool hasEnrollments = await _context.Enrollment
                .AnyAsync(e => e.categoryID == categoryID);

            if (hasEnrollments)
                return Conflict(
                    "Cannot delete a category with enrollments.");

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();

        }//end of async: DeleteCategory

    }//end of class: CategoryController

}//end of namespace