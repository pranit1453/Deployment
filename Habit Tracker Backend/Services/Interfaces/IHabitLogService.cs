using Habit_Tracker_Backend.DTOs.Logs;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IHabitLogService
    {
        Task<HabitLogDto> LogHabitAsync(long userId, long habitId, LogHabitDto dto);

        Task<HabitLogDto?> GetHabitLogAsync(long userId, long habitId, DateOnly date);

        Task DeleteHabitLogAsync(long userId, long habitId, DateOnly date);

        Task<List<HabitLogDto>> GetHabitLogsAsync(long userId, long habitId, DateOnly? startDate, DateOnly? endDate);

        Task<int> BulkDeleteHabitLogsAsync(long userId, long habitId, List<DateOnly> dates);
    }
}

