namespace Habit_Tracker_Backend.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public DashboardSummaryDto Summary { get; set; } = null!;

        public List<HabitDashboardDto> OngoingHabits { get; set; } = new();

        public List<HabitDashboardDto> PastHabits { get; set; } = new();
    }

}
