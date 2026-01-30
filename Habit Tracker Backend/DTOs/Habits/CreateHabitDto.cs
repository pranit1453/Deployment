using System.ComponentModel.DataAnnotations;
using Habit_Tracker_Backend.Models.Enums;

namespace Habit_Tracker_Backend.DTOs.Habits
{
    public class CreateHabitDto
    {
        [Required]
        public long CategoryId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string HabitName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [CustomValidation(typeof(CreateHabitDto), nameof(ValidateStartDate))]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one schedule day is required")]
        public List<string> ScheduleDays { get; set; } = new();

        public HabitPriority Priority { get; set; } = HabitPriority.MEDIUM;

        public TimeSpan? ReminderTime { get; set; }

        public static ValidationResult? ValidateStartDate(DateTime startDate, ValidationContext context)
        {
            var today = DateTime.Today;
            if (startDate.Date < today)
            {
                return new ValidationResult("Start date cannot be in the past. Please select today or a future date.");
            }
            return ValidationResult.Success;
        }
    }
}
