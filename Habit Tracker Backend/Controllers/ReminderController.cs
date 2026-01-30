using Habit_Tracker_Backend.Services.Interfaces;
using Habit_Tracker_Backend.DTOs.Reminders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/reminders")]
    [Authorize(Roles = "USER")]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return long.Parse(claim.Value);
        }

        [HttpPost("toggle/{habitId:long}")]
        public async Task<IActionResult> ToggleReminder(long habitId, [FromBody] bool enabled)
        {
            var result = await _reminderService.ToggleReminderAsync(GetUserId(), habitId, enabled);
            
            if (!result)
                return BadRequest(new { message = "Failed to toggle reminder" });

            return Ok(new
            {
                success = true,
                message = $"Reminder {(enabled ? "enabled" : "disabled")} successfully"
            });
        }

        [HttpPost("send-missed-alerts")]
        public async Task<IActionResult> SendMissedAlerts()
        {
            await _reminderService.SendMissedHabitAlertsAsync(GetUserId());
            return Ok(new { message = "Missed habit alerts sent" });
        }

        [HttpPut("update-time/{habitId:long}")]
        public async Task<IActionResult> UpdateReminderTime(long habitId, [FromBody] string timeString)
        {
            if (!TimeSpan.TryParse(timeString, out var reminderTime))
                return BadRequest(new { message = "Invalid time format. Use HH:mm format." });

            var result = await _reminderService.UpdateReminderTimeAsync(GetUserId(), habitId, reminderTime);
            
            if (!result)
                return BadRequest(new { message = "Failed to update reminder time" });

            return Ok(new
            {
                success = true,
                message = "Reminder time updated successfully",
                reminderTime = reminderTime.ToString(@"hh\:mm")
            });
        }

        [HttpGet("{habitId:long}")]
        public async Task<IActionResult> GetReminderSettings(long habitId)
        {
            var settings = await _reminderService.GetReminderSettingsAsync(GetUserId(), habitId);
            
            if (settings == null)
                return NotFound(new { message = "Reminder settings not found" });

            return Ok(settings);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReminders()
        {
            var reminders = await _reminderService.GetAllRemindersAsync(GetUserId());
            return Ok(reminders);
        }

        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdateReminders([FromBody] List<BulkReminderUpdateDto> updates)
        {
            if (updates == null || updates.Count == 0)
                return BadRequest(new { message = "No updates provided" });

            var result = await _reminderService.BulkUpdateRemindersAsync(GetUserId(), updates);
            
            if (!result)
                return BadRequest(new { message = "Failed to update reminders" });

            return Ok(new
            {
                success = true,
                message = "Reminders updated successfully",
                data = result
            });
        }
    }
}
