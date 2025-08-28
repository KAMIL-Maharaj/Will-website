using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Alabaster.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            // Check if the user is logged in and has the "Admin" role
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Redirect non-admins to login or home page
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}
