using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "USER")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard([FromQuery] DateOnly? date)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var userId = long.Parse(claim.Value);
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var result = await _dashboardService.GetDashboardAsync(userId, targetDate);

            return Ok(result);
        }
    }
}
