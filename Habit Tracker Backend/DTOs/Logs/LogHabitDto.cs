using Habit_Tracker_Backend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs.Logs
{
    public class LogHabitDto
    {
        [Required]
        public HabitLogStatus Status { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public DateOnly? LogDate { get; set; }
    }
}
