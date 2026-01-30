using Habit_Tracker_Backend.Data;
using Habit_Tracker_Backend.DTOs.Feedback;
using Habit_Tracker_Backend.Exceptions;
using Habit_Tracker_Backend.Models.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Submit([FromBody] SubmitFeedbackDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Feedback data is required.");

            var entity = new Feedback
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                Rating = dto.Rating,
                Message = dto.Message.Trim()
            };

            _context.Feedback.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Thank you! Your feedback has been submitted successfully."
            });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var feedback = await _context.Feedback
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                feedback = feedback.Select(f => new
                {
                    feedbackId = f.FeedbackId,
                    name = f.Name,
                    email = f.Email,
                    rating = f.Rating,
                    message = f.Message,
                    createdAt = f.CreatedAt
                })
            });
        }
    }
}
