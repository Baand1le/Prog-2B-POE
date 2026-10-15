using Microsoft.AspNetCore.Mvc;

namespace RaceDayApp_POE.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
