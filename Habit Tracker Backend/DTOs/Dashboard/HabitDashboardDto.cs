namespace Habit_Tracker_Backend.DTOs.Dashboard
{
    public class HabitDashboardDto
    {
        public long HabitId { get; set; }

        public string HabitName { get; set; } = null!;

        public long CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Status { get; set; } = "PENDING";

        public bool IsScheduledToday { get; set; }

        public int CurrentStreak { get; set; }

        public int LongestStreak { get; set; }

        public bool IsOngoing { get; set; }

        public double CompletionRate { get; set; }
    }

}
