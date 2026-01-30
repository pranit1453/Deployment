using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs.Feedback
{
    public class SubmitFeedbackDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [MaxLength(2000)]
        public string Message { get; set; } = null!;
    }
}
