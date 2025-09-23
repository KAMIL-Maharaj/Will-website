using Microsoft.AspNetCore.Mvc;
using Alabaster.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace Alabaster.Controllers
{
    public class PrayerController : Controller
    {
        private readonly FirebaseClient _firebase;

        public PrayerController()
        {
            _firebase = new FirebaseClient("https://alabaster-8cfcd-default-rtdb.firebaseio.com/");
        }

        // User: Prayer Request Form
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("FirebaseToken") == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // User: Submit Prayer Request
        [HttpPost]
        public async Task<IActionResult> Submit(string name, string request)
        {
            if (HttpContext.Session.GetString("FirebaseToken") == null)
                return RedirectToAction("Login", "Auth");

            var prayer = new PrayerRequest
            {
                UserId = HttpContext.Session.GetString("UserId"),
                UserEmail = HttpContext.Session.GetString("UserEmail"),
                Name = name,
                Request = request
            };

            await _firebase.Child("prayers").Child(prayer.Id).PutAsync(prayer);

            TempData["Success"] = "Your prayer request has been sent 🙏";
            return RedirectToAction("Index");
        }

        // Admin: View All Prayer Requests
        public async Task<IActionResult> AdminView()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var prayers = await _firebase.Child("prayers").OnceAsync<PrayerRequest>();
            var prayerList = prayers.Select(p => p.Object).OrderByDescending(p => p.CreatedAt).ToList();

            return View(prayerList);
        }
    }
}
