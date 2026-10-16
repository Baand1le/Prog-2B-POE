using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Data;
using RaceDayApp_POE.DTOs;
using RaceDayApp_POE.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Results = RaceDayApp_POE.Models.Results;

namespace RaceDayApp_POE.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrollments/{enrollmentID}/result
        [HttpPost("enrollments/{enrollmentID}/result")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "Record a Participant's race result",
            Description = "Allow the event's Organiser to record the finishing time and finishing position of" +
            " an enrolled Participant."
        )]
        public async Task<IActionResult> CreateResult(
            string enrollmentID,
            [FromBody] CreateResultDTO dto)
        {
            var enrollment = await _context.Enrollment
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.enrollmentID == enrollmentID);

            if (enrollment == null)
                return NotFound("Enrollment not found.");

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (enrollment.Event.organiserID != userID)
                return Forbid();

            if (dto.finishTime <= TimeSpan.Zero)
                return BadRequest(
                    "Finish time must be greater than zero.");

            bool resultExists = await _context.Result
                .AnyAsync(r => r.enrollmentID == enrollmentID);

            if (resultExists)
                return Conflict(
                    "A result already exists for this enrollment.");

            var result = new Results
            {
                resultID = Guid.NewGuid()
                    .ToString("N")[..20],
                enrollmentID = enrollmentID,
                finishTime = dto.finishTime,
                finishPosition = dto.finishPosition
            };

            _context.Result.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                result.resultID,
                result.enrollmentID,
                result.finishTime,
                result.finishPosition
            });
        }

        // PUT: api/enrollments/{enrollmentID}/result
        [HttpPut("enrollments/{enrollmentID}/result")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "Update a Participant's race result",
            Description = "Allow the event's Organiser to correct or update a previously recorded finishing time or " +
            "position."
        )]
        public async Task<IActionResult> UpdateResult(
            string enrollmentID,
            [FromBody] CreateResultDTO dto)
        {
            var enrollment = await _context.Enrollment
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.enrollmentID == enrollmentID);

            if (enrollment == null)
                return NotFound("Enrollment not found.");

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (enrollment.Event.organiserID != userID)
                return Forbid();

            if (dto.finishTime <= TimeSpan.Zero)
                return BadRequest(
                    "Finish time must be greater than zero.");

            var result = await _context.Result
                .FirstOrDefaultAsync(r =>
                    r.enrollmentID == enrollmentID);

            if (result == null)
                return NotFound("Result not found.");

            result.finishTime = dto.finishTime;
            result.finishPosition = dto.finishPosition;

            await _context.SaveChangesAsync();

            return Ok("Result updated successfully.");
        }

        // GET: api/results/mine
        [HttpGet("results/mine")]
        [Authorize(Roles = "Participant")]
        [SwaggerOperation(
            Summary = "View your race results",
            Description = "Retrieve the logged-in Participant's race history, including finishing times," +
            " positions and total recorded finishers."
        )]
        public async Task<IActionResult> GetMyResults()
        {
            var participantID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var results = await _context.Result
                .AsNoTracking()
                .Where(r =>
                    r.Enrollment.participantID == participantID)
                .Select(r => new
                {
                    r.resultID,
                    r.enrollmentID,
                    eventID = r.Enrollment.eventID,
                    eventName = r.Enrollment.Event.Name,
                    eventDate = r.Enrollment.Event.Date,
                    categoryName =
                        r.Enrollment.Category.categoryName,
                    r.finishTime,
                    r.finishPosition,
                    totalFinishers = _context.Result.Count(
                        other =>
                            other.Enrollment.eventID ==
                            r.Enrollment.eventID)
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/events/{eventID}/results
        [HttpGet("events/{eventID}/results")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(
            Summary = "View all results for a race event",
            Description = "Allow the event's Organiser to view the recorded finishing times and positions" +
            " of Participants in that event."
        )]
        public async Task<IActionResult> GetEventResults(
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

            var results = await _context.Result
                .AsNoTracking()
                .Where(r =>
                    r.Enrollment.eventID == eventID)
                .Select(r => new
                {
                    r.resultID,
                    r.enrollmentID,
                    participantID =
                        r.Enrollment.participantID,
                    firstName =
                        r.Enrollment.Participant.firstName,
                    surname =
                        r.Enrollment.Participant.Surname,
                    categoryName =
                        r.Enrollment.Category.categoryName,
                    r.finishTime,
                    r.finishPosition
                })
                .OrderBy(r => r.finishPosition)
                .ToListAsync();

            return Ok(results);
        }
    }
}