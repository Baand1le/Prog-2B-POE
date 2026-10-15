using Microsoft.AspNetCore.Mvc;

namespace RaceDayApp_POE.Controllers
{
    public class EnrollmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
