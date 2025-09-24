using Microsoft.AspNetCore.Mvc;
using Alabaster.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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
            var eventList = events.Select(e =>
            {
                var ev = e.Object;
                ev.Id = e.Key;
                return ev;
            }).ToList();

            // Move past events to PastEvents
            await MovePastEvents(eventList);

            var upcomingEvents = eventList
                .Where(e => DateTime.TryParse(e.Date, out DateTime date) && date >= DateTime.Today)
                .OrderBy(e => DateTime.Parse(e.Date))
                .ToList();

            return View(upcomingEvents);
        }

        // GET: /Events/AddEvent (Admin only)
        [HttpGet]
        public IActionResult AddEvent()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // POST: /Events/AddEvent (Admin only)
        [HttpPost]
        public async Task<IActionResult> AddEvent(UpcomingEvent model)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
                return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                await _firebase.Child("Events").PostAsync(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: /Events/Volunteer
        [HttpGet]
        public async Task<IActionResult> Volunteer(string eventId = null)
        {
            // Ensure user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["ErrorMessage"] = "You must log in to volunteer.";
                return RedirectToAction("Login", "Auth");
            }

            var events = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
            var eventList = events.Select(e =>
            {
                var ev = e.Object;
                ev.Id = e.Key;
                return ev;
            }).ToList();

            ViewBag.Events = eventList;

            var volunteer = new Volunteer();

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
            // Ensure user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["ErrorMessage"] = "You must log in to volunteer.";
                return RedirectToAction("Login", "Auth");
            }

            // Fetch events for dropdown
            var allEvents = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
            var eventList = allEvents.Select(e =>
            {
                var ev = e.Object;
                ev.Id = e.Key;
                return ev;
            }).ToList();

            ViewBag.Events = eventList;

            // Server-side validation
            if (string.IsNullOrEmpty(model.EventId))
                ModelState.AddModelError("EventId", "Please select an event.");

            if (!string.IsNullOrEmpty(model.IdNumber) && !System.Text.RegularExpressions.Regex.IsMatch(model.IdNumber, @"^\d{13}$"))
                ModelState.AddModelError("IdNumber", "ID Number must be exactly 13 digits.");

            if (!string.IsNullOrEmpty(model.Phone) && !System.Text.RegularExpressions.Regex.IsMatch(model.Phone, @"^\d{10}$"))
                ModelState.AddModelError("Phone", "Phone number must be exactly 10 digits.");

            // Notes is optional

            if (!ModelState.IsValid)
                return View(model);

            var selectedEvent = eventList.FirstOrDefault(e => e.Id == model.EventId);
            if (selectedEvent != null)
                model.EventName = selectedEvent.Name;

            try
            {
                await _firebase.Child("Volunteers").PostAsync(model);
                TempData["Message"] = "Thank you for volunteering!";
                return RedirectToAction("VolunteerThankYou");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to save volunteer: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Events/VolunteerThankYou
        [HttpGet]
        public IActionResult VolunteerThankYou()
        {
            return View();
        }

        // GET: /Events/Past
        [HttpGet]
        public async Task<IActionResult> Past()
        {
            var pastEvents = await _firebase.Child("PastEvents").OnceAsync<UpcomingEvent>();
            var pastList = pastEvents.Select(e =>
            {
                var ev = e.Object;
                ev.Id = e.Key;
                return ev;
            })
            .OrderByDescending(e => DateTime.Parse(e.Date))
            .ToList();

            return View(pastList);
        }

        // Move past events from Events to PastEvents in Firebase
        private async Task MovePastEvents(List<UpcomingEvent> eventList)
        {
            DateTime today = DateTime.Today;

            foreach (var evt in eventList.ToList())
            {
                if (DateTime.TryParse(evt.Date, out DateTime eventDate) && eventDate < today)
                {
                    await _firebase.Child("PastEvents").PostAsync(evt);
                    await _firebase.Child("Events").Child(evt.Id).DeleteAsync();
                    eventList.Remove(evt);
                }
            }
        }
    }
}
