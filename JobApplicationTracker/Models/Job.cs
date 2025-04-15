using System;
using System.ComponentModel.DataAnnotations;

namespace JobApplicationTracker.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public decimal Salary { get; set; }

        [Required]
        public string Company { get; set; }

        public string PostedBy { get; set; } = string.Empty;

        public DateTime PostedDate { get; set; } = DateTime.Now;
    }
}
