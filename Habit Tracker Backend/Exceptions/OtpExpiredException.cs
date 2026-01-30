namespace Habit_Tracker_Backend.Exceptions
{
    public class OtpExpiredException : AppException
    {
        public OtpExpiredException()
            : base("OTP has expired", "OTP_EXPIRED") { }

        public override int StatusCode => StatusCodes.Status400BadRequest;
    }
}
