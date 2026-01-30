using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MinLength(6), MaxLength(6)]
        public string Otp { get; set; } = null!;

        [Required, MinLength(6), MaxLength(100)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }

}
