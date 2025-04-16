/******************************************************************************
 * Application Name: Job Application Tracker
 * Author: Ji-Soo Kang
 * Instructor: Mahboob Ali
 ******************************************************************************/
using System.ComponentModel.DataAnnotations;

namespace JobApplicationTracker.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Optional extras (map to ApplicationUser if needed)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
         
        [Required(ErrorMessage = "Please select a role.")]
        [Display(Name = "Role")]
        public string Role { get; set; }

    }
}
