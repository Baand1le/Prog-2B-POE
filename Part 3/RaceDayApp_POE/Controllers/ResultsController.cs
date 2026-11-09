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
        [SwaggerResponse(201, "Race result recorded successfully.")]
        [SwaggerResponse(400, "Finishing time must be greater than zero.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "Only the event's Organiser can record results.")]
        [SwaggerResponse(404, "Enrollment not found.")]
        [SwaggerResponse(409, "A result already exists for this enrollment.")]
        public async Task<IActionResult> CreateResult(
            string enrollmentID,
            [FromBody] CreateResultDTO dto)
        {
            var enrollment = await _context.Enrollment
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.enrollmentID == enrollmentID);

            if (enrollment == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The enrollment could not be found. Please verify the enrollment ID."
                });

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (enrollment.Event.organiserID != userID)
                return Forbid();

            if (dto.finishTime <= TimeSpan.Zero)
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid finishing time. The finish time must be greater than zero."
                });

            bool resultExists = await _context.Result
                .AnyAsync(r => r.enrollmentID == enrollmentID);

            if (resultExists)
                return Conflict(new
                {
                    status = 409,
                    message = "A race result has already been recorded for this enrollment. Use the update endpoint to modify it."
                });

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
        [SwaggerResponse(200, "Race result updated successfully.")]
        [SwaggerResponse(400, "Invalid finishing time.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "You do not have permission to update this result.")]
        [SwaggerResponse(404, "Enrollment or result not found.")]
        public async Task<IActionResult> UpdateResult(
            string enrollmentID,
            [FromBody] CreateResultDTO dto)
        {
            var enrollment = await _context.Enrollment
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.enrollmentID == enrollmentID);

            if (enrollment == null)
                return NotFound(new
                {
                    status = 404,
                    message = "The enrollment could not be found. Please verify the enrollment ID."
                });

            var userID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (enrollment.Event.organiserID != userID)
                return Forbid();

            if (dto.finishTime <= TimeSpan.Zero)
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid finishing time. The finish time must be greater than zero."
                });

            var result = await _context.Result
                .FirstOrDefaultAsync(r =>
                    r.enrollmentID == enrollmentID);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    message = "No recorded result was found for this enrollment."
                });

            result.finishTime = dto.finishTime;
            result.finishPosition = dto.finishPosition;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Race result updated successfully.",
                result.resultID,
                result.enrollmentID,
                result.finishTime,
                result.finishPosition
            });
        }

        // GET: api/results/mine
        [HttpGet("results/mine")]
        [Authorize(Roles = "Participant")]
        [SwaggerOperation(
            Summary = "View your race results",
            Description = "Retrieve the logged-in Participant's race history, including finishing times," +
            " positions and total recorded finishers."
        )]
        [SwaggerResponse(200, "Your race results retrieved successfully.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "Only Participants can view their personal results.")]
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
        [SwaggerResponse(200, "Event results retrieved successfully.")]
        [SwaggerResponse(401, "Authentication required.")]
        [SwaggerResponse(403, "Only the event's Organiser can view these results.")]
        [SwaggerResponse(404, "Event not found.")]
        public async Task<IActionResult> GetEventResults(
            string eventID)
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