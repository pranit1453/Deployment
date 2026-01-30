namespace Habit_Tracker_Backend.DTOs.Analytics
{
    public class AnalyticsResponseDto
    {
        public DailyAnalyticsDto? Daily { get; set; }
        public WeeklyAnalyticsDto? Weekly { get; set; }
        public MonthlyAnalyticsDto? Monthly { get; set; }
        public List<CategoryAnalyticsDto> CategoryWise { get; set; } = new();
        public List<StreakTrendDto> StreakTrend { get; set; } = new();
        public int LongestActiveStreak { get; set; }
        public int BestStreakEver { get; set; }
    }
}
