using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("habit_reminder")]
    public class HabitReminder
    {
        [Key]
        [Column("reminder_id")]
        public long ReminderId { get; set; }

        [Required]
        [Column("habit_id")]
        public long HabitId { get; set; }

        [Required]
        [Column("reminder_time")]
        public TimeSpan ReminderTime { get; set; }

        [Required]
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        //[ForeignKey(nameof(HabitId))]
        //public Habit? Habit { get; set; }   
    }
}
