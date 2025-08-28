using Microsoft.AspNetCore.Mvc;

namespace Alabaster.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
