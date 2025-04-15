using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplicationTracker.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int JobId { get; set; }
        public string? ResumePath { get; set; } 
        public ApplicationStatus Status { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("JobId")]
        public Job Job { get; set; }
    }

    public enum ApplicationStatus
    {
        Pending,
        Interview,
        Rejected,
        Hired
    }
}
