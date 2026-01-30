using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs
{
    public class VerifyEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
    }
}
