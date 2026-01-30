namespace Habit_Tracker_Backend.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = null!;
    }
}
