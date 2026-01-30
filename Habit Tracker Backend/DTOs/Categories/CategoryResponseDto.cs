namespace Habit_Tracker_Backend.DTOs.Categories
{
    public class CategoryResponseDto
    {
        public long CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
