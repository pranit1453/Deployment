using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Habits;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class HabitService : IHabitService
    {
        private readonly AppDbContext _context;

        public HabitService(AppDbContext context)
        {
            _context = context;
        }

        // -------------------- CREATE --------------------
        public async Task<long> CreateHabitAsync(long userId, CreateHabitDto dto)
        {
            if (dto.ScheduleDays == null || !dto.ScheduleDays.Any())
                throw new ScheduleRequiredException();

            await using var tx = await _context.Database.BeginTransactionAsync();

            var categoryExists = await _context.HabitCategories
                .AnyAsync(c => c.CategoryId == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                throw new InvalidCategoryException();

            var habit = new Habit
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                HabitName = dto.HabitName.Trim(),
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Priority = dto.Priority,
                IsActive = true
            };

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            var schedules = new List<HabitSchedule>();

            foreach (var day in dto.ScheduleDays)
            {
                if (!Enum.TryParse<HabitDayOfWeek>(day, true, out var parsedDay))
                    throw new InvalidScheduleException(day);

                schedules.Add(new HabitSchedule
                {
                    HabitId = habit.HabitId,
                    HabitDayOfWeek = parsedDay
                });
            }

            _context.HabitSchedules.AddRange(schedules);

            _context.HabitStreaks.Add(new HabitStreak
            {
                HabitId = habit.HabitId,
                CurrentStreak = 0,
                LongestStreak = 0
            });

            // Add reminder if provided
            if (dto.ReminderTime.HasValue)
            {
                _context.HabitReminders.Add(new HabitReminder
                {
                    HabitId = habit.HabitId,
                    ReminderTime = dto.ReminderTime.Value,
                    IsEnabled = true
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return habit.HabitId;
        }

        // -------------------- GET ALL --------------------
        public async Task<List<HabitResponseDto>> GetHabitsAsync(long userId)
        {
            return await _context.Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.HabitCategory)
                .Include(h => h.HabitStreak)
                .Include(h => h.HabitReminder)
                .Select(h => new HabitResponseDto
                {
                    HabitId = h.HabitId,
                    CategoryId = h.CategoryId,
                    CategoryName = h.HabitCategory.CategoryName,
                    HabitName = h.HabitName,
                    Description = h.Description,
                    StartDate = h.StartDate,
                    EndDate = h.EndDate,
                    Priority = h.Priority,
                    IsActive = h.IsActive,
                    CreatedAt = h.CreatedAt,
                    CurrentStreak = h.HabitStreak != null ? h.HabitStreak.CurrentStreak : 0,
                    LongestStreak = h.HabitStreak != null ? h.HabitStreak.LongestStreak : 0,
                    ReminderTime = h.HabitReminder != null ? h.HabitReminder.ReminderTime : null,
                    ReminderEnabled = h.HabitReminder != null && h.HabitReminder.IsEnabled
                })
                .ToListAsync();
        }

        // -------------------- GET BY ID --------------------
        public async Task<HabitResponseDto?> GetHabitByIdAsync(long userId, long habitId)
        {
            return await _context.Habits
                .Where(h => h.HabitId == habitId && h.UserId == userId)
                .Include(h => h.HabitCategory)
                .Include(h => h.HabitStreak)
                .Include(h => h.HabitReminder)
                .Select(h => new HabitResponseDto
                {
                    HabitId = h.HabitId,
                    CategoryId = h.CategoryId,
                    CategoryName = h.HabitCategory.CategoryName,
                    HabitName = h.HabitName,
                    Description = h.Description,
                    StartDate = h.StartDate,
                    EndDate = h.EndDate,
                    Priority = h.Priority,
                    IsActive = h.IsActive,
                    CreatedAt = h.CreatedAt,
                    CurrentStreak = h.HabitStreak != null ? h.HabitStreak.CurrentStreak : 0,
                    LongestStreak = h.HabitStreak != null ? h.HabitStreak.LongestStreak : 0,
                    ReminderTime = h.HabitReminder != null ? h.HabitReminder.ReminderTime : null,
                    ReminderEnabled = h.HabitReminder != null && h.HabitReminder.IsEnabled
                })
                .FirstOrDefaultAsync();
        }

        // -------------------- UPDATE --------------------
        public async Task UpdateHabitAsync(long userId, long habitId, CreateHabitDto dto)
        {
            if (dto.ScheduleDays == null || !dto.ScheduleDays.Any())
                throw new ScheduleRequiredException();

            await using var tx = await _context.Database.BeginTransactionAsync();

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            var duplicateExists = await _context.Habits.AnyAsync(h =>
                h.UserId == userId &&
                h.HabitName == dto.HabitName.Trim() &&
                h.HabitId != habitId);

            if (duplicateExists)
                throw new DuplicateHabitException();

            habit.CategoryId = dto.CategoryId;
            habit.HabitName = dto.HabitName.Trim();
            habit.Description = dto.Description;
            habit.StartDate = dto.StartDate;
            habit.EndDate = dto.EndDate;
            habit.Priority = dto.Priority;

            var oldSchedules = await _context.HabitSchedules
                .Where(s => s.HabitId == habitId)
                .ToListAsync();

            _context.HabitSchedules.RemoveRange(oldSchedules);

            var newSchedules = new List<HabitSchedule>();

            foreach (var day in dto.ScheduleDays)
            {
                if (!Enum.TryParse<HabitDayOfWeek>(day, true, out var parsedDay))
                    throw new InvalidScheduleException(day);

                newSchedules.Add(new HabitSchedule
                {
                    HabitId = habitId,
                    HabitDayOfWeek = parsedDay
                });
            }

            _context.HabitSchedules.AddRange(newSchedules);

            // Update or create reminder
            var existingReminder = await _context.HabitReminders
                .FirstOrDefaultAsync(r => r.HabitId == habitId);

            if (dto.ReminderTime.HasValue)
            {
                if (existingReminder != null)
                {
                    existingReminder.ReminderTime = dto.ReminderTime.Value;
                    existingReminder.IsEnabled = true;
                }
                else
                {
                    _context.HabitReminders.Add(new HabitReminder
                    {
                        HabitId = habitId,
                        ReminderTime = dto.ReminderTime.Value,
                        IsEnabled = true
                    });
                }
            }
            else if (existingReminder != null)
            {
                existingReminder.IsEnabled = false;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        // -------------------- TOGGLE STATUS --------------------
        public async Task ToggleHabitStatusAsync(long userId, long habitId)
        {
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            habit.IsActive = !habit.IsActive;
            await _context.SaveChangesAsync();
        }

        // -------------------- DELETE --------------------
        public async Task DeleteHabitAsync(long userId, long habitId)
        {
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            // Soft delete - just mark as inactive
            habit.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task SetHabitDailyStatusAsync(
    long userId,
    long habitId,
    DateOnly date,
    HabitLogStatus status)
        {
            // 1️ Validate habit ownership
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                throw new HabitNotFoundException();

            // 2️ Find today's log (by HabitId only)
            var log = await _context.HabitLogs
                .FirstOrDefaultAsync(l =>
                    l.HabitId == habitId &&
                    l.LogDate == date);

            // 3️ Insert or update
            if (log == null)
            {
                log = new HabitLog
                {
                    HabitId = habitId,
                    LogDate = date,
                    Status = status
                };
                _context.HabitLogs.Add(log);
            }
            else
            {
                log.Status = status; // DONE ↔ SKIPPED
            }

            await _context.SaveChangesAsync();
        }

    }
}
