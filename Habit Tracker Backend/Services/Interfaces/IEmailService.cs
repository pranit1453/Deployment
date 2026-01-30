namespace Habit_Tracker_Backend.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
