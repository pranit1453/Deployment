namespace Habit_Tracker_Backend.DTOs.Analytics
{
    public class WeeklyAnalyticsDto
    {
        public DateOnly WeekStart { get; set; }
        public DateOnly WeekEnd { get; set; }
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int SkippedHabits { get; set; }
        public int MissedHabits { get; set; }
        public double CompletionPercentage { get; set; }
        public List<DailyAnalyticsDto> DailyBreakdown { get; set; } = new();
    }
}
