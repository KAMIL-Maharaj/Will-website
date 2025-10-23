using Alabaster.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Alabaster.Controllers
{
    public class TestimoniesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string firebaseUrl = "https://alabaster-8cfcd-default-rtdb.firebaseio.com"; 

        public TestimoniesController()
        {
            _httpClient = new HttpClient();
        }

        // ===== HELPER PATCH METHOD =====
        private async Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = content
            };
            return await _httpClient.SendAsync(request);
        }

        // ===== DISPLAY APPROVED TESTIMONIES =====
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{firebaseUrl}/testimonies.json");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return View(new List<Testimony>());

            var dict = JsonConvert.DeserializeObject<Dictionary<string, Testimony>>(json);
            foreach (var item in dict)
                item.Value.Id = item.Key;

            var approved = dict.Values.Where(t => t.IsApproved).ToList();
            return View(approved);
        }

        // ===== SHOW FORM TO CREATE TESTIMONY =====
        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("FirebaseToken")))
            {
                TempData["Error"] = "You must be logged in to submit a testimony.";
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // ===== HANDLE FORM SUBMISSION =====
        [HttpPost]
        public async Task<IActionResult> Create(Testimony model, IFormFile ImageUpload, string NameOption, string CustomName)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("FirebaseToken")))
            {
                TempData["Error"] = "You must be logged in to submit a testimony.";
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Description))
            {
                ModelState.AddModelError("", "Title and Description are required.");
                return View(model);
            }

            if (NameOption == "Custom" && string.IsNullOrEmpty(CustomName))
            {
                ModelState.AddModelError("", "Please enter your name or choose Anonymous.");
                return View(model);
            }

            if (ImageUpload != null && ImageUpload.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageUpload.CopyToAsync(ms);
                model.ImageBase64 = Convert.ToBase64String(ms.ToArray());
            }
            else
            {
                model.ImageBase64 = "";
            }

            model.CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            model.CreatedBy = NameOption == "Custom" ? CustomName : "Anonymous";
            model.IsApproved = false; // ✅ Default: Pending approval

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{firebaseUrl}/testimonies.json", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to submit testimony. Please try again.");
            return View(model);
        }

        // ===== ADMIN VIEW: PENDING TESTIMONIES =====
        public async Task<IActionResult> Admin()
        {
            // Optional: restrict to admin users
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction("Index");
            }

            var response = await _httpClient.GetAsync($"{firebaseUrl}/testimonies.json");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return View(new List<Testimony>());

            var dict = JsonConvert.DeserializeObject<Dictionary<string, Testimony>>(json);
            foreach (var item in dict)
                item.Value.Id = item.Key;

            var pending = dict.Values.Where(t => !t.IsApproved).ToList();
            return View(pending);
        }

        // ===== APPROVE TESTIMONY =====
        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Admin");

            var updateData = new { IsApproved = true };
            var json = JsonConvert.SerializeObject(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await PatchAsync($"{firebaseUrl}/testimonies/{id}.json", content);

            return RedirectToAction("Admin");
        }

        // ===== REJECT / DELETE TESTIMONY =====
        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Admin");

            await _httpClient.DeleteAsync($"{firebaseUrl}/testimonies/{id}.json");
            return RedirectToAction("Admin");
        }
    }
}
