using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Analytics;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsResponseDto> GetAnalyticsAsync(
            long userId,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            startDate ??= today.AddDays(-30);
            endDate ??= today;

            var daily = await GetDailyAnalyticsAsync(userId, today);
            var weekly = await GetWeeklyAnalyticsAsync(userId, GetWeekStart(today));
            var monthly = await GetMonthlyAnalyticsAsync(userId, today.Year, today.Month);
            var categoryWise = await GetCategoryAnalyticsAsync(userId, startDate, endDate);
            var streakTrend = await GetStreakTrendAsync(userId, null, startDate, endDate);

            // Get longest active streak and best streak ever
            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.HabitStreak)
                .ToListAsync();

            var longestActiveStreak = habits
                .Where(h => h.IsActive && h.HabitStreak != null)
                .Select(h => h.HabitStreak!.CurrentStreak)
                .DefaultIfEmpty(0)
                .Max();

            var bestStreakEver = habits
                .Where(h => h.HabitStreak != null)
                .Select(h => h.HabitStreak!.LongestStreak)
                .DefaultIfEmpty(0)
                .Max();

            return new AnalyticsResponseDto
            {
                Daily = daily,
                Weekly = weekly,
                Monthly = monthly,
                CategoryWise = categoryWise,
                StreakTrend = streakTrend,
                LongestActiveStreak = longestActiveStreak,
                BestStreakEver = bestStreakEver
            };
        }

        public async Task<DailyAnalyticsDto> GetDailyAnalyticsAsync(
            long userId,
            DateOnly date)
        {
            var dayOfWeek = date.DayOfWeek switch
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

            // Get habits scheduled for this day
            var scheduledHabits = await _context.Habits
                .Where(h => h.UserId == userId && h.IsActive)
                .Include(h => h.HabitSchedules)
                .Where(h => h.HabitSchedules.Any(s => s.HabitDayOfWeek == dayOfWeek))
                .Select(h => h.HabitId)
                .ToListAsync();

            if (!scheduledHabits.Any())
            {
                return new DailyAnalyticsDto
                {
                    Date = date,
                    TotalHabits = 0,
                    CompletedHabits = 0,
                    SkippedHabits = 0,
                    PendingHabits = 0,
                    CompletionPercentage = 0
                };
            }

            // Get logs for this date
            var logs = await _context.HabitLogs
                .Where(l => scheduledHabits.Contains(l.HabitId) && l.LogDate == date)
                .ToListAsync();

            var completed = logs.Count(l => l.Status == HabitLogStatus.DONE);
            var skipped = logs.Count(l => l.Status == HabitLogStatus.SKIPPED);
            var pending = scheduledHabits.Count - completed - skipped;

            return new DailyAnalyticsDto
            {
                Date = date,
                TotalHabits = scheduledHabits.Count,
                CompletedHabits = completed,
                SkippedHabits = skipped,
                PendingHabits = pending,
                CompletionPercentage = scheduledHabits.Count > 0
                    ? Math.Min((completed * 100.0) / scheduledHabits.Count, 100.0)
                    : 0
            };
        }

        public async Task<WeeklyAnalyticsDto> GetWeeklyAnalyticsAsync(
            long userId,
            DateOnly weekStart)
        {
            var weekEnd = weekStart.AddDays(6);
            var dailyBreakdown = new List<DailyAnalyticsDto>();

            for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
            {
                dailyBreakdown.Add(await GetDailyAnalyticsAsync(userId, date));
            }

            var totalHabits = dailyBreakdown.Sum(d => d.TotalHabits);
            var completedHabits = dailyBreakdown.Sum(d => d.CompletedHabits);
            var skippedHabits = dailyBreakdown.Sum(d => d.SkippedHabits);
            var missedHabits = dailyBreakdown.Sum(d => d.PendingHabits);

            return new WeeklyAnalyticsDto
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                TotalHabits = totalHabits,
                CompletedHabits = completedHabits,
                SkippedHabits = skippedHabits,
                MissedHabits = missedHabits,
                CompletionPercentage = totalHabits > 0
                    ? Math.Min((completedHabits * 100.0) / totalHabits, 100.0)
                    : 0,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<MonthlyAnalyticsDto> GetMonthlyAnalyticsAsync(
            long userId,
            int year,
            int month)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var dailyBreakdown = new List<DailyAnalyticsDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                dailyBreakdown.Add(await GetDailyAnalyticsAsync(userId, date));
            }

            var totalHabits = dailyBreakdown.Sum(d => d.TotalHabits);
            var completedHabits = dailyBreakdown.Sum(d => d.CompletedHabits);
            var skippedHabits = dailyBreakdown.Sum(d => d.SkippedHabits);
            var missedHabits = dailyBreakdown.Sum(d => d.PendingHabits);

            return new MonthlyAnalyticsDto
            {
                Year = year,
                Month = month,
                TotalHabits = totalHabits,
                CompletedHabits = completedHabits,
                SkippedHabits = skippedHabits,
                MissedHabits = missedHabits,
                CompletionPercentage = totalHabits > 0
                    ? Math.Min((completedHabits * 100.0) / totalHabits, 100.0)
                    : 0,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<List<CategoryAnalyticsDto>> GetCategoryAnalyticsAsync(
            long userId,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            startDate ??= today.AddDays(-30);
            endDate ??= today;

            var categories = await _context.HabitCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            var result = new List<CategoryAnalyticsDto>();

            foreach (var category in categories)
            {
                var habits = await _context.Habits
                    .Where(h => h.UserId == userId && h.CategoryId == category.CategoryId && h.IsActive)
                    .Include(h => h.HabitStreak)
                    .ToListAsync();

                if (!habits.Any())
                    continue;

                var habitIds = habits.Select(h => h.HabitId).ToList();

                var logs = await _context.HabitLogs
                    .Where(l => habitIds.Contains(l.HabitId) &&
                                l.LogDate >= startDate.Value &&
                                l.LogDate <= endDate.Value)
                    .ToListAsync();

                var totalHabits = logs.Select(l => l.HabitId).Distinct().Count();
                var completedHabits = logs.Count(l => l.Status == HabitLogStatus.DONE);
                var skippedHabits = logs.Count(l => l.Status == HabitLogStatus.SKIPPED);

                var averageStreak = habits
                    .Where(h => h.HabitStreak != null)
                    .Select(h => h.HabitStreak!.CurrentStreak)
                    .DefaultIfEmpty(0)
                    .Average();

                result.Add(new CategoryAnalyticsDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    TotalHabits = totalHabits,
                    CompletedHabits = completedHabits,
                    SkippedHabits = skippedHabits,
                    CompletionPercentage = totalHabits > 0
                        ? Math.Min((completedHabits * 100.0) / totalHabits, 100.0)
                        : 0,
                    AverageStreak = (int)Math.Round(averageStreak)
                });
            }

            return result;
        }

        public async Task<List<StreakTrendDto>> GetStreakTrendAsync(
            long userId,
            long? habitId = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            startDate ??= today.AddDays(-30);
            endDate ??= today;

            var query = _context.Habits
                .Where(h => h.UserId == userId);

            if (habitId.HasValue)
            {
                query = query.Where(h => h.HabitId == habitId.Value);
            }

            var habits = await query
                .Include(h => h.HabitStreak)
                .ToListAsync();

            var result = new List<StreakTrendDto>();

            foreach (var habit in habits)
            {
                if (habit.HabitStreak == null)
                    continue;

                // Get logs for the date range
                var logs = await _context.HabitLogs
                    .Where(l => l.HabitId == habit.HabitId &&
                                l.LogDate >= startDate.Value &&
                                l.LogDate <= endDate.Value &&
                                l.Status == HabitLogStatus.DONE)
                    .OrderBy(l => l.LogDate)
                    .ToListAsync();

                // Calculate streak for each date
                var currentStreak = 0;
                for (var date = startDate.Value; date <= endDate.Value; date = date.AddDays(1))
                {
                    var log = logs.FirstOrDefault(l => l.LogDate == date);
                    if (log != null && log.Status == HabitLogStatus.DONE)
                    {
                        currentStreak++;
                    }
                    else
                    {
                        if (currentStreak > 0)
                        {
                            result.Add(new StreakTrendDto
                            {
                                Date = date.AddDays(-1),
                                StreakValue = currentStreak,
                                HabitId = habit.HabitId,
                                HabitName = habit.HabitName
                            });
                        }
                        currentStreak = 0;
                    }
                }

                if (currentStreak > 0)
                {
                    result.Add(new StreakTrendDto
                    {
                        Date = endDate.Value,
                        StreakValue = currentStreak,
                        HabitId = habit.HabitId,
                        HabitName = habit.HabitName
                    });
                }
            }

            return result.OrderBy(r => r.Date).ToList();
        }

        private static DateOnly GetWeekStart(DateOnly date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff);
        }
    }
}
