using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
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
                return NotFound(raceEvent);
            return Ok(raceEvent);

        }//end of async GetEvent

        //Post: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
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
                    raceEvent.eventID,
                    raceEvent.organiserID,
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
        public async Task<IActionResult> UpdateEvent(
            string eventID, EventDTO dto)
        {
            var raceEvent = await _context.Event.FindAsync(eventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

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

            return Ok("Event updated successfully.");
        }//end of UpdateEvent

        //Delete: api/events/{eventID}
        [HttpDelete("{eventID}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(string eventID)
        {
            var raceEvent = await _context.Event.FindAsync(eventID);

            if (raceEvent == null)
                return NotFound("Event Not found");

            var userID =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (raceEvent.organiserID != userID)
                return Forbid();

            bool hasCategories = await _context.Category
                .AnyAsync(c => c.eventID == eventID);

            bool hasEnrollments = await _context.Enrollment
                .AnyAsync(e => e.eventID == eventID);

            if (hasCategories || hasEnrollments)
                return Conflict(
                    "Cannot delete and with categories or enrollments.");

            _context.Event.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();

        }//end of DeleteEvent

    }//end of class: EventsController

}//end of namespace