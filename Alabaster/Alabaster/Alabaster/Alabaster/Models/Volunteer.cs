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

        [Required(ErrorMessage = "ID Number is required.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be exactly 13 digits.")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string Phone { get; set; }

        public string? Notes { get; set; } // optional

        [Range(1, 50, ErrorMessage = "Please enter between 1 and 50 volunteers.")]
        public int NumberOfVolunteers { get; set; } = 1;
    }
}
