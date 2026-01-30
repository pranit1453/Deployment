using Habit_Tracker_Backend.DTOs.Dashboard;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardAsync(
            long userId,
            DateOnly targetDate
        );
    }

}
