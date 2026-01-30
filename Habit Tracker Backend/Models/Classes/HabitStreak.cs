using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("habit_streaks")]
    public class HabitStreak
    {
        [Key]
        [Column("streak_id")]
        public long StreakId { get; set; }

        [Required]
        [Column("habit_id")]
        public long HabitId { get; set; }

        [Required]
        [Column("current_streak")]
        public int CurrentStreak { get; set; } = 0;  

        [Required]
        [Column("longest_streak")]
        public int LongestStreak { get; set; } = 0;   

        [Column("last_completed_date")]
        public DateOnly? LastCompletedDate { get; set; }

        [ForeignKey(nameof(HabitId))]
        public Habit? Habit { get; set; }
    }
}
