using Twilio.Exceptions;

namespace Habit_Tracker_Backend.Exceptions
{
    public class InvalidOtpException : AppException
    {
        public InvalidOtpException()
            : base("Invalid OTP", "OTP_INVALID") { }

        public override int StatusCode => StatusCodes.Status400BadRequest;
    }
}
