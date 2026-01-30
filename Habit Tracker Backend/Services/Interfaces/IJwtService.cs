using Habit_Tracker_Backend.Models.Classes;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
