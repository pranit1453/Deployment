namespace Habit_Tracker_Backend.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message) { }
        public override int StatusCode => StatusCodes.Status401Unauthorized;
    }
}
