using Habit_Tracker_Backend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs.Habits
{
    public class HabitDailyStatusDto
    {
        [Required]
        public HabitLogStatus Status { get; set; }
    }
}
