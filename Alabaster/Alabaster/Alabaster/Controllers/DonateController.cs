using Microsoft.AspNetCore.Mvc;

namespace Alabaster.Controllers
{
    public class DonateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
