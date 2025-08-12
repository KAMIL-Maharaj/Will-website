using Microsoft.AspNetCore.Mvc;

namespace Alabaster.Controllers
{
    public class UpcomingEventsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddUpcomingEvents()
        {
            return View();
        }
        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult Volunteer()
        {
            return View();
        }
    }
}
