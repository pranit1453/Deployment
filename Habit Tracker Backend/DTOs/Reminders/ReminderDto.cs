namespace Habit_Tracker_Backend.DTOs.Reminders
{
    public class ReminderDto
    {
        public long HabitId { get; set; }
        public string HabitName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string ReminderTime { get; set; } = string.Empty;
        public List<string> ScheduleDays { get; set; } = new();
    }
}
