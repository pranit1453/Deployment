using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Dashboard;
using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(
            long userId,
            DateOnly targetDate)
        {
            // ✅ Convert DateOnly → HabitDayOfWeek
            var todayHabitDay = targetDate.DayOfWeek switch
            {
                DayOfWeek.Monday => HabitDayOfWeek.MON,
                DayOfWeek.Tuesday => HabitDayOfWeek.TUE,
                DayOfWeek.Wednesday => HabitDayOfWeek.WED,
                DayOfWeek.Thursday => HabitDayOfWeek.THU,
                DayOfWeek.Friday => HabitDayOfWeek.FRI,
                DayOfWeek.Saturday => HabitDayOfWeek.SAT,
                DayOfWeek.Sunday => HabitDayOfWeek.SUN,
                _ => throw new ArgumentOutOfRangeException()
            };

            //  Fetch all habits (active and inactive) with schedules + streaks + category
            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.HabitSchedules)
                .Include(h => h.HabitStreak)
                .Include(h => h.HabitCategory)
                .ToListAsync();

            var habitIds = habits.Select(h => h.HabitId).ToList();

            //  Fetch today's logs
            var logs = await _context.HabitLogs
                .Where(l => l.LogDate == targetDate && habitIds.Contains(l.HabitId))
                .ToDictionaryAsync(l => l.HabitId, l => l.Status);

            var dashboardHabits = new List<HabitDashboardDto>();

            //  Build habit cards
            foreach (var habit in habits)
            {
                bool isScheduledToday = habit.HabitSchedules
                    .Any(s => s.HabitDayOfWeek == todayHabitDay);

                string status;

                if (!isScheduledToday)
                {
                    status = "NOT_SCHEDULED";
                }
                else if (logs.TryGetValue(habit.HabitId, out var logStatus))
                {
                    status = logStatus.ToString(); // DONE / SKIPPED
                }
                else
                {
                    status = HabitLogStatus.PENDING.ToString();
                }

                // Determine if habit is ongoing or past
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                bool isOngoing = habit.IsActive && 
                                (habit.EndDate == null || DateOnly.FromDateTime(habit.EndDate.Value) >= today);

                dashboardHabits.Add(new HabitDashboardDto
                {
                    HabitId = habit.HabitId,
                    HabitName = habit.HabitName,
                    CategoryId = habit.CategoryId,
                    CategoryName = habit.HabitCategory.CategoryName,
                    Description = habit.Description,
                    StartDate = habit.StartDate,
                    EndDate = habit.EndDate,
                    Status = status,
                    IsScheduledToday = isScheduledToday,
                    CurrentStreak = habit.HabitStreak?.CurrentStreak ?? 0,
                    LongestStreak = habit.HabitStreak?.LongestStreak ?? 0,
                    IsOngoing = isOngoing
                });
            }

            // Separate ongoing and past habits
            var ongoingHabits = dashboardHabits.Where(h => h.IsOngoing).ToList();
            var pastHabits = dashboardHabits.Where(h => !h.IsOngoing).ToList();

            // Summary (ONLY ongoing scheduled habits)
            var scheduledOngoingHabits = ongoingHabits
                .Where(h => h.IsScheduledToday)
                .ToList();

            var summary = new DashboardSummaryDto
            {
                Date = targetDate,
                TotalHabits = scheduledOngoingHabits.Count,
                DoneCount = scheduledOngoingHabits.Count(h => h.Status == "DONE"),
                SkippedCount = scheduledOngoingHabits.Count(h => h.Status == "SKIPPED"),
                PendingCount = scheduledOngoingHabits.Count(h => h.Status == "PENDING"),
                CompletionPercentage =
                    scheduledOngoingHabits.Count == 0
                        ? 0
                        : Math.Min(
                            (scheduledOngoingHabits.Count(h => h.Status == "DONE") * 100.0)
                            / scheduledOngoingHabits.Count,
                            100.0
                          )
            };

            return new DashboardResponseDto
            {
                Summary = summary,
                OngoingHabits = ongoingHabits,
                PastHabits = pastHabits
            };
        }

        private async Task<double> CalculateHabitCompletionRateAsync(Habit habit, List<HabitLog> habitLogs)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = DateOnly.FromDateTime(habit.StartDate);
            var endDate = habit.EndDate.HasValue ? DateOnly.FromDateTime(habit.EndDate.Value) : today;
            var totalDays = (endDate.DayNumber - startDate.DayNumber) + 1;
            
            if (totalDays <= 0) return 0;

            var completedDays = habitLogs
                .Where(log => log.Status == HabitLogStatus.DONE && 
                              log.LogDate >= startDate && 
                              log.LogDate <= endDate)
                .Select(log => log.LogDate)
                .Distinct()
                .Count();

            var completionRate = (double)completedDays / totalDays * 100;
            return Math.Min(completionRate, 100.0);
        }
    }
}
