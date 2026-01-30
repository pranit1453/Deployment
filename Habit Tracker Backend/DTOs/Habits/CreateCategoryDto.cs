using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Backend.DTOs.Habits
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }
    }
}
