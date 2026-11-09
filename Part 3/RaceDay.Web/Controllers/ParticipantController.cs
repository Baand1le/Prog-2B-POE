using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireRole("Participant")]
public class ParticipantController : Controller
{
    private readonly ApiService _api;
    private readonly LocalImageService _images;

    public ParticipantController(ApiService api, LocalImageService images)
    {
        _api = api;
        _images = images;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? eventType)
    {
        var result = await _api.GetAsync<List<ApiEvent>>("api/events");
        if (!result.IsSuccess)
            TempData["Error"] = result.Message;

        var events = (result.Data ?? new List<ApiEvent>())
            .Where(e => e.Date >= DateTime.Now)
            .OrderBy(e => e.Date);

        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).OrderBy(e => e.Date);
        if (!string.IsNullOrWhiteSpace(eventType))
            events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).OrderBy(e => e.Date);

        return View(new ParticipantEventsViewModel
        {
            Events = events.ToList(),
            Search = search,
            EventType = eventType
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string eventID)
    {
        var eventResult = await _api.GetAsync<ApiEvent>($"api/events/{eventID}");
        if (!eventResult.IsSuccess || eventResult.Data == null)
            return NotFound();

        var categories = await _api.GetAsync<List<ApiCategory>>($"api/events/{eventID}/categories");
        return View(new EventDetailsViewModel
        {
            Event = eventResult.Data,
            Categories = categories.Data ?? new List<ApiCategory>(),
            BannerUrl = _images.Find("events", eventID)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(string eventID, string categoryID)
    {
        if (string.IsNullOrWhiteSpace(categoryID))
        {
            TempData["Error"] = "Choose an event category before enrolling.";
            return RedirectToAction(nameof(Details), new { eventID });
        }

        var result = await _api.PostAsync<EnrollmentRequest, ApiMessage>("api/enrollments", new EnrollmentRequest
        {
            CategoryID = categoryID
        });

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "You are now enrolled. Your place has been saved."
            : result.Message;
        return RedirectToAction(nameof(Details), new { eventID });
    }

    [HttpGet]
    public async Task<IActionResult> MyEnrollments()
    {
        var enrollments = await _api.GetAsync<List<ApiParticipantEnrollment>>("api/enrollments/mine");
        if (!enrollments.IsSuccess)
        {
            TempData["Error"] = enrollments.Message;
            return View(new ParticipantEnrollmentsViewModel());
        }

        var results = await _api.GetAsync<List<ApiMyResult>>("api/results/mine");
        var resultLookup = (results.Data ?? new List<ApiMyResult>()).ToDictionary(r => r.EventID, r => r);
        var model = new ParticipantEnrollmentsViewModel();

        foreach (var enrollment in enrollments.Data ?? new List<ApiParticipantEnrollment>())
        {
            resultLookup.TryGetValue(enrollment.EventID, out var raceResult);
            var status = enrollment.EventDate > DateTime.Now
                ? "Upcoming"
                : raceResult != null ? "Result published" : "Awaiting result";

            model.Enrollments.Add(new ParticipantEnrollmentRow
            {
                Enrollment = enrollment,
                Status = status,
                ResultSummary = raceResult == null ? null :
                    $"{raceResult.FinishTime:hh\\:mm\\:ss} · position {raceResult.FinishPosition} of {raceResult.TotalFinishers}"
            });
        }

        model.Enrollments = model.Enrollments.OrderBy(e => e.Enrollment.EventDate).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetAsync<List<ApiMyResult>>("api/results/mine");
        if (!result.IsSuccess)
            TempData["Error"] = result.Message;
        var results = (result.Data ?? new List<ApiMyResult>()).OrderByDescending(r => r.EventDate).ToList();
        return View(results);
    }
}
