using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Admin;
using Habit_Tracker_Backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users/count")]
        public async Task<IActionResult> GetUserCount()
        {
            var count = await _context.Users.CountAsync();
            return Ok(new { totalCount = count });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.UserId)
                .Select(u => new AdminUserItemDto
                {
                    UserId = u.UserId,
                    FullName = u.MiddleName != null
                        ? $"{u.FirstName} {u.MiddleName} {u.LastName}"
                        : $"{u.FirstName} {u.LastName}"
                })
                .ToListAsync();

            return Ok(new AdminUserListDto
            {
                TotalCount = users.Count,
                Users = users
            });
        }

        [HttpGet("users/detailed")]
        public async Task<IActionResult> GetUsersDetailed()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.UserId)
                .Select(u => new AdminUserDetailDto
                {
                    UserId = u.UserId,
                    FullName = u.MiddleName != null
                        ? $"{u.FirstName} {u.MiddleName} {u.LastName}"
                        : $"{u.FirstName} {u.LastName}",
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    IsEmailVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt,
                    TotalHabits = _context.Habits.Count(h => h.UserId == u.UserId),
                    ActiveHabits = _context.Habits.Count(h => h.UserId == u.UserId && h.IsActive)
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalHabits = await _context.Habits.CountAsync();
            var activeHabits = await _context.Habits.CountAsync(h => h.IsActive);
            var totalHabitLogs = await _context.HabitLogs.CountAsync();
            var totalCategories = await _context.HabitCategories.CountAsync();
            var totalFeedback = await _context.Feedback.CountAsync();

            var averageHabitsPerUser = totalUsers > 0 ? (double)totalHabits / totalUsers : 0;

            // Calculate average completion rate
            var totalScheduledHabits = await _context.HabitLogs
                .GroupBy(l => new { l.HabitId, l.LogDate })
                .CountAsync();

            var completedHabits = await _context.HabitLogs
                .CountAsync(l => l.Status == HabitLogStatus.DONE);

            var averageCompletionRate = totalScheduledHabits > 0
                ? Math.Min((completedHabits * 100.0) / totalScheduledHabits, 100.0)
                : 0;

            var statistics = new AdminStatisticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalHabits = totalHabits,
                ActiveHabits = activeHabits,
                TotalHabitLogs = totalHabitLogs,
                TotalCategories = totalCategories,
                TotalFeedback = totalFeedback,
                AverageHabitsPerUser = Math.Round(averageHabitsPerUser, 2),
                AverageCompletionRate = Math.Round(averageCompletionRate, 2)
            };

            return Ok(statistics);
        }

        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback()
        {
            var feedback = await _context.Feedback
                .AsNoTracking()
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new
                {
                    feedbackId = f.FeedbackId,
                    name = f.Name,
                    email = f.Email,
                    rating = f.Rating,
                    message = f.Message,
                    createdAt = f.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                feedback = feedback
            });
        }
    }
}
