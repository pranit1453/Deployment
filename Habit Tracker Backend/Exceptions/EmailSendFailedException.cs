
namespace Habit_Tracker_Backend.Exceptions
{
    public class EmailSendFailedException : AppException
    {
        public EmailSendFailedException()
            : base("Failed to send email", "EMAIL_SEND_FAILED")
        {
        }

        public override int StatusCode => 500;
    }
}
