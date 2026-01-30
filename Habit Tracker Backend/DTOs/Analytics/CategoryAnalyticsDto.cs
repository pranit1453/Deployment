namespace Habit_Tracker_Backend.DTOs.Analytics
{
    public class CategoryAnalyticsDto
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int SkippedHabits { get; set; }
        public double CompletionPercentage { get; set; }
        public int AverageStreak { get; set; }
    }
}
