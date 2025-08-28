using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Alabaster.Models
{
    public class Volunteer
    {
        [Required(ErrorMessage = "Please select an event.")]
        public string EventId { get; set; }

        [BindNever] // prevent users from binding this manually
        public string? EventName { get; set; }

        [Required(ErrorMessage = "Your name is required.")]
        public string Name { get; set; }

        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string Phone { get; set; }

        public string Notes { get; set; }

        public string TShirtSize { get; set; }

        [Range(1, 50, ErrorMessage = "Please enter a valid number of volunteers.")]
        public int NumberOfVolunteers { get; set; } = 1;
    }
}
