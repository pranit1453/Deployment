using Habit_Tracker_Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("users")]
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        [Column("user_id")]
        public long UserId { get; set; }

        [Required, MaxLength(50)]
        [Column("first_name")]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)]
        [Column("middle_name")]
        public string? MiddleName { get; set; }

        [Required, MaxLength(50)]
        [Column("last_name")]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(50)]
        [Column("username")]
        public string Username { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required, MaxLength(15)]
        [Column("mobile_number")]
        public string MobileNumber { get; set; } = null!;

        [Required, MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [Column("dob")]
        public DateTime? Dob { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_mobile_verified")]
        public bool IsMobileVerified { get; set; }

        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [Required]
        [Column("role")]
        public Role Role { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [Column("email_notifications_enabled")]
        public bool EmailNotificationsEnabled { get; set; } = true;

        //// Navigation
        //public ICollection<Habit> Habits { get; set; } = new List<Habit>();
        public ICollection<UserOtp> UserOtps { get; set; } = new List<UserOtp>();
    }


}
