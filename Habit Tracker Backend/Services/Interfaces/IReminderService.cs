using Habit_Tracker_Backend.DTOs.Reminders;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IReminderService
    {
        Task SendHabitRemindersAsync();
        Task SendMissedHabitAlertsAsync(long userId);
        Task SendStreakMilestoneNotificationAsync(long userId, long habitId, int streakCount);
        Task<bool> ToggleReminderAsync(long userId, long habitId, bool enabled);
        Task<bool> UpdateReminderTimeAsync(long userId, long habitId, TimeSpan reminderTime);
        Task<ReminderDto?> GetReminderSettingsAsync(long userId, long habitId);
        Task<List<ReminderDto>> GetAllRemindersAsync(long userId);
        Task<bool> BulkUpdateRemindersAsync(long userId, List<BulkReminderUpdateDto> reminderUpdates);
    }
}
