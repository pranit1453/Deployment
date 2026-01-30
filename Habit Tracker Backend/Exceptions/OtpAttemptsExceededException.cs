namespace Habit_Tracker_Backend.Exceptions
{
    public class OtpAttemptsExceededException : AppException
    {
        public OtpAttemptsExceededException()
            : base("OTP attempts exceeded. Please request a new OTP.",
                   "OTP_ATTEMPTS_EXCEEDED")
        { }

        public override int StatusCode => StatusCodes.Status429TooManyRequests;
    }
}
