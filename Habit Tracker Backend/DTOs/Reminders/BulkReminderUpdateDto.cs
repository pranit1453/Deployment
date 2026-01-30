namespace Habit_Tracker_Backend.DTOs.Reminders
{
    public class BulkReminderUpdateDto
    {
        public long HabitId { get; set; }
        public bool Enabled { get; set; }
        public string? ReminderTime { get; set; }
    }
}
