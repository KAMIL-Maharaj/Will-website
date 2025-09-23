using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Alabaster.Models
{
    public class Volunteer
    {
        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public string EventId { get; set; }

        [BindNever]
        public string? EventName { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [StringLength(13, ErrorMessage = "ID Number cannot exceed 13 characters.")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; }

        public string Notes { get; set; }
        public string TShirtSize { get; set; }

        [Range(1, 50, ErrorMessage = "Please enter between 1 and 50 volunteers.")]
        public int NumberOfVolunteers { get; set; } = 1;
    }
}
