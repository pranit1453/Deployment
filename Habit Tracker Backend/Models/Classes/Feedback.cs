using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("feedback")]
    public class Feedback
    {
        [Key]
        [Column("feedback_id")]
        public long FeedbackId { get; set; }

        [Required, MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("rating")]
        public int Rating { get; set; }

        [Required, MaxLength(2000)]
        [Column("message")]
        public string Message { get; set; } = null!;

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }
    }
}
