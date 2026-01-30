using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.Models.Enums;
using Habit_Tracker_Backend.Services.Interfaces;
using Habit_Tracker_Backend.DTOs.Reminders;
using Microsoft.EntityFrameworkCore;
using Habit_Tracker_Backend.Exceptions;

namespace Habit_Tracker_Backend.Services.Implementations
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ReminderService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendHabitRemindersAsync()
        {
            //var now = DateTime.UtcNow;
            //var currentTime = TimeOnly.FromDateTime(now);
            //var today = DateOnly.FromDateTime(now);

            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            var currentTime = TimeOnly.FromDateTime(now);
            var today = DateOnly.FromDateTime(now);

            var nowTimeSpan = new TimeSpan(
                currentTime.Hour,
                currentTime.Minute,
                0
            );

            // Get habits with reminders enabled for current time
            //var habitsToRemind = await _context.Habits
            //    .Where(h => h.IsActive)
            //    .Include(h => h.HabitReminder)
            //    .Include(h => h.HabitSchedules)
            //    .Include(h => h.HabitCategory)
            //    .Where(h => h.HabitReminder != null &&
            //                h.HabitReminder.IsEnabled &&
            //                h.HabitReminder.ReminderTime.Hours == currentTime.Hour &&
            //                h.HabitReminder.ReminderTime.Minutes == currentTime.Minute)
            //    .ToListAsync();

            var windowStart = nowTimeSpan.Add(TimeSpan.FromMinutes(-1));
            var windowEnd = nowTimeSpan.Add(TimeSpan.FromMinutes(1));

            var habitsToRemind = await _context.Habits
                .Where(h => h.IsActive)
                .Include(h => h.HabitReminder)
                .Include(h => h.HabitSchedules)
                .Include(h => h.HabitCategory)
                .Where(h =>
                    h.HabitReminder != null &&
                    h.HabitReminder.IsEnabled &&
                    h.HabitReminder.ReminderTime >= windowStart &&
                    h.HabitReminder.ReminderTime <= windowEnd
                )
                .ToListAsync();

            var dayOfWeek = today.DayOfWeek switch
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

            foreach (var habit in habitsToRemind)
            {
                // Check if habit is scheduled for today
                if (!habit.HabitSchedules.Any(s => s.HabitDayOfWeek == dayOfWeek))
                    continue;

                // Check if already completed today
                var todayLog = await _context.HabitLogs
                    .FirstOrDefaultAsync(l => l.HabitId == habit.HabitId && l.LogDate == today);

                if (todayLog != null && todayLog.Status == HabitLogStatus.DONE)
                    continue;

                // Get user
                var user = await _context.Users.FindAsync(habit.UserId);
                if (user == null || !user.IsActive || !user.EmailNotificationsEnabled)
                    continue;

                // Send reminder email
                //await SendReminderEmailAsync(user.Email, user.FirstName, habit.HabitName, habit.HabitCategory.CategoryName);

                try
                {
                    await SendReminderEmailAsync(
                        user.Email,
                        user.FirstName,
                        habit.HabitName,
                        habit.HabitCategory.CategoryName
                    );
                }
                catch (EmailSendFailedException)
                {
                   
                }

            }
        }

        public async Task SendMissedHabitAlertsAsync(long userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var yesterday = today.AddDays(-1);

            var dayOfWeek = yesterday.DayOfWeek switch
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

            // Get habits scheduled for yesterday
            var scheduledHabits = await _context.Habits
                .Where(h => h.UserId == userId && h.IsActive)
                .Include(h => h.HabitSchedules)
                .Include(h => h.HabitCategory)
                .Where(h => h.HabitSchedules.Any(s => s.HabitDayOfWeek == dayOfWeek))
                .ToListAsync();

            var missedHabits = new List<(string HabitName, string CategoryName)>();

            foreach (var habit in scheduledHabits)
            {
                var log = await _context.HabitLogs
                    .FirstOrDefaultAsync(l => l.HabitId == habit.HabitId && l.LogDate == yesterday);

                if (log == null || log.Status != HabitLogStatus.DONE)
                {
                    missedHabits.Add((habit.HabitName, habit.HabitCategory.CategoryName));
                }
            }

            if (missedHabits.Any())
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null && user.IsActive && user.EmailNotificationsEnabled)
                {
                    await SendMissedHabitsEmailAsync(user.Email, user.FirstName, missedHabits, yesterday);
                }
            }
        }

        public async Task SendStreakMilestoneNotificationAsync(long userId, long habitId, int streakCount)
        {
            // Milestones: 7, 14, 30, 50, 100, 200, 365 days
            var milestones = new[] { 7, 14, 30, 50, 100, 200, 365 };

            if (!milestones.Contains(streakCount))
                return;

            var habit = await _context.Habits
                .Include(h => h.HabitCategory)
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                return;

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive || !user.EmailNotificationsEnabled)
                return;

            await SendStreakMilestoneEmailAsync(
                user.Email,
                user.FirstName,
                habit.HabitName,
                streakCount);
        }

        public async Task<bool> ToggleReminderAsync(long userId, long habitId, bool enabled)
        {
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                return false;

            var reminder = await _context.HabitReminders
                .FirstOrDefaultAsync(r => r.HabitId == habitId);

            if (reminder == null)
            {
                if (enabled)
                {
                    // Create default reminder (9 AM)
                    _context.HabitReminders.Add(new Models.Classes.HabitReminder
                    {
                        HabitId = habitId,
                        ReminderTime = new TimeSpan(9, 0, 0),
                        IsEnabled = true
                    });
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }

            reminder.IsEnabled = enabled;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateReminderTimeAsync(long userId, long habitId, TimeSpan reminderTime)
        {
            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == habitId && h.UserId == userId);

            if (habit == null)
                return false;

            var reminder = await _context.HabitReminders
                .FirstOrDefaultAsync(r => r.HabitId == habitId);

            if (reminder == null)
            {
                _context.HabitReminders.Add(new Models.Classes.HabitReminder
                {
                    HabitId = habitId,
                    ReminderTime = reminderTime,
                    IsEnabled = true
                });
            }
            else
            {
                reminder.ReminderTime = reminderTime;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task SendReminderEmailAsync(string email, string firstName, string habitName, string categoryName)
        {
            var subject = $"Reminder: Time for {habitName}";
            var body = $@"
                <h2>Hi {firstName}!</h2>
                <p>This is a reminder that it's time for your habit:</p>
                <h3>{habitName}</h3>
                <p><strong>Category:</strong> {categoryName}</p>
                <p>Don't forget to mark it as done when you complete it!</p>
                <p>Keep up the great work! 💪</p>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        private async Task SendMissedHabitsEmailAsync(
            string email,
            string firstName,
            List<(string HabitName, string CategoryName)> missedHabits,
            DateOnly date)
        {
            var subject = $"You missed some habits yesterday";
            var habitsList = string.Join("", missedHabits.Select(h => $"<li><strong>{h.HabitName}</strong> ({h.CategoryName})</li>"));

            var body = $@"
                <h2>Hi {firstName}!</h2>
                <p>You missed the following habits on {date:MMMM dd, yyyy}:</p>
                <ul>{habitsList}</ul>
                <p>Don't worry - every day is a fresh start! Get back on track today! 🚀</p>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        private async Task SendStreakMilestoneEmailAsync(
            string email,
            string firstName,
            string habitName,
            int streakCount)
        {
            var subject = $"🎉 Amazing! {streakCount} day streak for {habitName}!";
            var body = $@"
                <h2>Congratulations, {firstName}!</h2>
                <p>You've reached an incredible milestone:</p>
                <h1 style='color: #4CAF50;'>{streakCount} DAYS IN A ROW!</h1>
                <p><strong>Habit:</strong> {habitName}</p>
                <p>Your consistency is inspiring! Keep up the amazing work! 🔥</p>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task<ReminderDto?> GetReminderSettingsAsync(long userId, long habitId)
        {
            var reminder = await _context.HabitReminders
                .Join(_context.Habits.Where(h => h.UserId == userId),
                      reminder => reminder.HabitId,
                      habit => habit.HabitId,
                      (reminder, habit) => new { reminder, habit })
                .Join(_context.HabitCategories,
                      combined => combined.habit.CategoryId,
                      category => category.CategoryId,
                      (combined, category) => new { combined.reminder, combined.habit, category })
                .Where(r => r.reminder.HabitId == habitId)
                .Select(r => new ReminderDto
                {
                    HabitId = r.reminder.HabitId,
                    HabitName = r.habit.HabitName,
                    CategoryName = r.category.CategoryName,
                    Enabled = r.reminder.IsEnabled,
                    ReminderTime = TimeOnly.FromTimeSpan(r.reminder.ReminderTime).ToString("HH:mm")
                })
                .FirstOrDefaultAsync();

            return reminder;
        }

        public async Task<List<ReminderDto>> GetAllRemindersAsync(long userId)
        {
            var reminders = await _context.HabitReminders
                .Join(_context.Habits.Where(h => h.UserId == userId),
                      reminder => reminder.HabitId,
                      habit => habit.HabitId,
                      (reminder, habit) => new { reminder, habit })
                .Join(_context.HabitCategories,
                      combined => combined.habit.CategoryId,
                      category => category.CategoryId,
                      (combined, category) => new { combined.reminder, combined.habit, category })
                .Join(_context.HabitSchedules,
                      combined => combined.habit.HabitId,
                      schedule => schedule.HabitId,
                      (combined, schedule) => new { combined.reminder, combined.habit, combined.category, schedule })
                .Select(r => new ReminderDto
                {
                    HabitId = r.reminder.HabitId,
                    HabitName = r.habit.HabitName,
                    CategoryName = r.category.CategoryName,
                    Enabled = r.reminder.IsEnabled,
                    ReminderTime = TimeOnly.FromTimeSpan(r.reminder.ReminderTime).ToString("HH:mm")
                })
                .ToListAsync();

            // Add schedule days to each reminder
            var result = new List<ReminderDto>();
            foreach (var reminder in reminders)
            {
                var scheduleDays = await _context.HabitSchedules
                    .Where(s => s.HabitId == reminder.HabitId)
                    .Select(s => s.HabitDayOfWeek.ToString())
                    .ToListAsync();
                
                reminder.ScheduleDays = scheduleDays;
                result.Add(reminder);
            }

            return result;
        }

        public async Task<bool> BulkUpdateRemindersAsync(long userId, List<BulkReminderUpdateDto> reminderUpdates)
        {
            var updatedCount = 0;
            
            foreach (var update in reminderUpdates)
            {
                var reminder = await _context.HabitReminders
                    .Join(_context.Habits.Where(h => h.UserId == userId),
                          reminder => reminder.HabitId,
                          habit => habit.HabitId,
                          (reminder, habit) => new { reminder, habit })
                    .Where(r => r.reminder.HabitId == update.HabitId)
                    .Select(r => r.reminder)
                    .FirstOrDefaultAsync();

                if (reminder != null)
                {
                    reminder.IsEnabled = update.Enabled;

                    if (!string.IsNullOrEmpty(update.ReminderTime) && 
                        TimeSpan.TryParse(update.ReminderTime, out var time))
                    {
                        reminder.ReminderTime = time;
                    }

                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return updatedCount > 0;
        }
    }
}
