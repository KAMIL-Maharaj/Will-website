using Microsoft.AspNetCore.Mvc;

namespace Alabaster.Controllers
{
    public class AdminController : Controller
    {
        // Admin-only dashboard
        public IActionResult Index()
        {
            // Check session for admin role
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}
