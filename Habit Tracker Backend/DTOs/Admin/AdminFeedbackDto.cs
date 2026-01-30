namespace Habit_Tracker_Backend.DTOs.Admin
{
    public class AdminFeedbackListDto
    {
        public int TotalCount { get; set; }
        public List<AdminFeedbackItemDto> Feedback { get; set; } = new();
    }

    public class AdminFeedbackItemDto
    {
        public long FeedbackId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Rating { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
