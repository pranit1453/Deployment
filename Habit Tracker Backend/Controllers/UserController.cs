using Habit_Tracker_Backend.DTOs.User;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Habit_Tracker_Backend.Controllers
{
    [Authorize(Roles = "USER")]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated");
            return long.Parse(claim.Value);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _userService.GetProfileAsync(GetUserId());
            return Ok(profile);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var profile = await _userService.UpdateProfileAsync(GetUserId(), dto);
            return Ok(profile);
        }

        [HttpPost("toggle-notifications")]
        public async Task<IActionResult> ToggleEmailNotifications([FromBody] bool enabled)
        {
            var result = await _userService.ToggleEmailNotificationsAsync(GetUserId(), enabled);
            return Ok(new
            {
                success = true,
                emailNotificationsEnabled = result,
                message = $"Email notifications {(result ? "enabled" : "disabled")} successfully"
            });
        }
    }
}
