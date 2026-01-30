namespace Habit_Tracker_Backend.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public DateOnly Date { get; set; } 

        public int TotalHabits { get; set; }

        public int DoneCount { get; set; }

        public int SkippedCount { get; set; }

        public int PendingCount { get; set; }

        public double CompletionPercentage { get; set; }
    }

}
