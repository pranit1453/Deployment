namespace Habit_Tracker_Backend.DTOs.Analytics
{
    public class StreakTrendDto
    {
        public DateOnly Date { get; set; }
        public int StreakValue { get; set; }
        public long HabitId { get; set; }
        public string HabitName { get; set; } = string.Empty;
    }
}
