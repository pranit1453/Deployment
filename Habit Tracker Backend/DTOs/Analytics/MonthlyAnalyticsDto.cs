namespace Habit_Tracker_Backend.DTOs.Analytics
{
    public class MonthlyAnalyticsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int SkippedHabits { get; set; }
        public int MissedHabits { get; set; }
        public double CompletionPercentage { get; set; }
        public List<DailyAnalyticsDto> DailyBreakdown { get; set; } = new();
    }
}
