using Habit_Tracker_Backend.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Habit_Tracker_Backend.Models.Classes
{
    [Table("habit_schedule")]
    public class HabitSchedule
    {
        [Key]
        [Column("schedule_id")]
        public long ScheduleId { get; set; }

        [Required]
        [Column("habit_id")]
        public long HabitId { get; set; }

        [Required]
        [Column("day_of_week")]
        public HabitDayOfWeek HabitDayOfWeek { get; set; }
        //[ForeignKey(nameof(HabitId))]
        //public Habit? Habit { get; set; } 
    }
}
