using Habit_Tracker_Backend.DTOs.Analytics;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsResponseDto> GetAnalyticsAsync(
            long userId,
            DateOnly? startDate = null,
            DateOnly? endDate = null);

        Task<DailyAnalyticsDto> GetDailyAnalyticsAsync(
            long userId,
            DateOnly date);

        Task<WeeklyAnalyticsDto> GetWeeklyAnalyticsAsync(
            long userId,
            DateOnly weekStart);

        Task<MonthlyAnalyticsDto> GetMonthlyAnalyticsAsync(
            long userId,
            int year,
            int month);

        Task<List<CategoryAnalyticsDto>> GetCategoryAnalyticsAsync(
            long userId,
            DateOnly? startDate = null,
            DateOnly? endDate = null);

        Task<List<StreakTrendDto>> GetStreakTrendAsync(
            long userId,
            long? habitId = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null);
    }
}
