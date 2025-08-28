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
            // Load all events from Firebase
            var events = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
            var eventList = events.Select(e =>
            {
                var ev = e.Object;
                ev.Id = e.Key; // store Firebase ID
                return ev;
            }).ToList();

            // Move past events to PastEvents
            await MovePastEvents(eventList);

            // Filter only upcoming events
            var upcomingEvents = eventList
                .Where(e => DateTime.TryParse(e.Date, out DateTime date) && date >= DateTime.Today)
                .OrderBy(e => DateTime.Parse(e.Date))
                .ToList();

            return View(upcomingEvents);
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
            var eventList = events.Select(e =>
            {
                var ev = e.Object;
                ev.Id = e.Key;
                return ev;
            }).ToList();
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
                    var selectedEvent = events.Select(e =>
                    {
                        var ev = e.Object;
                        ev.Id = e.Key;
                        return ev;
                    }).FirstOrDefault(e => e.Id == model.EventId);

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

            foreach (var evt in eventList.ToList()) // work on a copy
            {
                if (DateTime.TryParse(evt.Date, out DateTime eventDate) && eventDate < today)
                {
                    // Add to PastEvents
                    await _firebase.Child("PastEvents").PostAsync(evt);

                    // Remove from Events
                    await _firebase.Child("Events").Child(evt.Id).DeleteAsync();

                    // Remove locally so it won't be shown as upcoming
                    eventList.Remove(evt);
                }
            }
        }
    }
}
