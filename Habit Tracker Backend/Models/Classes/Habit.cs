namespace Habit_Tracker_Backend.Models.Classes
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Habit_Tracker_Backend.Models.Enums;

    [Table("habits")]
    public class Habit
    {
        [Key]
        [Column("habit_id")]
        public long HabitId { get; set; }

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [Required]
        [Column("category_id")]
        public long CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("habit_name")]
        public string HabitName { get; set; } = null!;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Column("priority")]
        public HabitPriority Priority { get; set; } = HabitPriority.MEDIUM;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public HabitCategory HabitCategory { get; set; } = null!;

        public ICollection<HabitSchedule> HabitSchedules { get; set; }
            = new List<HabitSchedule>();

        public ICollection<HabitLog> HabitLogs { get; set; }
            = new List<HabitLog>();

        public HabitStreak? HabitStreak { get; set; }

        public HabitReminder? HabitReminder { get; set; }
    }

}
