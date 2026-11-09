using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiService _api;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApiService api, ILogger<HomeController> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<ApiEvent>>("api/events");
        ViewBag.ApiMessage = result.IsSuccess ? null : result.Message;
        var events = result.Data ?? new List<ApiEvent>();
        return View(events.Where(e => e.Date >= DateTime.Now)
            .OrderBy(e => e.Date).Take(6).ToList());
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
