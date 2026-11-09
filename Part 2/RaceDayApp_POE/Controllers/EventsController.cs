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
    [Route("api/events")]
    [ApiController]
    [Authorize]
    public class EventsController : Controller
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Browse all race events",
            Description = "Retrieve a list of all available race events,including thier dates, locations, distances " +
            "and event types."
            )]
        [SwaggerResponse(200, "Race events retrieved successfully.")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Event
                .AsNoTracking()
                .Select(e => new
                {
                    e.eventID,
                    e.organiserID,
                    e.Name,
                    e.Description,
                    e.Date,
                    e.Location,
                    e.Distance,
                    e.eventType
                })
                .ToListAsync();
            return Ok(events);

        }//end of async GetEvents

        //Get: api/events/{eventID}
        [HttpGet("{eventID}")]
        [AllowAnonymous]
        [SwaggerOperation(
        Summary = "View a specific race event",
        Description = "Retrieve the details of a race event using its unique event ID."
        )]
        [SwaggerResponse(200, "Event retrieved successfully.")]
        [SwaggerResponse(404, "No event was found with the provided ID.")]
        public async Task<IActionResult> GetEvent(string eventID)
        {
            var raceEvent = await _context.Event
                .AsNoTracking()
                .Where(e => e.eventID == eventID)
                .Select(e => new
                {
                    e.eventID,
                    e.organiserID,
                    e.Name,
                    e.Description,
                    e.Date,
                    e.Location,
                    e.Distance,
                    e.eventType,
                })
                .FirstOrDefaultAsync();

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = "The requested event could not be found."
                });
            }
            return Ok(raceEvent);

        }//end of async GetEvent

        //Post: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
        Summary = "Create a new race event",
        Description = "Allow an authenticated Organiser to create a race event. The event is automatically " +
            "linked to the logged-in Organiser."
        )]
        [SwaggerResponse(201, "Event created successfully.")]
        [SwaggerResponse(400, "Invalid event information supplied.")]
        [SwaggerResponse(401, "You must be logged in to create an event.")]
        [SwaggerResponse(403, "Access denied. Only Organisers can create events.")]
        public async Task<IActionResult> CreateEvent([FromBody] EventDTO dto)
        {
            var organizerID =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var raceEvent = new Events
            {
                eventID = Guid.NewGuid().ToString("N")[..20],
                organiserID = organizerID!,
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                Distance = dto.Distance,
                eventType = dto.eventType
            };

            _context.Event.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { eventID = raceEvent.eventID },
                new
                {
                    message = "Event created successfully",
                    eventID = raceEvent.eventID,
                    raceEvent.Name,
                    raceEvent.Description,
                    raceEvent.Date,
                    raceEvent.Location,
                    raceEvent.Distance,
                    raceEvent.eventType
                });
        }

        //Put: api/events/{eventID}
        [HttpPut("{eventID}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
        Summary = "Update an existing race event",
        Description = "Allow an Organiser to modify the details of a race event they own."
        )]
        [SwaggerResponse(200, "Event updated successfully.")]
        [SwaggerResponse(400, "The supplied event details are invalid.")]
        [SwaggerResponse(401, "Authentication is required.")]
        [SwaggerResponse(403, "Access denied. You cannot update this event.")]
        [SwaggerResponse(404, "The requested event could not be found.")]
        public async Task<IActionResult> UpdateEvent(
    string eventID, [FromBody] EventDTO dto)
        {
            var raceEvent = await _context.Event.FindAsync(eventID);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = "The event you are trying to update does not exist."
                });
            }

            var userID =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            raceEvent.Name = dto.Name;
            raceEvent.Description = dto.Description;
            raceEvent.Date = dto.Date;
            raceEvent.Location = dto.Location;
            raceEvent.Distance = dto.Distance;
            raceEvent.eventType = dto.eventType;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventID = raceEvent.eventID,
                raceEvent.Name,
                raceEvent.Description,
                raceEvent.Date,
                raceEvent.Location,
                raceEvent.Distance,
                raceEvent.eventType
            });
        }//end of UpdateEvent

        //Delete: api/events/{eventID}
        [HttpDelete("{eventID}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
        Summary = "Delete a race event",
        Description = "Allow an Organiser to delete an event they own, provided it has no associated categories" +
            " or enrollments."
        )]
        [SwaggerResponse(200, "Event deleted successfully.")]
        [SwaggerResponse(401, "Authentication is required.")]
        [SwaggerResponse(403, "Access denied. You cannot delete this event.")]
        [SwaggerResponse(404, "The requested event could not be found.")]
        [SwaggerResponse(409, "The event has categories or enrollments and cannot be deleted.")]
        public async Task<IActionResult> DeleteEvent(string eventID)
        {
            var raceEvent = await _context.Event.FindAsync(eventID);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = "The event you are trying to delete does not exist."
                });
            }

            var userID =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            bool hasCategories = await _context.Category
                .AnyAsync(c => c.eventID == eventID);

            bool hasEnrollments = await _context.Enrollment
                .AnyAsync(e => e.eventID == eventID);

            if (hasCategories || hasEnrollments)
            {
                return Conflict(new
                {
                    status = 409,
                    message = "Cannot delete this event because it has existing categories or enrollments."
                });
            }

            _context.Event.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully.",
                eventID
            });
        }//end of DeleteEvent

    }//end of class: EventsController

}//end of namespace