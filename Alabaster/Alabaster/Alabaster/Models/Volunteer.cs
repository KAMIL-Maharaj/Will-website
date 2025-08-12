using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Alabaster.Models
{
    public class Volunteer
    {
        public string EventId { get; set; }

        [BindNever]
        public string? EventName { get; set; }  // Make it nullable string

        public string Name { get; set; }
        public string IdNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Notes { get; set; }
        public string TShirtSize { get; set; }
        public int NumberOfVolunteers { get; set; }
    }
}
