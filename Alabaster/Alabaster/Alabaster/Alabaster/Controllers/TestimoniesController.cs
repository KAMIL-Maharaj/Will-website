using Alabaster.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Alabaster.Controllers
{
    public class TestimoniesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string firebaseUrl = "https://alabaster-8cfcd-default-rtdb.firebaseio.com"; // Replace with your actual Firebase Realtime DB URL (without `/` at end)

        public TestimoniesController()
        {
            _httpClient = new HttpClient();
        }

        // Display all testimonies
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{firebaseUrl}/testimonies.json");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return View(new List<Testimony>());

            var dict = JsonConvert.DeserializeObject<Dictionary<string, Testimony>>(json);

            // Add Firebase keys as IDs
            foreach (var item in dict)
                item.Value.Id = item.Key;

            return View(dict.Values);
        }

        // Show form to create a new testimony
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Handle submission of the testimony form
        [HttpPost]
        public async Task<IActionResult> Create(Testimony model, IFormFile ImageUpload)
        {
            if (ImageUpload != null && ImageUpload.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageUpload.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                model.ImageBase64 = Convert.ToBase64String(imageBytes);
            }

            model.CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            model.CreatedBy = User.Identity?.Name ?? "Anonymous";

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{firebaseUrl}/testimonies.json", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to submit testimony. Please try again.");
            return View(model);
        }
    }
}
