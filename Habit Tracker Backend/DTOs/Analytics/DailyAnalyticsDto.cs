namespace Habit_Tracker_Backend.DTOs.Analytics
{
    public class DailyAnalyticsDto
    {
        public DateOnly Date { get; set; }
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int SkippedHabits { get; set; }
        public int PendingHabits { get; set; }
        public double CompletionPercentage { get; set; }
    }
}
