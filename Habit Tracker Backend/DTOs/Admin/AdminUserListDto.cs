namespace Habit_Tracker_Backend.DTOs.Admin
{
    public class AdminUserListDto
    {
        public int TotalCount { get; set; }
        public List<AdminUserItemDto> Users { get; set; } = new();
    }

    public class AdminUserItemDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
    }
}
