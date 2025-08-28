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
            try
            {
                var events = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
                var eventList = events.Select(e =>
                {
                    var ev = e.Object;
                    ev.Id = e.Key;
                    return ev;
                }).ToList();

                await MovePastEvents(eventList);

                var upcomingEvents = eventList
                    .Where(e => DateTime.TryParse(e.Date, out DateTime date) && date >= DateTime.Today)
                    .OrderBy(e => DateTime.Parse(e.Date))
                    .ToList();

                return View(upcomingEvents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load events. " + ex.Message;
                return View(new List<UpcomingEvent>());
            }
        }

        // GET: /Events/AddEvent
        [HttpGet]
        public IActionResult AddEvent() => View();

        // POST: /Events/AddEvent
        [HttpPost]
        public async Task<IActionResult> AddEvent(UpcomingEvent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _firebase.Child("Events").PostAsync(model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to add event: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: /Events/Volunteer
        [HttpGet]
        public async Task<IActionResult> Volunteer(string eventId)
        {
            try
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

                // Clear validation errors so they don't show on GET
                ModelState.Clear();

                return View(volunteer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Could not load events. " + ex.Message;
                return View(new Volunteer());
            }
        }

        // POST: /Events/Volunteer
        [HttpPost]
        public async Task<IActionResult> Volunteer(Volunteer model)
        {
            if (ModelState.IsValid)
            {
                try
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

                    await _firebase.Child("Volunteers").PostAsync(model);
                    TempData["Message"] = "Thank you for volunteering!";
                    return RedirectToAction("VolunteerThankYou");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to save volunteer: " + ex.Message);
                }
            }

            try
            {
                var allEvents = await _firebase.Child("Events").OnceAsync<UpcomingEvent>();
                ViewBag.Events = allEvents.Select(e => e.Object).ToList();
            }
            catch
            {
                ViewBag.Events = new List<UpcomingEvent>();
                ModelState.AddModelError("", "Could not load events list.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VolunteerThankYou() => View();

        // GET: /Events/Past
        [HttpGet]
        public async Task<IActionResult> Past()
        {
            try
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
            catch (Exception ex)
            {
                TempData["Error"] = "Could not load past events. " + ex.Message;
                return View(new List<UpcomingEvent>());
            }
        }

        // Move past events from Events to PastEvents
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
