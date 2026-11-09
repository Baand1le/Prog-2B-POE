using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

[RequireRole("Organiser")]
public class OrganiserController : Controller
{
    private readonly ApiService _api;
    private readonly LocalImageService _images;

    public OrganiserController(ApiService api, LocalImageService images)
    {
        _api = api;
        _images = images;
    }

    public async Task<IActionResult> Index()
    {
        var model = await BuildDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Events()
    {
        var model = await BuildDashboardAsync();
        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => View("EventForm", new EventFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (model.Date <= DateTime.Now)
            ModelState.AddModelError(nameof(model.Date), "Choose a future event date.");

        if (!ModelState.IsValid)
            return View("EventForm", model);

        var result = await _api.PostAsync<EventRequest, ApiEventSaveResponse>("api/events", ToEventRequest(model));
        if (!result.IsSuccess || result.Data == null || string.IsNullOrWhiteSpace(result.Data.EventID))
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View("EventForm", model);
        }

        try
        {
            if (model.BannerImage != null)
                await _images.SaveAsync(model.BannerImage, "events", result.Data.EventID);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = "The event was created, but its banner could not be saved: " + ex.Message;
            return RedirectToAction(nameof(Events));
        }

        TempData["Success"] = "Your event has been created.";
        return RedirectToAction(nameof(Categories), new { eventID = result.Data.EventID });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string eventID)
    {
        var raceEvent = await GetOwnedEventAsync(eventID);
        if (raceEvent == null)
            return NotFound();

        return View("EventForm", new EventFormViewModel
        {
            EventID = raceEvent.EventID,
            Name = raceEvent.Name,
            Description = raceEvent.Description,
            Date = raceEvent.Date,
            Location = raceEvent.Location,
            Distance = raceEvent.Distance,
            EventType = raceEvent.EventType,
            ExistingBannerUrl = _images.Find("events", raceEvent.EventID)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string eventID, EventFormViewModel model)
    {
        model.EventID = eventID;
        if (!ModelState.IsValid)
        {
            model.ExistingBannerUrl = _images.Find("events", eventID);
            return View("EventForm", model);
        }

        var result = await _api.PutAsync<EventRequest, ApiEventSaveResponse>($"api/events/{eventID}", ToEventRequest(model));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            model.ExistingBannerUrl = _images.Find("events", eventID);
            return View("EventForm", model);
        }

        try
        {
            if (model.BannerImage != null)
                await _images.SaveAsync(model.BannerImage, "events", eventID);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = "Event details were saved, but the banner could not be uploaded: " + ex.Message;
            return RedirectToAction(nameof(Events));
        }

        TempData["Success"] = "Event details updated.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string eventID)
    {
        var result = await _api.DeleteAsync($"api/events/{eventID}");
        if (!result.IsSuccess)
            TempData["Error"] = result.Message;
        else
        {
            _images.Delete("events", eventID);
            TempData["Success"] = "The event was deleted.";
        }
        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> Categories(string eventID, string? categoryID = null)
    {
        var raceEvent = await GetOwnedEventAsync(eventID);
        if (raceEvent == null)
            return NotFound();

        var categoriesResult = await _api.GetAsync<List<ApiCategory>>($"api/events/{eventID}/categories");
        if (!categoriesResult.IsSuccess)
        {
            TempData["Error"] = categoriesResult.Message;
            return RedirectToAction(nameof(Events));
        }

        var model = new CategoryFormViewModel
        {
            EventID = eventID,
            EventName = raceEvent.Name,
            Categories = categoriesResult.Data ?? new List<ApiCategory>()
        };

        if (!string.IsNullOrWhiteSpace(categoryID))
        {
            var category = model.Categories.FirstOrDefault(c => c.CategoryID == categoryID);
            if (category != null)
            {
                model.CategoryID = category.CategoryID;
                model.CategoryName = category.CategoryName;
                model.AgeGroup = category.AgeGroup;
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Categories(CategoryFormViewModel model)
    {
        var raceEvent = await GetOwnedEventAsync(model.EventID);
        if (raceEvent == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            var existing = await _api.GetAsync<List<ApiCategory>>($"api/events/{model.EventID}/categories");
            model.Categories = existing.Data ?? new List<ApiCategory>();
            model.EventName = raceEvent.Name;
            return View(model);
        }

        ApiResult<ApiMessage> result;
        var request = new CategoryRequest { CategoryName = model.CategoryName.Trim(), AgeGroup = model.AgeGroup.Trim() };
        if (string.IsNullOrWhiteSpace(model.CategoryID))
            result = await _api.PostAsync<CategoryRequest, ApiMessage>($"api/events/{model.EventID}/categories", request);
        else
            result = await _api.PutAsync<CategoryRequest, ApiMessage>($"api/events/{model.EventID}/categories/{model.CategoryID}", request);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? (string.IsNullOrWhiteSpace(model.CategoryID) ? "Category added." : "Category updated.")
            : result.Message;
        return RedirectToAction(nameof(Categories), new { eventID = model.EventID });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(string eventID, string categoryID)
    {
        var result = await _api.DeleteAsync($"api/events/{eventID}/categories/{categoryID}");
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Category deleted." : result.Message;
        return RedirectToAction(nameof(Categories), new { eventID });
    }

    [HttpGet]
    public async Task<IActionResult> Enrollments(string eventID)
    {
        var raceEvent = await GetOwnedEventAsync(eventID);
        if (raceEvent == null)
            return NotFound();

        var enrollments = await _api.GetAsync<List<ApiEventEnrollment>>($"api/events/{eventID}/enrollments");
        if (!enrollments.IsSuccess)
        {
            TempData["Error"] = enrollments.Message;
            return RedirectToAction(nameof(Events));
        }

        var results = await _api.GetAsync<List<ApiEventResult>>($"api/events/{eventID}/results");
        var model = new OrganiserEnrollmentsViewModel
        {
            Event = raceEvent,
            Enrollments = enrollments.Data ?? new List<ApiEventEnrollment>(),
            ResultEnrollmentIDs = (results.Data ?? new List<ApiEventResult>()).Select(r => r.EnrollmentID).ToHashSet()
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Results(string eventID)
    {
        var raceEvent = await GetOwnedEventAsync(eventID);
        if (raceEvent == null)
            return NotFound();

        var enrollments = await _api.GetAsync<List<ApiEventEnrollment>>($"api/events/{eventID}/enrollments");
        var results = await _api.GetAsync<List<ApiEventResult>>($"api/events/{eventID}/results");
        if (!enrollments.IsSuccess || !results.IsSuccess)
        {
            TempData["Error"] = !enrollments.IsSuccess ? enrollments.Message : results.Message;
            return RedirectToAction(nameof(Events));
        }

        var resultLookup = (results.Data ?? new List<ApiEventResult>()).ToDictionary(r => r.EnrollmentID, r => r);
        var model = new OrganiserResultsViewModel
        {
            Event = raceEvent,
            Rows = (enrollments.Data ?? new List<ApiEventEnrollment>()).Select(e => new OrganiserResultRow
            {
                Enrollment = e,
                Result = resultLookup.GetValueOrDefault(e.EnrollmentID)
            }).ToList()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveResult(string eventID, ResultEntryViewModel model)
    {
        var raceEvent = await GetOwnedEventAsync(eventID);
        if (raceEvent == null)
            return NotFound();

        if (raceEvent.Date > DateTime.Now)
        {
            TempData["Error"] = "Results can only be entered after the event has taken place.";
            return RedirectToAction(nameof(Results), new { eventID });
        }

        if (model.FinishPosition < 1 || !TimeSpan.TryParse(model.FinishTime, out var finishTime) || finishTime <= TimeSpan.Zero)
        {
            TempData["Error"] = "Enter a valid finish time greater than zero and a finishing position of at least 1.";
            return RedirectToAction(nameof(Results), new { eventID });
        }

        var eventEnrollments = await _api.GetAsync<List<ApiEventEnrollment>>($"api/events/{eventID}/enrollments");
        if (!eventEnrollments.IsSuccess || !(eventEnrollments.Data ?? new List<ApiEventEnrollment>()).Any(e => e.EnrollmentID == model.EnrollmentID))
        {
            TempData["Error"] = "The selected participant is not enrolled in this event.";
            return RedirectToAction(nameof(Results), new { eventID });
        }

        var eventResults = await _api.GetAsync<List<ApiEventResult>>($"api/events/{eventID}/results");
        if (!eventResults.IsSuccess)
        {
            TempData["Error"] = eventResults.Message;
            return RedirectToAction(nameof(Results), new { eventID });
        }

        var request = new ResultRequest { FinishTime = finishTime, FinishPosition = model.FinishPosition };
        var existing = (eventResults.Data ?? new List<ApiEventResult>()).FirstOrDefault(r => r.EnrollmentID == model.EnrollmentID);
        ApiResult<ApiMessage> saved;
        if (existing == null)
            saved = await _api.PostAsync<ResultRequest, ApiMessage>($"api/enrollments/{model.EnrollmentID}/result", request);
        else
            saved = await _api.PutAsync<ResultRequest, ApiMessage>($"api/enrollments/{model.EnrollmentID}/result", request);

        TempData[saved.IsSuccess ? "Success" : "Error"] = saved.IsSuccess ? "Race result saved." : saved.Message;
        return RedirectToAction(nameof(Results), new { eventID });
    }

    private async Task<OrganiserDashboardViewModel> BuildDashboardAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var eventsResult = await _api.GetAsync<List<ApiEvent>>("api/events");
        if (!eventsResult.IsSuccess)
        {
            TempData["Error"] = eventsResult.Message;
            return new OrganiserDashboardViewModel();
        }

        var ownEvents = (eventsResult.Data ?? new List<ApiEvent>())
            .Where(e => e.OrganiserID == userId)
            .OrderBy(e => e.Date)
            .ToList();

        var model = new OrganiserDashboardViewModel { TotalEvents = ownEvents.Count };
        foreach (var raceEvent in ownEvents)
        {
            var enrollments = await _api.GetAsync<List<ApiEventEnrollment>>($"api/events/{raceEvent.EventID}/enrollments");
            var results = await _api.GetAsync<List<ApiEventResult>>($"api/events/{raceEvent.EventID}/results");
            var count = enrollments.Data?.Count ?? 0;
            var resultCount = results.Data?.Count ?? 0;
            model.TotalEnrollments += count;
            model.Events.Add(new OrganiserEventCardViewModel
            {
                Event = raceEvent,
                EnrollmentCount = count,
                ResultCount = resultCount,
                BannerUrl = _images.Find("events", raceEvent.EventID)
            });
        }

        model.UpcomingEvents = ownEvents.Count(e => e.Date >= DateTime.Now);
        return model;
    }

    private async Task<ApiEvent?> GetOwnedEventAsync(string eventID)
    {
        var result = await _api.GetAsync<ApiEvent>($"api/events/{eventID}");
        if (!result.IsSuccess || result.Data == null)
            return null;
        return result.Data.OrganiserID == HttpContext.Session.GetString("UserId") ? result.Data : null;
    }

    private static EventRequest ToEventRequest(EventFormViewModel model) => new()
    {
        Name = model.Name.Trim(),
        Description = model.Description.Trim(),
        Date = model.Date,
        Location = model.Location.Trim(),
        Distance = model.Distance.Trim(),
        EventType = model.EventType
    };
}
