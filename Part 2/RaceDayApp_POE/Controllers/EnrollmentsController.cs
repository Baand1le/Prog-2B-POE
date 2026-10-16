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
    [Route("api")]
    [ApiController]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrollmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrollments
        [HttpPost("enrollments")]
        [Authorize(Roles = "Participant")]
        [SwaggerOperation(
            Summary = "Enroll in a race event",
            Description = "Allow an authenticated Participant to enroll in an upcoming event by selecting a " +
            "category. The Participant's identity is obtained automatically from their login session."
        )]
        public async Task<IActionResult> CreateEnrollment(
            [FromBody] CreateEnrollmentDTO dto)
        {
            var participantID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var category = await _context.Category
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.categoryID == dto.categoryID);

            if (category == null)
                return NotFound("Category not found.");

            // Check that the event is still upcoming
            if (category.Event.Date <= DateTime.Now)
                return BadRequest(
                    "Cannot enroll in an event that has already started.");

            // Prevent duplicate enrollment in the same event
            bool alreadyEnrolled = await _context.Enrollment
                .AnyAsync(e =>
                    e.participantID == participantID &&
                    e.eventID == category.eventID);

            if (alreadyEnrolled)
                return Conflict(
                    "You are already enrolled in this event.");

            var enrollment = new Enrollments
            {
                enrollmentID = Guid.NewGuid()
                    .ToString("N")[..20],
                participantID = participantID!,
                eventID = category.eventID,
                categoryID = category.categoryID,
                enrollmentDate = DateTime.Now
            };

            _context.Enrollment.Add(enrollment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrollment successful.",
                enrollment.enrollmentID,
                enrollment.participantID,
                enrollment.eventID,
                enrollment.categoryID,
                enrollment.enrollmentDate
            });
        }

        // GET: api/enrollments/mine
        [HttpGet("enrollments/mine")]
        [Authorize(Roles = "Participant")]
        [SwaggerOperation(
            Summary = "View your race enrollments",
            Description = "Retrieve all race events and categories in which the logged-in Participant is enrolled."
        )]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var participantID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var enrollments = await _context.Enrollment
                .AsNoTracking()
                .Where(e => e.participantID == participantID)
                .Select(e => new
                {
                    e.enrollmentID,
                    e.eventID,
                    eventName = e.Event.Name,
                    eventDate = e.Event.Date,
                    eventLocation = e.Event.Location,
                    e.categoryID,
                    categoryName = e.Category.categoryName,
                    e.enrollmentDate
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // GET: api/events/{eventID}/enrollments
        [HttpGet("events/{eventID}/enrollments")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "View Participants enrolled in an event",
            Description = "Allow the event's Organiser to retrieve a list of Participants registered for" +
            " that event."
        )]
        public async Task<IActionResult> GetEventEnrollments(
            string eventID)
        {
            var raceEvent = await _context.Event
                .FindAsync(eventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            var enrollments = await _context.Enrollment
                .AsNoTracking()
                .Where(e => e.eventID == eventID)
                .Select(e => new
                {
                    e.enrollmentID,
                    e.participantID,
                    firstName = e.Participant.firstName,
                    surname = e.Participant.Surname,
                    e.categoryID,
                    categoryName = e.Category.categoryName,
                    e.enrollmentDate
                })
                .ToListAsync();

            return Ok(enrollments);
        }
    }
}