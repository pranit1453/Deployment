using Habit_Tracker_Backend.DTOs.Habits;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Habit_Tracker_Backend.Controllers
{

    [ApiController]
    [Route("api/habits")]
    [Authorize(Roles = "USER")]
    public class HabitController : ControllerBase
    {
        private readonly IHabitService _habitService;

        public HabitController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return long.Parse(claim.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHabit([FromBody] CreateHabitDto dto)
        {
            var habitId = await _habitService.CreateHabitAsync(GetUserId(), dto);

            return CreatedAtAction(
                nameof(GetHabitById),
                new { habitId },
                new
                {
                    habitId,
                    message = "Habit created successfully"
                });
        }

        [HttpGet]
        public async Task<IActionResult> GetHabits()
        {
            return Ok(await _habitService.GetHabitsAsync(GetUserId()));
        }

        [HttpGet("{habitId:long}")]
        public async Task<IActionResult> GetHabitById(long habitId)
        {
            var habit = await _habitService.GetHabitByIdAsync(GetUserId(), habitId);

            if (habit == null)
                throw new HabitNotFoundException();

            return Ok(habit);
        }

        [HttpPut("{habitId:long}")]
        public async Task<IActionResult> UpdateHabit(long habitId, [FromBody] CreateHabitDto dto)
        {
            await _habitService.UpdateHabitAsync(GetUserId(), habitId, dto);
            return Ok(new
            {
                success = true,
                message = "Habit updated successfully"
            });
        }

        [HttpPatch("{habitId:long}/status")]
        public async Task<IActionResult> ToggleHabitStatus(long habitId)
        {
            await _habitService.ToggleHabitStatusAsync(GetUserId(), habitId);
            return Ok(new
            {
                success = true,
                message = "Habit status updated successfully"
            });
        }

        [HttpPost("{habitId:long}/log")]
        public async Task<IActionResult> SetHabitDailyStatus(
          long habitId,
          [FromBody] HabitDailyStatusDto dto)
        {
            await _habitService.SetHabitDailyStatusAsync(
                GetUserId(),
                habitId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                dto.Status);

            return Ok(new
            {
                success = true,
                message = $"Habit marked as {dto.Status}"
            });
        }
        [HttpDelete("{habitId:long}")]
        public async Task<IActionResult> DeleteHabit(long habitId)
        {
            await _habitService.DeleteHabitAsync(GetUserId(), habitId);
            return Ok(new
            {
                success = true,
                message = "Habit deleted successfully"
            });
        }
    }

}
