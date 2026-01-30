using Habit_Tracker_Backend.DTOs.Analytics;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize(Roles = "USER")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return long.Parse(claim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var result = await _analyticsService.GetAnalyticsAsync(
                GetUserId(),
                startDate,
                endDate);

            return Ok(result);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyAnalytics([FromQuery] DateOnly? date)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await _analyticsService.GetDailyAnalyticsAsync(GetUserId(), targetDate);
            return Ok(result);
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyAnalytics([FromQuery] DateOnly? weekStart)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var start = weekStart ?? GetWeekStart(today);
            var result = await _analyticsService.GetWeeklyAnalyticsAsync(GetUserId(), start);
            return Ok(result);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyAnalytics(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await _analyticsService.GetMonthlyAnalyticsAsync(
                GetUserId(),
                year ?? today.Year,
                month ?? today.Month);
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryAnalytics(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var result = await _analyticsService.GetCategoryAnalyticsAsync(
                GetUserId(),
                startDate,
                endDate);
            return Ok(result);
        }

        [HttpGet("streak-trend")]
        public async Task<IActionResult> GetStreakTrend(
            [FromQuery] long? habitId,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var result = await _analyticsService.GetStreakTrendAsync(
                GetUserId(),
                habitId,
                startDate,
                endDate);
            return Ok(result);
        }

        private static DateOnly GetWeekStart(DateOnly date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff);
        }
    }
}
