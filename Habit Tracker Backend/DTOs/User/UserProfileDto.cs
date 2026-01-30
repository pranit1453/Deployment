using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs.User
{
    public class UserProfileResponseDto
    {
        public long UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public DateTime? Dob { get; set; }
        public bool EmailNotificationsEnabled { get; set; } = true;
    }

    public class UpdateProfileDto
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
        public string MobileNumber { get; set; } = null!;

        public DateTime? Dob { get; set; }
    }
}
