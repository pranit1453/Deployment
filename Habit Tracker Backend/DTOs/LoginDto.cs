using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = null!;
    }
}
