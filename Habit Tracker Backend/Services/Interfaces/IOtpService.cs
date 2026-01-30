using Habit_Tracker_Backend.Models.Classes;
using Habit_Tracker_Backend.Models.Enums;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IOtpService
    {
        Task SendOtpAsync(User user, OtpType type, OtpChannel channel);

        Task ValidateOtpAsync(
            long userId,
            string otp,
            OtpType type
        );
    }
}
