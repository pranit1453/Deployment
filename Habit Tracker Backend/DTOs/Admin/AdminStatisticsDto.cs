namespace Habit_Tracker_Backend.DTOs.Admin
{
    public class AdminStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalHabits { get; set; }
        public int ActiveHabits { get; set; }
        public int TotalHabitLogs { get; set; }
        public int TotalCategories { get; set; }
        public int TotalFeedback { get; set; }
        public double AverageHabitsPerUser { get; set; }
        public double AverageCompletionRate { get; set; }
    }
}
