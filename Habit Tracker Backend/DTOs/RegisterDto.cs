using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Habit_Tracker_Backend.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(15)]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be exactly 10 digits starting with 6, 7, 8, or 9")]
        public string MobileNumber { get; set; } = null!;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = null!;

        public DateTime? Dob { get; set; }
    }
}
