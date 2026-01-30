using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Logs;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class HabitLogService : IHabitLogService
    {
        private readonly AppDbContext _context;

        public HabitLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HabitLogDto> LogHabitAsync(long userId, long habitId, LogHabitDto dto)
        {
            // Default to today if date not provided
            var logDate = dto.LogDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // Verify habit exists and belongs to user
            var habit = await _context.Habits
                .Include(h => h.HabitSchedules)
                .Include(h => h.HabitStreak)
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            // Check if habit is scheduled for this day
            var dayOfWeek = ConvertToDayOfWeek(logDate.DayOfWeek);
            var isScheduled = habit.HabitSchedules.Any(s => s.HabitDayOfWeek == dayOfWeek);

            if (!isScheduled)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var message = logDate == today
                    ? "This habit is not scheduled for today. You can only log it on its scheduled days."
                    : $"This habit is not scheduled for {logDate.DayOfWeek}. You can only log it on its scheduled days.";
                throw new BadRequestException(message);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check for existing log
                var existingLog = await _context.HabitLogs
                    .FirstOrDefaultAsync(l => l.HabitId == habitId && l.LogDate == logDate);

                if (existingLog != null)
                {
                    // Update existing log
                    var oldStatus = existingLog.Status;
                    existingLog.Status = dto.Status;
                    existingLog.Remarks = dto.Remarks;

                    // Recalculate streak if status changed
                    if (oldStatus != dto.Status)
                    {
                        await UpdateStreakAsync(habit, logDate);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new HabitLogDto
                    {
                        LogId = existingLog.LogId,
                        HabitId = existingLog.HabitId,
                        LogDate = existingLog.LogDate,
                        Status = existingLog.Status,
                        Remarks = existingLog.Remarks
                    };
                }
                else
                {
                    // Create new log
                    var newLog = new HabitLog
                    {
                        HabitId = habitId,
                        LogDate = logDate,
                        Status = dto.Status,
                        Remarks = dto.Remarks
                    };

                    _context.HabitLogs.Add(newLog);
                    await _context.SaveChangesAsync();

                    // Update streak
                    await UpdateStreakAsync(habit, logDate);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new HabitLogDto
                    {
                        LogId = newLog.LogId,
                        HabitId = newLog.HabitId,
                        LogDate = newLog.LogDate,
                        Status = newLog.Status,
                        Remarks = newLog.Remarks
                    };
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<HabitLogDto?> GetHabitLogAsync(long userId, long habitId, DateOnly date)
        {
            // Verify habit belongs to user
            var habitExists = await _context.Habits
                .AnyAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (!habitExists)
                throw new HabitNotFoundException();

            var log = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == habitId && l.LogDate == date);

            if (log == null)
                return null;

            return new HabitLogDto
            {
                LogId = log.LogId,
                HabitId = log.HabitId,
                LogDate = log.LogDate,
                Status = log.Status,
                Remarks = log.Remarks
            };
        }

        public async Task DeleteHabitLogAsync(long userId, long habitId, DateOnly date)
        {
            // Verify habit belongs to user
            var habit = await _context.Habits
                .Include(h => h.HabitStreak)
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            var log = await _context.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == habitId && l.LogDate == date);

            if (log == null)
                throw new NotFoundException("Habit log not found");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.HabitLogs.Remove(log);
                await _context.SaveChangesAsync();

                // Recalculate streak after deletion
                await UpdateStreakAsync(habit, date);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<HabitLogDto>> GetHabitLogsAsync(long userId, long habitId, DateOnly? startDate, DateOnly? endDate)
        {
            // Verify habit belongs to user
            var habitExists = await _context.Habits
                .AnyAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (!habitExists)
                throw new HabitNotFoundException();

            var query = _context.HabitLogs
                .Where(l => l.HabitId == habitId);

            if (startDate.HasValue)
                query = query.Where(l => l.LogDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.LogDate <= endDate.Value);

            var logs = await query
                .OrderBy(l => l.LogDate)
                .Select(l => new HabitLogDto
                {
                    LogId = l.LogId,
                    HabitId = l.HabitId,
                    LogDate = l.LogDate,
                    Status = l.Status,
                    Remarks = l.Remarks
                })
                .ToListAsync();

            return logs;
        }

        public async Task<int> BulkDeleteHabitLogsAsync(long userId, long habitId, List<DateOnly> dates)
        {
            // Verify habit belongs to user
            var habit = await _context.Habits
                .Include(h => h.HabitStreak)
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            var logsToDelete = await _context.HabitLogs
                .Where(l => l.HabitId == habitId && dates.Contains(l.LogDate))
                .ToListAsync();

            if (!logsToDelete.Any())
                return 0;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.HabitLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                // Recalculate streak for each deleted date
                foreach (var date in dates.OrderByDescending(d => d))
                {
                    await UpdateStreakAsync(habit, date);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return logsToDelete.Count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpdateStreakAsync(Habit habit, DateOnly changeDate)
        {
            // Get all DONE logs for this habit, ordered by date
            var doneLogs = await _context.HabitLogs
                .Where(l => l.HabitId == habit.HabitId && l.Status == HabitLogStatus.DONE)
                .OrderByDescending(l => l.LogDate)
                .Select(l => l.LogDate)
                .ToListAsync();

            var streak = habit.HabitStreak;
            if (streak == null)
            {
                // Create streak record if it doesn't exist
                streak = new HabitStreak
                {
                    HabitId = habit.HabitId,
                    CurrentStreak = 0,
                    LongestStreak = 0
                };
                _context.HabitStreaks.Add(streak);
            }

            if (!doneLogs.Any())
            {
                // No completed logs
                streak.CurrentStreak = 0;
                streak.LastCompletedDate = null;
                return;
            }

            // Get scheduled days for this habit
            var scheduledDays = await _context.HabitSchedules
                .Where(s => s.HabitId == habit.HabitId)
                .Select(s => s.HabitDayOfWeek)
                .ToListAsync();

            // Calculate current streak (consecutive scheduled days completed)
            int currentStreak = 0;
            DateOnly checkDate = doneLogs.First(); // Most recent done log
            streak.LastCompletedDate = checkDate;

            foreach (var logDate in doneLogs)
            {
                if (logDate == checkDate)
                {
                    currentStreak++;
                    checkDate = GetPreviousScheduledDay(checkDate, scheduledDays);
                }
                else
                {
                    // Gap found, streak broken
                    break;
                }
            }

            streak.CurrentStreak = currentStreak;

            // Calculate longest streak
            int longestStreak = 0;
            int tempStreak = 0;
            DateOnly? previousExpectedDate = null;

            foreach (var logDate in doneLogs.OrderBy(d => d))
            {
                if (previousExpectedDate == null)
                {
                    tempStreak = 1;
                }
                else
                {
                    var nextExpected = GetNextScheduledDay(previousExpectedDate.Value, scheduledDays);
                    if (logDate == nextExpected)
                    {
                        tempStreak++;
                    }
                    else
                    {
                        longestStreak = Math.Max(longestStreak, tempStreak);
                        tempStreak = 1;
                    }
                }
                previousExpectedDate = logDate;
            }
            longestStreak = Math.Max(longestStreak, tempStreak);
            streak.LongestStreak = Math.Max(streak.LongestStreak, longestStreak);
        }

        private DateOnly GetPreviousScheduledDay(DateOnly fromDate, List<HabitDayOfWeek> scheduledDays)
        {
            var checkDate = fromDate.AddDays(-1);
            for (int i = 0; i < 7; i++)
            {
                var dayOfWeek = ConvertToDayOfWeek(checkDate.DayOfWeek);
                if (scheduledDays.Contains(dayOfWeek))
                {
                    return checkDate;
                }
                checkDate = checkDate.AddDays(-1);
            }
            return fromDate.AddDays(-1); // Fallback
        }

        private DateOnly GetNextScheduledDay(DateOnly fromDate, List<HabitDayOfWeek> scheduledDays)
        {
            var checkDate = fromDate.AddDays(1);
            for (int i = 0; i < 7; i++)
            {
                var dayOfWeek = ConvertToDayOfWeek(checkDate.DayOfWeek);
                if (scheduledDays.Contains(dayOfWeek))
                {
                    return checkDate;
                }
                checkDate = checkDate.AddDays(1);
            }
            return fromDate.AddDays(1); // Fallback
        }

        private HabitDayOfWeek ConvertToDayOfWeek(DayOfWeek day)
        {
            return day switch
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
        }
    }
}
