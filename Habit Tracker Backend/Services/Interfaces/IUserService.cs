using Habit_Tracker_Backend.DTOs.User;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponseDto> GetProfileAsync(long userId);
        Task<UserProfileResponseDto> UpdateProfileAsync(long userId, UpdateProfileDto dto);
        Task<bool> ToggleEmailNotificationsAsync(long userId, bool enabled);
    }
}
