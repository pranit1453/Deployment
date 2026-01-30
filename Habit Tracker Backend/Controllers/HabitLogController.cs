using Habit_Tracker_Backend.DTOs.Logs;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/habits/{habitId:long}/logs")]
    [Authorize(Roles = "USER")]
    public class HabitLogController : ControllerBase
    {
        private readonly IHabitLogService _habitLogService;

        public HabitLogController(IHabitLogService habitLogService)
        {
            _habitLogService = habitLogService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return long.Parse(claim.Value);
        }

        
        [HttpPost]
        public async Task<IActionResult> LogHabit(long habitId, [FromBody] LogHabitDto dto)
        {
            var result = await _habitLogService.LogHabitAsync(GetUserId(), habitId, dto);
            
            return Ok(new
            {
                success = true,
                message = "Habit logged successfully",
                data = result
            });
        }

        
        [HttpGet("{date}")]
        public async Task<IActionResult> GetHabitLog(long habitId, DateOnly date)
        {
            var result = await _habitLogService.GetHabitLogAsync(GetUserId(), habitId, date);

            if (result == null)
                return NotFound(new { message = "Habit log not found" });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetHabitLogs(
            long habitId, 
            [FromQuery] DateOnly? startDate, 
            [FromQuery] DateOnly? endDate)
        {
            var result = await _habitLogService.GetHabitLogsAsync(GetUserId(), habitId, startDate, endDate);
            return Ok(result);
        }
       
        [HttpDelete("{date}")]
        public async Task<IActionResult> DeleteHabitLog(long habitId, DateOnly date)
        {
            await _habitLogService.DeleteHabitLogAsync(GetUserId(), habitId, date);
            
            return Ok(new
            {
                success = true,
                message = "Habit log deleted successfully"
            });
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDeleteHabitLogs(long habitId, [FromBody] List<DateOnly> dates)
        {
            if (dates == null || dates.Count == 0)
                return BadRequest(new { message = "No dates provided for deletion" });

            var result = await _habitLogService.BulkDeleteHabitLogsAsync(GetUserId(), habitId, dates);
            
            return Ok(new
            {
                success = true,
                message = $"{result} habit logs deleted successfully",
                deletedCount = result
            });
        }
    }
}
