using Habit_Tracker_Backend.Models.Enums;

namespace Habit_Tracker_Backend.DTOs.Habits
{
    public class HabitResponseDto
    {
        public long HabitId { get; set; }

        public long CategoryId { get; set; }   

        public string CategoryName { get; set; } = string.Empty;

        public string HabitName { get; set; } = string.Empty; 

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public HabitPriority Priority { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CurrentStreak { get; set; }

        public int LongestStreak { get; set; }

        public TimeSpan? ReminderTime { get; set; }

        public bool ReminderEnabled { get; set; }
    }

}
