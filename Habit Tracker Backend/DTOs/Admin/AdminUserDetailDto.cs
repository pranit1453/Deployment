namespace Habit_Tracker_Backend.DTOs.Admin
{
    public class AdminUserDetailDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalHabits { get; set; }
        public int ActiveHabits { get; set; }
    }
}
