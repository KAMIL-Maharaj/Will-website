using Microsoft.AspNetCore.Mvc;
using Alabaster.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Alabaster.Controllers
{
    public class EventsController : Controller
    {
        private readonly FirebaseClient _firebase;

        public EventsController()
        {
            _firebase = new FirebaseClient("https://alabaster-8cfcd-default-rtdb.firebaseio.com/");
        }

        // GET: /Events
        public async Task<IActionResult> Index()
        {
            var events = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
            var eventList = events.Select(e => e.Object).ToList();
            return View(eventList);
        }

        // GET: /Events/AddEvent
        [HttpGet]
        public IActionResult AddEvent()
        {
            return View();
        }

        // POST: /Events/AddEvent
        [HttpPost]
        public async Task<IActionResult> AddEvent(UpcomingEvent model)
        {
            if (ModelState.IsValid)
            {
                await _firebase.Child("Events").PostAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: /Events/Volunteer
        [HttpGet]
        public async Task<IActionResult> Volunteer(string eventId)
        {
            var events = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
            var eventList = events.Select(e => e.Object).ToList();
            ViewBag.Events = eventList;

            Volunteer volunteer = new Volunteer();

            if (!string.IsNullOrEmpty(eventId))
            {
                var selectedEvent = eventList.FirstOrDefault(e => e.Id == eventId);
                if (selectedEvent != null)
                {
                    volunteer.EventId = selectedEvent.Id;
                    volunteer.EventName = selectedEvent.Name;
                }
            }

            return View(volunteer);
        }

        // POST: /Events/Volunteer
        [HttpPost]
        public async Task<IActionResult> Volunteer(Volunteer model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.EventId))
                {
                    var events = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
                    var selectedEvent = events.Select(e => e.Object).FirstOrDefault(e => e.Id == model.EventId);
                    if (selectedEvent != null)
                    {
                        model.EventName = selectedEvent.Name;
                    }
                }

                try
                {
                    await _firebase.Child("Volunteers").PostAsync(model);
                    TempData["Message"] = "Thank you for volunteering!";
                    return RedirectToAction("VolunteerThankYou");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to save volunteer: " + ex.Message);
                }
            }

            // Reload events for dropdown if ModelState invalid or save failed
            var allEvents = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
            ViewBag.Events = allEvents.Select(e => e.Object).ToList();

            return View(model);
        }

        // GET: /Events/VolunteerThankYou
        [HttpGet]
        public IActionResult VolunteerThankYou()
        {
            return View();
        }
    }
}
