using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

}
