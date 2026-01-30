using Habit_Tracker_Backend.DTOs.Habits;
using Habit_Tracker_Backend.Models.Enums;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IHabitService
    {
        Task<long> CreateHabitAsync(long userId, CreateHabitDto dto);
        Task<List<HabitResponseDto>> GetHabitsAsync(long userId);
        Task<HabitResponseDto?> GetHabitByIdAsync(long userId, long habitId);
        Task UpdateHabitAsync(long userId, long habitId, CreateHabitDto dto);
        Task ToggleHabitStatusAsync(long userId, long habitId);
        Task DeleteHabitAsync(long userId, long habitId);
        Task SetHabitDailyStatusAsync(long userId,long habitId,DateOnly date,HabitLogStatus status);
    }
}
