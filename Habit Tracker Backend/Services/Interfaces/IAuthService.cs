using Habit_Tracker_Backend.DTOs;

namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<LoginResultDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> SendVerificationEmailAsync(string email);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto);
    }
}
